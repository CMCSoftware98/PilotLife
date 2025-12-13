#include "FlightData.h"
#include <sstream>
#include <iomanip>
#include <ctime>
#include <chrono>

// Constants for unit conversion
constexpr double LBS_TO_KGS = 0.453592;

FlightDataJson FlightDataJson::fromSimConnect(const SimConnectFlightData& data, const std::string& simVersion) {
    FlightDataJson json;

    json.aircraftTitle = data.title;
    json.latitude = data.latitude;
    json.longitude = data.longitude;
    json.altitudeIndicated = data.altitudeIndicated;
    json.altitudeTrue = data.altitudeTrue;
    json.altitudeAGL = data.altitudeAGL;
    json.airspeedIndicated = data.airspeedIndicated;
    json.airspeedTrue = data.airspeedTrue;
    json.groundSpeed = data.groundSpeed;
    json.machNumber = data.machNumber;
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

std::string FlightDataJson::formatFrequency(int freqHz) {
    // SimConnect returns frequency in Hz (e.g., 118700000 for 118.700 MHz)
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
    oss << "\"aircraftTitle\":\"" << aircraftTitle << "\",";
    oss << "\"latitude\":" << latitude << ",";
    oss << "\"longitude\":" << longitude << ",";
    oss << "\"altitudeIndicated\":" << std::setprecision(1) << altitudeIndicated << ",";
    oss << "\"altitudeTrue\":" << altitudeTrue << ",";
    oss << "\"altitudeAGL\":" << altitudeAGL << ",";
    oss << "\"airspeedIndicated\":" << airspeedIndicated << ",";
    oss << "\"airspeedTrue\":" << airspeedTrue << ",";
    oss << "\"groundSpeed\":" << groundSpeed << ",";
    oss << "\"machNumber\":" << std::setprecision(3) << machNumber << ",";
    oss << "\"headingMagnetic\":" << std::setprecision(1) << headingMagnetic << ",";
    oss << "\"headingTrue\":" << headingTrue << ",";
    oss << "\"track\":" << track << ",";
    oss << "\"fuelLbs\":" << fuelLbs << ",";
    oss << "\"fuelKgs\":" << fuelKgs << ",";
    oss << "\"payloadLbs\":" << payloadLbs << ",";
    oss << "\"payloadKgs\":" << payloadKgs << ",";
    oss << "\"totalWeightLbs\":" << totalWeightLbs << ",";
    oss << "\"totalWeightKgs\":" << totalWeightKgs << ",";
    oss << "\"com1Frequency\":\"" << com1Frequency << "\",";
    oss << "\"com2Frequency\":\"" << com2Frequency << "\",";
    oss << "\"nav1Frequency\":\"" << nav1Frequency << "\",";
    oss << "\"nav2Frequency\":\"" << nav2Frequency << "\",";
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
