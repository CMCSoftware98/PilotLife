#pragma once

// Must include winsock2.h before windows.h
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <windows.h>
#include <SimConnect.h>
#include <functional>
#include <atomic>
#include <thread>
#include <string>
#include "FlightData.h"

class SimConnectManager {
public:
    using FlightDataCallback = std::function<void(const FlightDataJson&)>;
    using StatusCallback = std::function<void(const SimulatorStatus&)>;

    SimConnectManager();
    ~SimConnectManager();

    // Connect to SimConnect
    bool connect(const std::string& appName = "PilotLife.Connector");

    // Disconnect from SimConnect
    void disconnect();

    // Check if connected
    bool isConnected() const { return m_connected; }

    // Set the simulator version string (for JSON output)
    void setSimulatorVersion(const std::string& version) { m_simulatorVersion = version; }

    // Set callbacks
    void setFlightDataCallback(FlightDataCallback callback) { m_flightDataCallback = callback; }
    void setStatusCallback(StatusCallback callback) { m_statusCallback = callback; }

    // Start/stop the dispatch loop
    void startDispatchLoop();
    void stopDispatchLoop();

private:
    // SimConnect handle
    HANDLE m_hSimConnect = nullptr;
    std::atomic<bool> m_connected{false};
    std::atomic<bool> m_running{false};
    std::thread m_dispatchThread;
    std::string m_simulatorVersion;

    // Callbacks
    FlightDataCallback m_flightDataCallback;
    StatusCallback m_statusCallback;

    // Internal methods
    void setupDataDefinitions();
    void requestPeriodicData();
    void dispatchLoop();

    // Static dispatch callback
    static void CALLBACK dispatchProc(
        SIMCONNECT_RECV* pData,
        DWORD cbData,
        void* pContext
    );

    // Handle specific message types
    void handleSimObjectData(SIMCONNECT_RECV_SIMOBJECT_DATA* pObjData);
    void handleOpen(SIMCONNECT_RECV_OPEN* pOpen);
    void handleQuit();
    void handleException(SIMCONNECT_RECV_EXCEPTION* pException);
};
