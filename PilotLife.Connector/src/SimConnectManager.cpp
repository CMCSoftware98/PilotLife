#include "SimConnectManager.h"
#include <iostream>

SimConnectManager::SimConnectManager() {}

SimConnectManager::~SimConnectManager() {
    stopDispatchLoop();
    disconnect();
}

bool SimConnectManager::connect(const std::string& appName) {
    if (m_connected) {
        return true;
    }

    HRESULT hr = SimConnect_Open(
        &m_hSimConnect,
        appName.c_str(),
        nullptr,    // hWnd - not using window messages
        0,          // UserEventWin32
        nullptr,    // hEventHandle
        0           // ConfigIndex - use default
    );

    if (SUCCEEDED(hr)) {
        m_connected = true;
        setupDataDefinitions();
        requestPeriodicData();
        return true;
    }

    std::cerr << "Failed to connect to SimConnect. HRESULT: " << hr << std::endl;
    return false;
}

void SimConnectManager::disconnect() {
    if (m_hSimConnect) {
        SimConnect_Close(m_hSimConnect);
        m_hSimConnect = nullptr;
        m_connected = false;
    }
}

void SimConnectManager::setupDataDefinitions() {
    // Define all the SimVars we want to receive
    // Order MUST match SimConnectFlightData struct in FlightData.h

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "TITLE", nullptr, SIMCONNECT_DATATYPE_STRING256);

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "PLANE LATITUDE", "degrees");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "PLANE LONGITUDE", "degrees");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "INDICATED ALTITUDE", "feet");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "PLANE ALTITUDE", "feet");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "PLANE ALT ABOVE GROUND", "feet");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "AIRSPEED INDICATED", "knots");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "AIRSPEED TRUE", "knots");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "GROUND VELOCITY", "knots");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "AIRSPEED MACH", "mach");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "HEADING INDICATOR", "degrees");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "PLANE HEADING DEGREES TRUE", "degrees");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "GPS GROUND TRUE TRACK", "degrees");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "FUEL TOTAL QUANTITY", "gallons");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "FUEL WEIGHT PER GALLON", "pounds");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "TOTAL WEIGHT", "pounds");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "EMPTY WEIGHT", "pounds");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "COM ACTIVE FREQUENCY:1", "Hz");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "COM ACTIVE FREQUENCY:2", "Hz");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "NAV ACTIVE FREQUENCY:1", "Hz");

    SimConnect_AddToDataDefinition(m_hSimConnect, DEFINITION_FLIGHT_DATA,
        "NAV ACTIVE FREQUENCY:2", "Hz");
}

void SimConnectManager::requestPeriodicData() {
    // Request data every 5 seconds
    // SIMCONNECT_PERIOD_SECOND requests once per sim second
    // We use interval of 5 to get data every 5 sim seconds
    SimConnect_RequestDataOnSimObject(
        m_hSimConnect,
        REQUEST_FLIGHT_DATA,
        DEFINITION_FLIGHT_DATA,
        SIMCONNECT_OBJECT_ID_USER,
        SIMCONNECT_PERIOD_SECOND,
        SIMCONNECT_DATA_REQUEST_FLAG_DEFAULT,
        0,      // origin
        5,      // interval (every 5 seconds)
        0       // limit (0 = no limit)
    );
}

void SimConnectManager::startDispatchLoop() {
    m_running = true;
    m_dispatchThread = std::thread(&SimConnectManager::dispatchLoop, this);
}

void SimConnectManager::stopDispatchLoop() {
    m_running = false;
    if (m_dispatchThread.joinable()) {
        m_dispatchThread.join();
    }
}

void SimConnectManager::dispatchLoop() {
    while (m_running && m_connected) {
        SimConnect_CallDispatch(m_hSimConnect, dispatchProc, this);
        Sleep(10); // Small delay to prevent CPU spinning
    }
}

void CALLBACK SimConnectManager::dispatchProc(
    SIMCONNECT_RECV* pData,
    DWORD cbData,
    void* pContext
) {
    SimConnectManager* self = static_cast<SimConnectManager*>(pContext);

    switch (pData->dwID) {
        case SIMCONNECT_RECV_ID_OPEN:
            self->handleOpen(reinterpret_cast<SIMCONNECT_RECV_OPEN*>(pData));
            break;

        case SIMCONNECT_RECV_ID_QUIT:
            self->handleQuit();
            break;

        case SIMCONNECT_RECV_ID_SIMOBJECT_DATA:
            self->handleSimObjectData(
                reinterpret_cast<SIMCONNECT_RECV_SIMOBJECT_DATA*>(pData));
            break;

        case SIMCONNECT_RECV_ID_EXCEPTION:
            self->handleException(
                reinterpret_cast<SIMCONNECT_RECV_EXCEPTION*>(pData));
            break;

        default:
            break;
    }
}

void SimConnectManager::handleSimObjectData(SIMCONNECT_RECV_SIMOBJECT_DATA* pObjData) {
    if (pObjData->dwRequestID == REQUEST_FLIGHT_DATA) {
        SimConnectFlightData* pFlightData =
            reinterpret_cast<SimConnectFlightData*>(&pObjData->dwData);

        if (m_flightDataCallback) {
            FlightDataJson jsonData = FlightDataJson::fromSimConnect(*pFlightData, m_simulatorVersion);
            m_flightDataCallback(jsonData);
        }
    }
}

void SimConnectManager::handleOpen(SIMCONNECT_RECV_OPEN* pOpen) {
    std::cout << "Connected to: " << pOpen->szApplicationName << std::endl;
    std::cout << "SimConnect version: " << pOpen->dwSimConnectVersionMajor
              << "." << pOpen->dwSimConnectVersionMinor << std::endl;

    if (m_statusCallback) {
        SimulatorStatus status;
        status.isConnected = true;
        status.isSimRunning = true;
        status.simulatorVersion = m_simulatorVersion;
        m_statusCallback(status);
    }
}

void SimConnectManager::handleQuit() {
    std::cout << "Simulator closed" << std::endl;
    m_connected = false;

    if (m_statusCallback) {
        SimulatorStatus status;
        status.isConnected = false;
        status.isSimRunning = false;
        status.simulatorVersion = m_simulatorVersion;
        status.connectionError = "Simulator closed";
        m_statusCallback(status);
    }
}

void SimConnectManager::handleException(SIMCONNECT_RECV_EXCEPTION* pException) {
    std::cerr << "SimConnect Exception: " << pException->dwException
              << " (SendID: " << pException->dwSendID
              << ", Index: " << pException->dwIndex << ")" << std::endl;
}
