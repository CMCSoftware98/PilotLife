#pragma once

// Must include winsock2.h before windows.h
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <windows.h>
#include <string>
#include <ctime>
#include <iomanip>
#include <sstream>

// Forward declaration for SimConnect types
struct SimConnectFlightData;

// SimConnect Data Definition IDs
enum DATA_DEFINE_ID {
    DEFINITION_FLIGHT_DATA = 0
};

// SimConnect Request IDs
enum DATA_REQUEST_ID {
    REQUEST_FLIGHT_DATA = 0
};

// Structure must match the order of AddToDataDefinition calls
// Uses #pragma pack to ensure proper memory alignment with SimConnect
#pragma pack(push, 1)
struct SimConnectFlightData {
    char title[256];              // TITLE
    char atcType[64];             // ATC TYPE (aircraft type e.g., "Boeing 737")
    char atcModel[64];            // ATC MODEL (model variant e.g., "B738")
    char atcId[64];               // ATC ID (tail number e.g., "N12345")
    char atcAirline[64];          // ATC AIRLINE (airline name)
    char atcFlightNumber[64];     // ATC FLIGHT NUMBER
    char category[256];           // CATEGORY (e.g., "Airplane", "Helicopter")
    int engineType;               // ENGINE TYPE (0=Piston, 1=Jet, 2=None, 3=Helo, 4=Rocket, 5=Turboprop)
    int numberOfEngines;          // NUMBER OF ENGINES
    double maxGrossWeight;        // MAX GROSS WEIGHT (pounds)
    double cruiseSpeed;           // DESIGN CRUISE ALT (knots)
    double latitude;              // PLANE LATITUDE (degrees)
    double longitude;             // PLANE LONGITUDE (degrees)
    double altitudeIndicated;     // INDICATED ALTITUDE (feet)
    double altitudeTrue;          // PLANE ALTITUDE (feet)
    double altitudeAGL;           // PLANE ALT ABOVE GROUND (feet)
    double airspeedIndicated;     // AIRSPEED INDICATED (knots)
    double airspeedTrue;          // AIRSPEED TRUE (knots)
    double groundSpeed;           // GROUND VELOCITY (knots)
    double machNumber;            // AIRSPEED MACH
    double headingMagnetic;       // HEADING INDICATOR (degrees)
    double headingTrue;           // PLANE HEADING DEGREES TRUE (degrees)
    double gpsGroundTrack;        // GPS GROUND TRUE TRACK (degrees)
    double fuelTotalQuantity;     // FUEL TOTAL QUANTITY (gallons)
    double fuelWeightPerGallon;   // FUEL WEIGHT PER GALLON (pounds)
    double totalWeight;           // TOTAL WEIGHT (pounds)
    double emptyWeight;           // EMPTY WEIGHT (pounds)
    int com1ActiveFreq;           // COM ACTIVE FREQUENCY:1 (Hz)
    int com2ActiveFreq;           // COM ACTIVE FREQUENCY:2 (Hz)
    int nav1ActiveFreq;           // NAV ACTIVE FREQUENCY:1 (Hz)
    int nav2ActiveFreq;           // NAV ACTIVE FREQUENCY:2 (Hz)
};
#pragma pack(pop)

// JSON-serializable flight data for WebSocket transmission
struct FlightDataJson {
    // Aircraft metadata
    std::string aircraftTitle;
    std::string atcType;
    std::string atcModel;
    std::string atcId;
    std::string atcAirline;
    std::string atcFlightNumber;
    std::string category;
    std::string engineTypeStr;
    int engineType;
    int numberOfEngines;
    double maxGrossWeightLbs;
    double cruiseSpeedKts;
    double emptyWeightLbs;

    // Position
    double latitude;
    double longitude;
    double altitudeIndicated;
    double altitudeTrue;
    double altitudeAGL;

    // Speed
    double airspeedIndicated;
    double airspeedTrue;
    double groundSpeed;
    double machNumber;

    // Heading
    double headingMagnetic;
    double headingTrue;
    double track;

    // Weight & Fuel
    double fuelLbs;
    double fuelKgs;
    double payloadLbs;
    double payloadKgs;
    double totalWeightLbs;
    double totalWeightKgs;

    // Radios
    std::string com1Frequency;
    std::string com2Frequency;
    std::string nav1Frequency;
    std::string nav2Frequency;

    // Metadata
    std::string timestamp;
    std::string simulatorVersion;

    // Convert from SimConnect struct
    static FlightDataJson fromSimConnect(const SimConnectFlightData& data, const std::string& simVersion);

    // Serialize to JSON string
    std::string toJson() const;

private:
    // Helper to format frequency from Hz to MHz string (e.g., 118700000 -> "118.700")
    static std::string formatFrequency(int freqHz);

    // Helper to get current UTC timestamp in ISO 8601 format
    static std::string getCurrentTimestamp();
};

// Simulator connection status
struct SimulatorStatus {
    bool isConnected;
    bool isSimRunning;
    std::string simulatorVersion;  // "MSFS2020" or "MSFS2024"
    std::string connectionError;

    std::string toJson() const;
};
