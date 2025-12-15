#include "FlightData.h"
#include <sstream>
#include <iomanip>
#include <ctime>
#include <chrono>

// Constants for unit conversion
constexpr double LBS_TO_KGS = 0.453592;

// Helper to convert engine type enum to string
static std::string engineTypeToString(int engineType) {
    switch (engineType) {
        case 0: return "Piston";
        case 1: return "Jet";
        case 2: return "None";
        case 3: return "Helo (Turbine)";
        case 4: return "Rocket";
        case 5: return "Turboprop";
        default: return "Unknown";
    }
}

FlightDataJson FlightDataJson::fromSimConnect(const SimConnectFlightData& data, const std::string& simVersion) {
    FlightDataJson json;

    // Aircraft metadata
    json.aircraftTitle = data.title;
    json.atcType = data.atcType;
    json.atcModel = data.atcModel;
    json.atcId = data.atcId;
    json.atcAirline = data.atcAirline;
    json.atcFlightNumber = data.atcFlightNumber;
    json.category = data.category;
    json.engineType = static_cast<int>(data.engineType);
    json.engineTypeStr = engineTypeToString(static_cast<int>(data.engineType));
    json.numberOfEngines = static_cast<int>(data.numberOfEngines);
    json.maxGrossWeightLbs = data.maxGrossWeight;
    json.cruiseSpeedKts = data.cruiseSpeed;
    json.emptyWeightLbs = data.emptyWeight;

    // Position
    json.latitude = data.latitude;
    json.longitude = data.longitude;
    json.altitudeIndicated = data.altitudeIndicated;
    json.altitudeTrue = data.altitudeTrue;
    json.altitudeAGL = data.altitudeAGL;

    // Speed
    json.airspeedIndicated = data.airspeedIndicated;
    json.airspeedTrue = data.airspeedTrue;
    json.groundSpeed = data.groundSpeed;
    json.machNumber = data.machNumber;

    // Heading
    json.headingMagnetic = data.headingMagnetic;
    json.headingTrue = data.headingTrue;
    json.track = data.gpsGroundTrack;

    // Calculate fuel in LBS and KGS
    double fuelGallons = data.fuelTotalQuantity;
    json.fuelLbs = fuelGallons * data.fuelWeightPerGallon;
    json.fuelKgs = json.fuelLbs * LBS_TO_KGS;

    // Calculate weights
    json.totalWeightLbs = data.totalWeight;
    json.totalWeightKgs = data.totalWeight * LBS_TO_KGS;

    // Payload = Total Weight - Empty Weight - Fuel
    json.payloadLbs = data.totalWeight - data.emptyWeight - json.fuelLbs;
    json.payloadKgs = json.payloadLbs * LBS_TO_KGS;

    // Format frequencies
    json.com1Frequency = formatFrequency(data.com1ActiveFreq);
    json.com2Frequency = formatFrequency(data.com2ActiveFreq);
    json.nav1Frequency = formatFrequency(data.nav1ActiveFreq);
    json.nav2Frequency = formatFrequency(data.nav2ActiveFreq);

    json.timestamp = getCurrentTimestamp();
    json.simulatorVersion = simVersion;

    return json;
}

std::string FlightDataJson::formatFrequency(double freqHz) {
    // SimConnect returns frequency in Hz as FLOAT64 (e.g., 118700000.0 for 118.700 MHz)
    double freqMHz = freqHz / 1000000.0;
    std::ostringstream oss;
    oss << std::fixed << std::setprecision(3) << freqMHz;
    return oss.str();
}

std::string FlightDataJson::getCurrentTimestamp() {
    auto now = std::chrono::system_clock::now();
    auto time_t_now = std::chrono::system_clock::to_time_t(now);
    auto ms = std::chrono::duration_cast<std::chrono::milliseconds>(
        now.time_since_epoch()) % 1000;

    std::tm tm_utc;
    gmtime_s(&tm_utc, &time_t_now);

    std::ostringstream oss;
    oss << std::put_time(&tm_utc, "%Y-%m-%dT%H:%M:%S")
        << '.' << std::setfill('0') << std::setw(3) << ms.count() << 'Z';
    return oss.str();
}

std::string FlightDataJson::toJson() const {
    std::ostringstream oss;
    oss << std::fixed << std::setprecision(6);

    oss << "{";
    // Aircraft metadata
    oss << "\"aircraftTitle\":\"" << aircraftTitle << "\",";
    oss << "\"atcType\":\"" << atcType << "\",";
    oss << "\"atcModel\":\"" << atcModel << "\",";
    oss << "\"atcId\":\"" << atcId << "\",";
    oss << "\"atcAirline\":\"" << atcAirline << "\",";
    oss << "\"atcFlightNumber\":\"" << atcFlightNumber << "\",";
    oss << "\"category\":\"" << category << "\",";
    oss << "\"engineType\":" << engineType << ",";
    oss << "\"engineTypeStr\":\"" << engineTypeStr << "\",";
    oss << "\"numberOfEngines\":" << numberOfEngines << ",";
    oss << "\"maxGrossWeightLbs\":" << std::setprecision(1) << maxGrossWeightLbs << ",";
    oss << "\"cruiseSpeedKts\":" << cruiseSpeedKts << ",";
    oss << "\"emptyWeightLbs\":" << emptyWeightLbs << ",";
    // Position
    oss << "\"latitude\":" << std::setprecision(6) << latitude << ",";
    oss << "\"longitude\":" << longitude << ",";
    oss << "\"altitudeIndicated\":" << std::setprecision(1) << altitudeIndicated << ",";
    oss << "\"altitudeTrue\":" << altitudeTrue << ",";
    oss << "\"altitudeAGL\":" << altitudeAGL << ",";
    // Speed
    oss << "\"airspeedIndicated\":" << airspeedIndicated << ",";
    oss << "\"airspeedTrue\":" << airspeedTrue << ",";
    oss << "\"groundSpeed\":" << groundSpeed << ",";
    oss << "\"machNumber\":" << std::setprecision(3) << machNumber << ",";
    // Heading
    oss << "\"headingMagnetic\":" << std::setprecision(1) << headingMagnetic << ",";
    oss << "\"headingTrue\":" << headingTrue << ",";
    oss << "\"track\":" << track << ",";
    // Weight & Fuel
    oss << "\"fuelLbs\":" << fuelLbs << ",";
    oss << "\"fuelKgs\":" << fuelKgs << ",";
    oss << "\"payloadLbs\":" << payloadLbs << ",";
    oss << "\"payloadKgs\":" << payloadKgs << ",";
    oss << "\"totalWeightLbs\":" << totalWeightLbs << ",";
    oss << "\"totalWeightKgs\":" << totalWeightKgs << ",";
    // Radios
    oss << "\"com1Frequency\":\"" << com1Frequency << "\",";
    oss << "\"com2Frequency\":\"" << com2Frequency << "\",";
    oss << "\"nav1Frequency\":\"" << nav1Frequency << "\",";
    oss << "\"nav2Frequency\":\"" << nav2Frequency << "\",";
    // Metadata
    oss << "\"timestamp\":\"" << timestamp << "\",";
    oss << "\"simulatorVersion\":\"" << simulatorVersion << "\"";
    oss << "}";

    return oss.str();
}

std::string SimulatorStatus::toJson() const {
    std::ostringstream oss;

    oss << "{";
    oss << "\"isConnected\":" << (isConnected ? "true" : "false") << ",";
    oss << "\"isSimRunning\":" << (isSimRunning ? "true" : "false") << ",";
    oss << "\"simulatorVersion\":\"" << simulatorVersion << "\"";
    if (!connectionError.empty()) {
        oss << ",\"connectionError\":\"" << connectionError << "\"";
    }
    oss << "}";

    return oss.str();
}
