import { invoke } from '@tauri-apps/api/core';

export interface FlightData {
    aircraftTitle: string;
    latitude: number;
    longitude: number;
    altitudeIndicated: number;
    altitudeTrue: number;
    altitudeAGL: number;
    airspeedIndicated: number;
    airspeedTrue: number;
    groundSpeed: number;
    machNumber: number;
    headingMagnetic: number;
    headingTrue: number;
    track: number;
    fuelLbs: number;
    fuelKgs: number;
    payloadLbs: number;
    payloadKgs: number;
    totalWeightLbs: number;
    totalWeightKgs: number;
    com1Frequency: string;
    com2Frequency: string;
    nav1Frequency: string;
    nav2Frequency: string;
    timestamp: string;
    simulatorVersion: string;
}

export interface SimulatorStatus {
    isConnected: boolean;
    isSimRunning: boolean;
    simulatorVersion: string;
    connectionError?: string;
}

type FlightDataHandler = (data: FlightData) => void;
type StatusHandler = (status: SimulatorStatus) => void;
type ConnectionHandler = (connected: boolean) => void;

class ConnectorService {
    private ws: WebSocket | null = null;
    private port: number | null = null;
    private flightDataHandlers: FlightDataHandler[] = [];
    private statusHandlers: StatusHandler[] = [];
    private connectionHandlers: ConnectionHandler[] = [];
    private reconnectTimeout: ReturnType<typeof setTimeout> | null = null;
    private isStarted = false;

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
