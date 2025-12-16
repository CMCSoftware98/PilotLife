import { invoke } from '@tauri-apps/api/core';

export interface FlightData {
    // Aircraft metadata
    aircraftTitle: string;
    atcType: string;
    atcModel: string;
    atcId: string;
    atcAirline: string;
    atcFlightNumber: string;
    category: string;
    engineType: number;
    engineTypeStr: string;
    numberOfEngines: number;
    maxGrossWeightLbs: number;
    cruiseSpeedKts: number;
    emptyWeightLbs: number;
    // Position
    latitude: number;
    longitude: number;
    altitudeIndicated: number;
    altitudeTrue: number;
    altitudeAGL: number;
    // Speed
    airspeedIndicated: number;
    airspeedTrue: number;
    groundSpeed: number;
    machNumber: number;
    // Heading
    headingMagnetic: number;
    headingTrue: number;
    track: number;
    // Weight & Fuel
    fuelLbs: number;
    fuelKgs: number;
    payloadLbs: number;
    payloadKgs: number;
    totalWeightLbs: number;
    totalWeightKgs: number;
    // Radios
    com1Frequency: string;
    com2Frequency: string;
    nav1Frequency: string;
    nav2Frequency: string;
    // Metadata
    timestamp: string;
    simulatorVersion: string;
}

export interface SimulatorStatus {
    isConnected: boolean;
    isSimRunning: boolean;
    simulatorVersion: string;
    connectionError?: string;
}

export interface AircraftFileData {
    found: boolean;
    requestId: string;
    manifest?: {
        packagePath: string;
        contentType: string;
        title: string;
        manufacturer: string;
        creator: string;
        packageVersion: string;
        minimumGameVersion: string;
        totalPackageSize: string;
        contentId: string;
        rawJson: string;
    };
    aircraftCfg?: {
        // [FLTSIM.0] section
        title: string;
        model: string;
        panel: string;
        sound: string;
        texture: string;
        atcType: string;
        atcModel: string;
        atcId: string;
        atcAirline: string;
        uiManufacturer: string;
        uiType: string;
        uiVariation: string;
        icaoAirline: string;
        // [GENERAL] section
        generalAtcType: string;
        generalAtcModel: string;
        editable: string;
        performance: string;
        category: string;
        rawContent: string;
    };
}

export interface MSFSPathsInfo {
    userCfgOptPath: string;
    configFilePath: string;
    indexedAircraftCount: number;
    searchPaths: string[];
}

type FlightDataHandler = (data: FlightData) => void;
type StatusHandler = (status: SimulatorStatus) => void;
type ConnectionHandler = (connected: boolean) => void;
type MSFSPathsHandler = (paths: MSFSPathsInfo) => void;

type AircraftDataResponseHandler = (data: AircraftFileData) => void;

class ConnectorService {
    private ws: WebSocket | null = null;
    private port: number | null = null;
    private flightDataHandlers: FlightDataHandler[] = [];
    private statusHandlers: StatusHandler[] = [];
    private connectionHandlers: ConnectionHandler[] = [];
    private msfsPathsHandlers: MSFSPathsHandler[] = [];
    private reconnectTimeout: ReturnType<typeof setTimeout> | null = null;
    private isStarted = false;
    private pendingAircraftDataRequests: Map<string, AircraftDataResponseHandler> = new Map();
    private currentMSFSPaths: MSFSPathsInfo | null = null;

    /**
     * Start the connector process and establish WebSocket connection
     */
    async start(): Promise<void> {
        if (this.isStarted) {
            console.warn('Connector already started');
            return;
        }

        try {
            // Start connector and get port from Tauri
            this.port = await invoke<number>('start_connector');
            this.isStarted = true;
            console.log(`Connector started on port ${this.port}`);

            // Small delay to allow the connector to fully start
            await new Promise(resolve => setTimeout(resolve, 500));

            this.connect();
        } catch (error) {
            console.error('Failed to start connector:', error);
            throw error;
        }
    }

    /**
     * Stop the connector and close WebSocket connection
     */
    async stop(): Promise<void> {
        this.clearReconnectTimeout();

        if (this.ws) {
            this.ws.close();
            this.ws = null;
        }

        if (this.isStarted) {
            try {
                await invoke('stop_connector');
            } catch (error) {
                console.error('Failed to stop connector:', error);
            }
            this.isStarted = false;
            this.port = null;
        }
    }

    /**
     * Check if the connector is running
     */
    isRunning(): boolean {
        return this.isStarted && this.ws?.readyState === WebSocket.OPEN;
    }

    /**
     * Get the current port (if connected)
     */
    getPort(): number | null {
        return this.port;
    }

    /**
     * Subscribe to flight data updates
     * @returns Unsubscribe function
     */
    onFlightData(handler: FlightDataHandler): () => void {
        this.flightDataHandlers.push(handler);
        return () => {
            const idx = this.flightDataHandlers.indexOf(handler);
            if (idx > -1) this.flightDataHandlers.splice(idx, 1);
        };
    }

    /**
     * Subscribe to status updates
     * @returns Unsubscribe function
     */
    onStatus(handler: StatusHandler): () => void {
        this.statusHandlers.push(handler);
        return () => {
            const idx = this.statusHandlers.indexOf(handler);
            if (idx > -1) this.statusHandlers.splice(idx, 1);
        };
    }

    /**
     * Subscribe to WebSocket connection status updates
     * @returns Unsubscribe function
     */
    onConnection(handler: ConnectionHandler): () => void {
        this.connectionHandlers.push(handler);
        return () => {
            const idx = this.connectionHandlers.indexOf(handler);
            if (idx > -1) this.connectionHandlers.splice(idx, 1);
        };
    }

    /**
     * Subscribe to MSFS paths info updates
     * @returns Unsubscribe function
     */
    onMSFSPaths(handler: MSFSPathsHandler): () => void {
        this.msfsPathsHandlers.push(handler);
        // If we already have paths info, call the handler immediately
        if (this.currentMSFSPaths) {
            handler(this.currentMSFSPaths);
        }
        return () => {
            const idx = this.msfsPathsHandlers.indexOf(handler);
            if (idx > -1) this.msfsPathsHandlers.splice(idx, 1);
        };
    }

    /**
     * Get current MSFS paths info (if available)
     */
    getMSFSPaths(): MSFSPathsInfo | null {
        return this.currentMSFSPaths;
    }

    /**
     * Request aircraft file data (manifest.json and aircraft.cfg) from the connector
     * @param aircraftTitle The title of the aircraft to look up
     * @param timeout Timeout in milliseconds (default 10000)
     * @returns Promise resolving to the aircraft file data
     */
    requestAircraftData(aircraftTitle: string, timeout = 10000): Promise<AircraftFileData> {
        return new Promise((resolve, reject) => {
            if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
                reject(new Error('WebSocket not connected'));
                return;
            }

            const requestId = crypto.randomUUID();
            const timeoutId = setTimeout(() => {
                this.pendingAircraftDataRequests.delete(requestId);
                reject(new Error('Request timed out'));
            }, timeout);

            this.pendingAircraftDataRequests.set(requestId, (data) => {
                clearTimeout(timeoutId);
                resolve(data);
            });

            const request = {
                type: 'getAircraftData',
                requestId,
                aircraftTitle
            };

            this.ws.send(JSON.stringify(request));
        });
    }

    private notifyConnectionStatus(connected: boolean): void {
        this.connectionHandlers.forEach(h => h(connected));
    }

    private connect(): void {
        if (!this.port || !this.isStarted) {
            console.warn('Cannot connect: connector not started or no port');
            return;
        }

        if (this.ws?.readyState === WebSocket.OPEN) {
            console.warn('WebSocket already connected');
            return;
        }

        const url = `ws://127.0.0.1:${this.port}`;
        console.log(`Connecting to WebSocket at ${url}`);

        this.ws = new WebSocket(url);

        this.ws.onopen = () => {
            console.log('WebSocket connected to PilotLife.Connector');
            this.clearReconnectTimeout();
            this.notifyConnectionStatus(true);
        };

        this.ws.onmessage = (event) => {
            try {
                const message = JSON.parse(event.data);

                if (message.type === 'flightData') {
                    this.flightDataHandlers.forEach(h => h(message.data));
                } else if (message.type === 'status') {
                    this.statusHandlers.forEach(h => h(message.data));
                } else if (message.type === 'msfsPaths') {
                    // Handle MSFS paths info
                    const pathsInfo: MSFSPathsInfo = {
                        userCfgOptPath: message.data?.userCfgOptPath || '',
                        configFilePath: message.data?.configFilePath || '',
                        indexedAircraftCount: message.data?.indexedAircraftCount || 0,
                        searchPaths: message.data?.searchPaths || []
                    };
                    this.currentMSFSPaths = pathsInfo;
                    this.msfsPathsHandlers.forEach(h => h(pathsInfo));
                } else if (message.type === 'aircraftDataResponse') {
                    // Handle aircraft file data response
                    const requestId = message.requestId as string;
                    const handler = this.pendingAircraftDataRequests.get(requestId);
                    if (handler) {
                        // Transform the response to match AircraftFileData interface
                        const data: AircraftFileData = {
                            found: message.data?.found ?? false,
                            requestId: requestId,
                            manifest: message.data?.manifest ? {
                                packagePath: message.data.manifest.packagePath || '',
                                contentType: message.data.manifest.contentType || '',
                                title: message.data.manifest.title || '',
                                manufacturer: message.data.manifest.manufacturer || '',
                                creator: message.data.manifest.creator || '',
                                packageVersion: message.data.manifest.packageVersion || '',
                                minimumGameVersion: message.data.manifest.minimumGameVersion || '',
                                totalPackageSize: message.data.manifest.totalPackageSize || '',
                                contentId: message.data.manifest.contentId || '',
                                rawJson: message.data.manifest.raw || ''
                            } : undefined,
                            aircraftCfg: message.data?.config ? {
                                title: message.data.config.title || '',
                                model: message.data.config.model || '',
                                panel: message.data.config.panel || '',
                                sound: message.data.config.sound || '',
                                texture: message.data.config.texture || '',
                                atcType: message.data.config.atcType || '',
                                atcModel: message.data.config.atcModel || '',
                                atcId: message.data.config.atcId || '',
                                atcAirline: message.data.config.atcAirline || '',
                                uiManufacturer: message.data.config.uiManufacturer || '',
                                uiType: message.data.config.uiType || '',
                                uiVariation: message.data.config.uiVariation || '',
                                icaoAirline: message.data.config.icaoAirline || '',
                                generalAtcType: message.data.config.generalAtcType || '',
                                generalAtcModel: message.data.config.generalAtcModel || '',
                                editable: message.data.config.editable || '',
                                performance: message.data.config.performance || '',
                                category: message.data.config.category || '',
                                rawContent: message.data.config.raw || ''
                            } : undefined
                        };
                        handler(data);
                        this.pendingAircraftDataRequests.delete(requestId);
                    }
                }
            } catch (error) {
                console.error('Failed to parse WebSocket message:', error);
            }
        };

        this.ws.onclose = (event) => {
            console.log(`WebSocket disconnected: ${event.code} - ${event.reason}`);
            this.notifyConnectionStatus(false);

            // Only reconnect if we're still supposed to be running
            if (this.isStarted) {
                this.scheduleReconnect();
            }
        };

        this.ws.onerror = (error) => {
            console.error('WebSocket error:', error);
        };
    }

    private scheduleReconnect(): void {
        this.clearReconnectTimeout();

        // Attempt reconnection after 5 seconds
        this.reconnectTimeout = setTimeout(() => {
            console.log('Attempting to reconnect...');
            this.connect();
        }, 5000);
    }

    private clearReconnectTimeout(): void {
        if (this.reconnectTimeout) {
            clearTimeout(this.reconnectTimeout);
            this.reconnectTimeout = null;
        }
    }
}

// Export singleton instance
export const connector = new ConnectorService();
