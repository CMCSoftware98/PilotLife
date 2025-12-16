// Must include winsock2.h before windows.h to avoid conflicts
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>

#include <iostream>
#include <chrono>
#include <thread>
#include <string>
#include <cstring>
#include <csignal>
#include <atomic>

// Include WebSocketServer first (uses winsock2)
#include "WebSocketServer.h"
// Then SimConnect (which may include old winsock)
#include "SimConnectManager.h"
#include "ProcessDetector.h"
#include "FlightData.h"
#include "AircraftIndexer.h"
#include <IXNetSystem.h>

// Configuration
constexpr int DEFAULT_PORT = 5050;
constexpr int PROCESS_CHECK_INTERVAL_MS = 10000;  // 10 seconds

// Global flag for graceful shutdown
std::atomic<bool> g_running{true};

void signalHandler(int signal) {
    std::cout << "\nShutdown signal received..." << std::endl;
    g_running = false;
}

int parsePort(int argc, char* argv[]) {
    int port = DEFAULT_PORT;

    for (int i = 1; i < argc; i++) {
        if ((strcmp(argv[i], "--port") == 0 || strcmp(argv[i], "-p") == 0) && i + 1 < argc) {
            port = std::atoi(argv[i + 1]);
            if (port <= 0 || port > 65535) {
                std::cerr << "Invalid port number: " << argv[i + 1] << ". Using default: " << DEFAULT_PORT << std::endl;
                port = DEFAULT_PORT;
            }
            break;
        }
    }

    return port;
}

void printUsage(const char* programName) {
    std::cout << "Usage: " << programName << " [options]" << std::endl;
    std::cout << "Options:" << std::endl;
    std::cout << "  --port, -p <port>  WebSocket server port (default: " << DEFAULT_PORT << ")" << std::endl;
    std::cout << "  --help, -h         Show this help message" << std::endl;
}

int main(int argc, char* argv[]) {
    // Check for help flag
    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "--help") == 0 || strcmp(argv[i], "-h") == 0) {
            printUsage(argv[0]);
            return 0;
        }
    }

    // Initialize network system (required for IXWebSocket on Windows)
    if (!ix::initNetSystem()) {
        std::cerr << "Failed to initialize network system" << std::endl;
        return 1;
    }

    // Set up signal handler for graceful shutdown
    std::signal(SIGINT, signalHandler);
    std::signal(SIGTERM, signalHandler);

    // Parse command line arguments
    int port = parsePort(argc, argv);

    std::cout << "========================================" << std::endl;
    std::cout << "  PilotLife.Connector" << std::endl;
    std::cout << "  SimConnect Flight Data Bridge" << std::endl;
    std::cout << "========================================" << std::endl;
    std::cout << "WebSocket port: " << port << std::endl;
    std::cout << std::endl;

    // Initialize WebSocket server
    WebSocketServer wsServer(port);
    if (!wsServer.start()) {
        std::cerr << "Failed to start WebSocket server on port " << port << std::endl;
        return 1;
    }

    // Initialize Aircraft Indexer for file data
    std::cout << "Scanning for aircraft packages..." << std::endl;
    AircraftIndexer aircraftIndexer;
    if (aircraftIndexer.initialize()) {
        std::cout << "Indexed " << aircraftIndexer.getIndexedCount() << " aircraft variants" << std::endl;
    } else {
        std::cout << "Warning: Could not index aircraft packages. File data will not be available." << std::endl;
    }

    // Set up message handler for aircraft data requests
    wsServer.setMessageHandler([&aircraftIndexer](const std::string& message, ix::WebSocket& client) -> std::string {
        std::cout << "Received: " << message << std::endl;

        // Simple JSON parsing for incoming requests
        // Looking for: {"type":"getAircraftData","requestId":"...","aircraftTitle":"..."}

        size_t typePos = message.find("\"type\"");
        if (typePos == std::string::npos) {
            std::cout << "No type field found" << std::endl;
            return "";
        }

        // Extract type value
        size_t typeValueStart = message.find("\"getAircraftData\"");
        if (typeValueStart == std::string::npos) {
            std::cout << "Not a getAircraftData request" << std::endl;
            return "";
        }

        // Extract requestId
        std::string requestId;
        size_t requestIdPos = message.find("\"requestId\"");
        if (requestIdPos != std::string::npos) {
            size_t colonPos = message.find(':', requestIdPos);
            size_t startQuote = message.find('"', colonPos);
            size_t endQuote = message.find('"', startQuote + 1);
            if (startQuote != std::string::npos && endQuote != std::string::npos) {
                requestId = message.substr(startQuote + 1, endQuote - startQuote - 1);
            }
        }
        std::cout << "RequestId: " << requestId << std::endl;

        // Extract aircraftTitle
        std::string aircraftTitle;
        size_t titlePos = message.find("\"aircraftTitle\"");
        if (titlePos != std::string::npos) {
            size_t colonPos = message.find(':', titlePos);
            size_t startQuote = message.find('"', colonPos);
            size_t endQuote = message.find('"', startQuote + 1);
            if (startQuote != std::string::npos && endQuote != std::string::npos) {
                aircraftTitle = message.substr(startQuote + 1, endQuote - startQuote - 1);
            }
        }
        std::cout << "AircraftTitle: " << aircraftTitle << std::endl;

        if (aircraftTitle.empty()) {
            std::cout << "Empty aircraft title, returning not found" << std::endl;
            return AircraftIndexer::toNotFoundResponse(requestId);
        }

        // Look up the aircraft
        std::cout << "Looking up aircraft..." << std::endl;
        auto result = aircraftIndexer.findByTitle(aircraftTitle);
        if (result.has_value()) {
            std::cout << "Found aircraft data for: " << aircraftTitle << std::endl;
            return AircraftIndexer::toJsonResponse(result.value(), requestId);
        } else {
            std::cout << "Aircraft not found: " << aircraftTitle << std::endl;
            return AircraftIndexer::toNotFoundResponse(requestId);
        }
    });

    // Initialize SimConnect manager
    SimConnectManager simConnect;

    // Track current sim status for sending to new clients
    std::atomic<bool> simIsConnected{false};
    std::atomic<bool> simIsRunning{false};
    std::string currentSimVersion;
    std::mutex simVersionMutex;

    // Set up callbacks
    simConnect.setFlightDataCallback([&wsServer](const FlightDataJson& data) {
        std::string json = "{\"type\":\"flightData\",\"data\":" + data.toJson() + "}";
        wsServer.broadcast(json);
    });

    simConnect.setStatusCallback([&wsServer](const SimulatorStatus& status) {
        std::string json = "{\"type\":\"status\",\"data\":" + status.toJson() + "}";
        wsServer.broadcast(json);
    });

    // When a new client connects, send them the current status and MSFS paths info
    wsServer.setClientConnectedCallback([&simIsConnected, &simIsRunning, &currentSimVersion, &simVersionMutex, &aircraftIndexer](ix::WebSocket& client) {
        // Send simulator status
        SimulatorStatus status;
        status.isConnected = simIsConnected.load();
        status.isSimRunning = simIsRunning.load();
        {
            std::lock_guard<std::mutex> lock(simVersionMutex);
            status.simulatorVersion = currentSimVersion;
        }
        std::string statusJson = "{\"type\":\"status\",\"data\":" + status.toJson() + "}";
        client.send(statusJson);

        // Send MSFS paths info
        std::string pathsJson = aircraftIndexer.toPathsInfoResponse();
        client.send(pathsJson);
    });

    // Main loop - detect MSFS process and connect
    bool wasConnected = false;
    SimulatorType lastDetectedType = SimulatorType::None;

    std::cout << "Waiting for Microsoft Flight Simulator..." << std::endl;

    while (g_running) {
        SimulatorType simType = ProcessDetector::detectMSFS();

        if (simType != SimulatorType::None && !simConnect.isConnected()) {
            std::string simVersion = ProcessDetector::getSimulatorTypeString(simType);
            std::cout << simVersion << " detected, attempting connection..." << std::endl;

            simConnect.setSimulatorVersion(simVersion);

            if (simConnect.connect()) {
                std::cout << "Connected to SimConnect!" << std::endl;
                simConnect.startDispatchLoop();
                wasConnected = true;
                lastDetectedType = simType;

                // Update tracked state
                simIsConnected = true;
                simIsRunning = true;
                {
                    std::lock_guard<std::mutex> lock(simVersionMutex);
                    currentSimVersion = simVersion;
                }

                // Broadcast connected status
                SimulatorStatus status;
                status.isConnected = true;
                status.isSimRunning = true;
                status.simulatorVersion = simVersion;
                wsServer.broadcast("{\"type\":\"status\",\"data\":" + status.toJson() + "}");
            } else {
                std::cerr << "Failed to connect to SimConnect" << std::endl;
            }
        }
        else if (simType == SimulatorType::None && wasConnected) {
            std::cout << "MSFS closed, disconnecting..." << std::endl;
            simConnect.stopDispatchLoop();
            simConnect.disconnect();
            wasConnected = false;

            // Update tracked state
            simIsConnected = false;
            simIsRunning = false;

            // Broadcast disconnected status
            SimulatorStatus status;
            status.isConnected = false;
            status.isSimRunning = false;
            status.simulatorVersion = ProcessDetector::getSimulatorTypeString(lastDetectedType);
            wsServer.broadcast("{\"type\":\"status\",\"data\":" + status.toJson() + "}");

            std::cout << "Waiting for Microsoft Flight Simulator..." << std::endl;
        }

        // Wait before next check
        for (int i = 0; i < PROCESS_CHECK_INTERVAL_MS / 100 && g_running; i++) {
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
        }
    }

    // Cleanup
    std::cout << "Shutting down..." << std::endl;
    simConnect.stopDispatchLoop();
    simConnect.disconnect();
    wsServer.stop();

    // Cleanup network system
    ix::uninitNetSystem();

    std::cout << "Goodbye!" << std::endl;
    return 0;
}
