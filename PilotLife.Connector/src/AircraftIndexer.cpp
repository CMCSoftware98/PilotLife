#include "AircraftIndexer.h"
#include <iostream>
#include <fstream>
#include <sstream>
#include <algorithm>
#include <cctype>
#include <shlobj.h>

// Simple JSON parsing for manifest.json (avoiding external dependency for now)
// We'll use basic string parsing since manifest.json is simple

AircraftIndexer::AircraftIndexer() {
    // Set default config file path for aircraft paths
    char appDataPath[MAX_PATH];
    if (SUCCEEDED(SHGetFolderPathA(NULL, CSIDL_APPDATA, NULL, 0, appDataPath))) {
        m_configFilePath = std::string(appDataPath) + "\\PilotLife\\aircraft_paths.json";
    }
}

bool AircraftIndexer::initialize() {
    std::lock_guard<std::mutex> lock(m_indexMutex);

    m_searchPaths.clear();
    m_packages.clear();
    m_titleIndex.clear();

    // First try to load config file for custom paths
    loadConfigFile();

    // Then detect MSFS paths
    auto detectedPaths = detectMSFSInstallPaths();
    for (const auto& path : detectedPaths) {
        // Avoid duplicates
        if (std::find(m_searchPaths.begin(), m_searchPaths.end(), path) == m_searchPaths.end()) {
            m_searchPaths.push_back(path);
        }
    }

    if (m_searchPaths.empty()) {
        std::cerr << "No MSFS paths found. Aircraft file data will not be available." << std::endl;
        return false;
    }

    // Scan all paths
    for (const auto& basePath : m_searchPaths) {
        std::cout << "Scanning: " << basePath << std::endl;
        scanAircraftFolders(basePath);
    }

    // Build the title index
    buildTitleIndex();

    std::cout << "Indexed " << m_titleIndex.size() << " aircraft variants from "
              << m_packages.size() << " packages" << std::endl;

    return !m_packages.empty();
}

void AircraftIndexer::rescan() {
    initialize();
}

std::optional<IndexedAircraft> AircraftIndexer::findByTitle(const std::string& title) const {
    std::lock_guard<std::mutex> lock(m_indexMutex);

    std::string normalizedTitle = normalizeTitle(title);

    // Try exact match first
    auto it = m_titleIndex.find(normalizedTitle);
    if (it != m_titleIndex.end()) {
        std::cout << "Found exact match in index for: " << title << std::endl;
        return it->second;
    }

    // Try partial match if exact match fails
    for (const auto& [key, aircraft] : m_titleIndex) {
        if (key.find(normalizedTitle) != std::string::npos ||
            normalizedTitle.find(key) != std::string::npos) {
            std::cout << "Found partial match in index: " << key << std::endl;
            return aircraft;
        }
    }

    // Fallback: do an on-demand search through all aircraft.cfg files
    std::cout << "No match in index, trying fallback search..." << std::endl;
    return searchForTitle(title);
}

size_t AircraftIndexer::getIndexedCount() const {
    std::lock_guard<std::mutex> lock(m_indexMutex);
    return m_titleIndex.size();
}

std::vector<std::string> AircraftIndexer::getSearchPaths() const {
    std::lock_guard<std::mutex> lock(m_indexMutex);
    return m_searchPaths;
}

std::string AircraftIndexer::parseInstalledPackagesPath(const std::string& userCfgPath) {
    std::ifstream file(userCfgPath);
    if (!file.is_open()) {
        return "";
    }

    std::string line;
    while (std::getline(file, line)) {
        // Look for: InstalledPackagesPath "C:\path\to\packages"
        size_t pos = line.find("InstalledPackagesPath");
        if (pos != std::string::npos) {
            size_t firstQuote = line.find('"', pos);
            size_t secondQuote = line.find('"', firstQuote + 1);
            if (firstQuote != std::string::npos && secondQuote != std::string::npos) {
                return line.substr(firstQuote + 1, secondQuote - firstQuote - 1);
            }
        }
    }
    return "";
}

std::vector<std::string> AircraftIndexer::detectMSFSInstallPaths() {
    std::vector<std::string> paths;

    char appData[MAX_PATH];
    char localAppData[MAX_PATH];

    // Get AppData and LocalAppData paths
    bool hasAppData = SUCCEEDED(SHGetFolderPathA(NULL, CSIDL_APPDATA, NULL, 0, appData));
    bool hasLocalAppData = SUCCEEDED(SHGetFolderPathA(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, localAppData));

    // UserCfg.opt locations to check (in order of preference)
    std::vector<std::pair<std::string, std::string>> userCfgPaths;

    if (hasAppData) {
        // MSFS 2024 Steam/Standard
        userCfgPaths.push_back({
            std::string(appData) + "\\Microsoft Flight Simulator 2024\\UserCfg.opt",
            "MSFS 2024 (Steam/Standard)"
        });
        // MSFS 2020 Steam/Standard
        userCfgPaths.push_back({
            std::string(appData) + "\\Microsoft Flight Simulator\\UserCfg.opt",
            "MSFS 2020 (Steam/Standard)"
        });
    }

    if (hasLocalAppData) {
        // MSFS 2024 MS Store
        userCfgPaths.push_back({
            std::string(localAppData) + "\\Packages\\Microsoft.Limitless_8wekyb3d8bbwe\\LocalCache\\UserCfg.opt",
            "MSFS 2024 (MS Store)"
        });
        // MSFS 2020 MS Store
        userCfgPaths.push_back({
            std::string(localAppData) + "\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalCache\\UserCfg.opt",
            "MSFS 2020 (MS Store)"
        });
    }

    // Check each UserCfg.opt location
    for (const auto& [cfgPath, description] : userCfgPaths) {
        if (!std::filesystem::exists(cfgPath)) {
            continue;
        }

        std::cout << "Found UserCfg.opt: " << cfgPath << " (" << description << ")" << std::endl;

        std::string installedPackagesPath = parseInstalledPackagesPath(cfgPath);
        if (installedPackagesPath.empty()) {
            std::cout << "  Could not parse InstalledPackagesPath" << std::endl;
            continue;
        }

        // Track the UserCfg.opt path that we used
        m_userCfgOptPath = cfgPath;

        std::cout << "  InstalledPackagesPath: " << installedPackagesPath << std::endl;

        // Add Community folder
        std::string communityPath = installedPackagesPath + "\\Community";
        if (std::filesystem::exists(communityPath)) {
            paths.push_back(communityPath);
            std::cout << "  Added Community folder: " << communityPath << std::endl;
        }

        // Add Official folder (for default aircraft)
        std::string officialPath = installedPackagesPath + "\\Official";
        if (std::filesystem::exists(officialPath)) {
            // Official folder has subfolders like "OneStore" or "Steam"
            for (const auto& entry : std::filesystem::directory_iterator(officialPath)) {
                if (entry.is_directory()) {
                    paths.push_back(entry.path().string());
                    std::cout << "  Added Official folder: " << entry.path().string() << std::endl;
                }
            }
        }
    }

    // If we found paths from UserCfg.opt, save them to our config and return
    if (!paths.empty()) {
        savePathsToConfig(paths);
        return paths;
    }

    std::cout << "No UserCfg.opt found, checking fallback locations..." << std::endl;

    // Fallback: Check common Xbox/Steam installation paths
    std::vector<std::string> commonPaths = {
        "C:\\XboxGames\\Microsoft Flight Simulator 2024\\Content\\Community",
        "C:\\XboxGames\\Microsoft Flight Simulator\\Content\\Community",
        "D:\\XboxGames\\Microsoft Flight Simulator 2024\\Content\\Community",
        "D:\\XboxGames\\Microsoft Flight Simulator\\Content\\Community",
    };

    for (const auto& path : commonPaths) {
        if (std::filesystem::exists(path)) {
            paths.push_back(path);
            std::cout << "  Found fallback path: " << path << std::endl;
        }
    }

    if (!paths.empty()) {
        savePathsToConfig(paths);
    }

    return paths;
}

void AircraftIndexer::savePathsToConfig(const std::vector<std::string>& paths) {
    // Save to config file in AppData
    char appData[MAX_PATH];
    if (!SUCCEEDED(SHGetFolderPathA(NULL, CSIDL_APPDATA, NULL, 0, appData))) {
        return;
    }

    std::string configDir = std::string(appData) + "\\PilotLife";
    std::filesystem::create_directories(configDir);

    std::string configPath = configDir + "\\aircraft_paths.json";
    std::ofstream file(configPath);
    if (!file.is_open()) {
        std::cerr << "Could not save paths to config: " << configPath << std::endl;
        return;
    }

    file << "{\n";
    file << "  \"communityPaths\": [\n";
    for (size_t i = 0; i < paths.size(); i++) {
        // Escape backslashes for JSON
        std::string escaped;
        for (char c : paths[i]) {
            if (c == '\\') escaped += "\\\\";
            else escaped += c;
        }
        file << "    \"" << escaped << "\"";
        if (i < paths.size() - 1) file << ",";
        file << "\n";
    }
    file << "  ],\n";
    file << "  \"lastUpdated\": \"" << std::time(nullptr) << "\"\n";
    file << "}\n";
    file.close();

    std::cout << "Saved paths to config: " << configPath << std::endl;
    m_configFilePath = configPath;
}

bool AircraftIndexer::loadPathsFromConfig() {
    char appData[MAX_PATH];
    if (!SUCCEEDED(SHGetFolderPathA(NULL, CSIDL_APPDATA, NULL, 0, appData))) {
        return false;
    }

    std::string configPath = std::string(appData) + "\\PilotLife\\aircraft_paths.json";
    if (!std::filesystem::exists(configPath)) {
        return false;
    }

    std::ifstream file(configPath);
    if (!file.is_open()) {
        return false;
    }

    std::stringstream buffer;
    buffer << file.rdbuf();
    std::string content = buffer.str();
    file.close();

    // Simple JSON parsing for paths array
    size_t pos = content.find("\"communityPaths\"");
    if (pos == std::string::npos) {
        return false;
    }

    size_t arrayStart = content.find('[', pos);
    size_t arrayEnd = content.find(']', arrayStart);
    if (arrayStart == std::string::npos || arrayEnd == std::string::npos) {
        return false;
    }

    std::string arrayContent = content.substr(arrayStart + 1, arrayEnd - arrayStart - 1);

    // Parse each path in the array
    size_t pathStart = 0;
    while ((pathStart = arrayContent.find('"', pathStart)) != std::string::npos) {
        size_t pathEnd = arrayContent.find('"', pathStart + 1);
        if (pathEnd == std::string::npos) break;

        std::string path = arrayContent.substr(pathStart + 1, pathEnd - pathStart - 1);

        // Unescape backslashes
        std::string unescaped;
        for (size_t i = 0; i < path.length(); i++) {
            if (path[i] == '\\' && i + 1 < path.length() && path[i + 1] == '\\') {
                unescaped += '\\';
                i++;
            } else {
                unescaped += path[i];
            }
        }

        // Only add if path still exists
        if (std::filesystem::exists(unescaped)) {
            m_searchPaths.push_back(unescaped);
            std::cout << "Loaded path from config: " << unescaped << std::endl;
        } else {
            std::cout << "Cached path no longer exists: " << unescaped << std::endl;
        }

        pathStart = pathEnd + 1;
    }

    m_configFilePath = configPath;
    return !m_searchPaths.empty();
}

bool AircraftIndexer::loadConfigFile() {
    // Delegate to loadPathsFromConfig for backwards compatibility
    return loadPathsFromConfig();
}

void AircraftIndexer::scanAircraftFolders(const std::string& basePath) {
    if (!std::filesystem::exists(basePath)) {
        return;
    }

    try {
        for (const auto& entry : std::filesystem::directory_iterator(basePath)) {
            if (!entry.is_directory()) continue;

            std::filesystem::path packagePath = entry.path();
            std::filesystem::path manifestPath = packagePath / "manifest.json";

            if (!std::filesystem::exists(manifestPath)) continue;

            // Parse manifest.json first
            AircraftManifest manifest = parseManifestJson(manifestPath);
            manifest.packagePath = packagePath.string();

            // Only process aircraft packages
            if (manifest.contentType != "AIRCRAFT") continue;

            // Look for Aircraft.cfg in SimObjects/Airplanes subdirectories
            std::filesystem::path simObjectsPath = packagePath / "SimObjects" / "Airplanes";
            if (std::filesystem::exists(simObjectsPath)) {
                for (const auto& aircraftDir : std::filesystem::directory_iterator(simObjectsPath)) {
                    if (!aircraftDir.is_directory()) continue;

                    std::filesystem::path cfgPath = aircraftDir.path() / "aircraft.cfg";
                    if (std::filesystem::exists(cfgPath)) {
                        // Parse ALL variations from this aircraft.cfg
                        auto variations = parseAllAircraftCfgVariations(cfgPath);

                        for (const auto& config : variations) {
                            IndexedAircraft aircraft;
                            aircraft.manifest = manifest;
                            aircraft.hasManifest = !manifest.contentId.empty();
                            aircraft.config = config;
                            aircraft.hasConfig = !config.rawContent.empty();

                            if (aircraft.hasManifest || aircraft.hasConfig) {
                                m_packages.push_back(aircraft);
                            }
                        }
                    }
                }
            }
        }
    } catch (const std::exception& e) {
        std::cerr << "Error scanning " << basePath << ": " << e.what() << std::endl;
    }
}

AircraftManifest AircraftIndexer::parseManifestJson(const std::filesystem::path& filePath) {
    AircraftManifest manifest;

    std::ifstream file(filePath);
    if (!file.is_open()) {
        return manifest;
    }

    std::stringstream buffer;
    buffer << file.rdbuf();
    manifest.rawJson = buffer.str();
    file.close();

    // Simple JSON parsing - look for key-value pairs
    auto extractValue = [&](const std::string& key) -> std::string {
        std::string searchKey = "\"" + key + "\"";
        size_t keyPos = manifest.rawJson.find(searchKey);
        if (keyPos == std::string::npos) return "";

        size_t colonPos = manifest.rawJson.find(':', keyPos);
        if (colonPos == std::string::npos) return "";

        // Skip whitespace
        size_t valueStart = colonPos + 1;
        while (valueStart < manifest.rawJson.length() &&
               (manifest.rawJson[valueStart] == ' ' || manifest.rawJson[valueStart] == '\t' ||
                manifest.rawJson[valueStart] == '\n' || manifest.rawJson[valueStart] == '\r')) {
            valueStart++;
        }

        if (valueStart >= manifest.rawJson.length()) return "";

        if (manifest.rawJson[valueStart] == '"') {
            // String value
            size_t valueEnd = manifest.rawJson.find('"', valueStart + 1);
            if (valueEnd != std::string::npos) {
                return manifest.rawJson.substr(valueStart + 1, valueEnd - valueStart - 1);
            }
        }
        return "";
    };

    manifest.contentType = extractValue("content_type");
    manifest.title = extractValue("title");
    manifest.manufacturer = extractValue("manufacturer");
    manifest.creator = extractValue("creator");
    manifest.packageVersion = extractValue("package_version");
    manifest.minimumGameVersion = extractValue("minimum_game_version");
    manifest.totalPackageSize = extractValue("total_package_size");
    manifest.contentId = extractValue("content_id");

    return manifest;
}

AircraftConfig AircraftIndexer::parseAircraftCfg(const std::filesystem::path& filePath) {
    auto variations = parseAllAircraftCfgVariations(filePath);
    if (!variations.empty()) {
        return variations[0];
    }
    return AircraftConfig();
}

std::vector<AircraftConfig> AircraftIndexer::parseAllAircraftCfgVariations(const std::filesystem::path& filePath) {
    std::vector<AircraftConfig> variations;

    std::ifstream file(filePath);
    if (!file.is_open()) {
        return variations;
    }

    std::stringstream buffer;
    buffer << file.rdbuf();
    std::string rawContent = buffer.str();
    file.close();

    std::istringstream stream(rawContent);
    std::string line;
    std::string currentSection;

    // Store GENERAL section data to apply to all variations
    std::string generalAtcType, generalAtcModel, generalEditable, generalPerformance, generalCategory;

    // Current variation being parsed
    AircraftConfig currentConfig;
    bool inFltsimSection = false;

    while (std::getline(stream, line)) {
        // Trim whitespace
        size_t start = line.find_first_not_of(" \t\r\n");
        if (start == std::string::npos) continue;
        line = line.substr(start);

        // Skip comments
        if (line[0] == ';' || line[0] == '#') continue;

        // Check for section header
        if (line[0] == '[') {
            // If we were in a FLTSIM section, save the current config
            if (inFltsimSection && !currentConfig.title.empty()) {
                currentConfig.rawContent = rawContent;
                currentConfig.generalAtcType = generalAtcType;
                currentConfig.generalAtcModel = generalAtcModel;
                currentConfig.editable = generalEditable;
                currentConfig.performance = generalPerformance;
                currentConfig.category = generalCategory;
                variations.push_back(currentConfig);
                currentConfig = AircraftConfig();
            }

            size_t end = line.find(']');
            if (end != std::string::npos) {
                currentSection = line.substr(1, end - 1);
                // Normalize section name
                std::transform(currentSection.begin(), currentSection.end(),
                              currentSection.begin(), ::toupper);

                // Check if this is a FLTSIM.x section
                inFltsimSection = (currentSection.find("FLTSIM.") == 0);
            }
            continue;
        }

        // Parse key=value
        size_t eqPos = line.find('=');
        if (eqPos == std::string::npos) continue;

        std::string key = line.substr(0, eqPos);
        std::string value = line.substr(eqPos + 1);

        // Trim key
        size_t keyEnd = key.find_last_not_of(" \t");
        if (keyEnd != std::string::npos) {
            key = key.substr(0, keyEnd + 1);
        }

        // Normalize key
        std::transform(key.begin(), key.end(), key.begin(), ::tolower);

        // Parse value (remove quotes and comments)
        value = parseConfigValue(value);

        if (inFltsimSection) {
            if (key == "title") currentConfig.title = value;
            else if (key == "model") currentConfig.model = value;
            else if (key == "panel") currentConfig.panel = value;
            else if (key == "sound") currentConfig.sound = value;
            else if (key == "texture") currentConfig.texture = value;
            else if (key == "atc_type") currentConfig.atcType = value;
            else if (key == "atc_model") currentConfig.atcModel = value;
            else if (key == "atc_id") currentConfig.atcId = value;
            else if (key == "atc_airline") currentConfig.atcAirline = value;
            else if (key == "ui_manufacturer") currentConfig.uiManufacturer = value;
            else if (key == "ui_type") currentConfig.uiType = value;
            else if (key == "ui_variation") currentConfig.uiVariation = value;
            else if (key == "icao_airline") currentConfig.icaoAirline = value;
        }
        else if (currentSection == "GENERAL") {
            if (key == "atc_type") generalAtcType = value;
            else if (key == "atc_model") generalAtcModel = value;
            else if (key == "editable") generalEditable = value;
            else if (key == "performance") generalPerformance = value;
            else if (key == "category") generalCategory = value;
        }
    }

    // Don't forget the last variation
    if (inFltsimSection && !currentConfig.title.empty()) {
        currentConfig.rawContent = rawContent;
        currentConfig.generalAtcType = generalAtcType;
        currentConfig.generalAtcModel = generalAtcModel;
        currentConfig.editable = generalEditable;
        currentConfig.performance = generalPerformance;
        currentConfig.category = generalCategory;
        variations.push_back(currentConfig);
    }

    return variations;
}

void AircraftIndexer::buildTitleIndex() {
    for (const auto& aircraft : m_packages) {
        // Index by manifest title
        if (!aircraft.manifest.title.empty()) {
            m_titleIndex[normalizeTitle(aircraft.manifest.title)] = aircraft;
        }

        // Also index by config title (often includes livery variation)
        if (!aircraft.config.title.empty()) {
            m_titleIndex[normalizeTitle(aircraft.config.title)] = aircraft;
        }
    }

    std::cout << "Built title index with " << m_titleIndex.size() << " entries" << std::endl;
}

std::optional<IndexedAircraft> AircraftIndexer::searchForTitle(const std::string& title) const {
    std::cout << "Performing fallback search for: " << title << std::endl;
    std::string normalizedSearch = normalizeTitle(title);

    // Search through all aircraft.cfg files in all search paths
    for (const auto& basePath : m_searchPaths) {
        if (!std::filesystem::exists(basePath)) continue;

        try {
            for (const auto& entry : std::filesystem::directory_iterator(basePath)) {
                if (!entry.is_directory()) continue;

                std::filesystem::path packagePath = entry.path();
                std::filesystem::path manifestPath = packagePath / "manifest.json";

                // Look for Aircraft.cfg in SimObjects/Airplanes subdirectories
                std::filesystem::path simObjectsPath = packagePath / "SimObjects" / "Airplanes";
                if (!std::filesystem::exists(simObjectsPath)) continue;

                for (const auto& aircraftDir : std::filesystem::directory_iterator(simObjectsPath)) {
                    if (!aircraftDir.is_directory()) continue;

                    std::filesystem::path cfgPath = aircraftDir.path() / "aircraft.cfg";
                    if (!std::filesystem::exists(cfgPath)) continue;

                    // Parse all variations in this aircraft.cfg
                    auto variations = const_cast<AircraftIndexer*>(this)->parseAllAircraftCfgVariations(cfgPath);

                    for (const auto& config : variations) {
                        if (normalizeTitle(config.title) == normalizedSearch) {
                            std::cout << "Found match in: " << cfgPath.string() << std::endl;

                            // Build the IndexedAircraft result
                            IndexedAircraft result;
                            result.config = config;
                            result.hasConfig = true;

                            // Try to get manifest if it exists
                            if (std::filesystem::exists(manifestPath)) {
                                result.manifest = const_cast<AircraftIndexer*>(this)->parseManifestJson(manifestPath);
                                result.manifest.packagePath = packagePath.string();
                                result.hasManifest = !result.manifest.contentId.empty();
                            }

                            return result;
                        }
                    }
                }
            }
        } catch (const std::exception& e) {
            std::cerr << "Error during fallback search in " << basePath << ": " << e.what() << std::endl;
        }
    }

    std::cout << "Fallback search found no matches" << std::endl;
    return std::nullopt;
}

std::string AircraftIndexer::normalizeTitle(const std::string& title) {
    std::string normalized = title;
    // Convert to lowercase
    std::transform(normalized.begin(), normalized.end(), normalized.begin(), ::tolower);
    // Remove extra whitespace
    normalized.erase(std::unique(normalized.begin(), normalized.end(),
        [](char a, char b) { return a == ' ' && b == ' '; }), normalized.end());
    return normalized;
}

std::string AircraftIndexer::parseConfigValue(const std::string& line) {
    std::string value = line;

    // Trim leading whitespace
    size_t start = value.find_first_not_of(" \t");
    if (start == std::string::npos) return "";
    value = value.substr(start);

    // Remove inline comments (semicolon)
    size_t commentPos = value.find(';');
    if (commentPos != std::string::npos) {
        value = value.substr(0, commentPos);
    }

    // Trim trailing whitespace
    size_t end = value.find_last_not_of(" \t\r\n");
    if (end != std::string::npos) {
        value = value.substr(0, end + 1);
    }

    // Remove surrounding quotes
    if (value.length() >= 2 && value.front() == '"' && value.back() == '"') {
        value = value.substr(1, value.length() - 2);
    }

    return value;
}

std::string AircraftIndexer::toJsonResponse(const IndexedAircraft& aircraft, const std::string& requestId) {
    auto escapeJson = [](const std::string& s) -> std::string {
        std::string result;
        result.reserve(s.length() * 2);
        for (char c : s) {
            switch (c) {
                case '"': result += "\\\""; break;
                case '\\': result += "\\\\"; break;
                case '\n': result += "\\n"; break;
                case '\r': result += "\\r"; break;
                case '\t': result += "\\t"; break;
                default: result += c;
            }
        }
        return result;
    };

    std::ostringstream json;
    json << "{";
    json << "\"type\":\"aircraftDataResponse\",";
    json << "\"requestId\":\"" << escapeJson(requestId) << "\",";
    json << "\"data\":{";
    json << "\"found\":true,";

    // Manifest data
    json << "\"manifest\":{";
    json << "\"contentType\":\"" << escapeJson(aircraft.manifest.contentType) << "\",";
    json << "\"title\":\"" << escapeJson(aircraft.manifest.title) << "\",";
    json << "\"manufacturer\":\"" << escapeJson(aircraft.manifest.manufacturer) << "\",";
    json << "\"creator\":\"" << escapeJson(aircraft.manifest.creator) << "\",";
    json << "\"packageVersion\":\"" << escapeJson(aircraft.manifest.packageVersion) << "\",";
    json << "\"minimumGameVersion\":\"" << escapeJson(aircraft.manifest.minimumGameVersion) << "\",";
    json << "\"totalPackageSize\":\"" << escapeJson(aircraft.manifest.totalPackageSize) << "\",";
    json << "\"contentId\":\"" << escapeJson(aircraft.manifest.contentId) << "\",";
    json << "\"raw\":\"" << escapeJson(aircraft.manifest.rawJson) << "\"";
    json << "},";

    // Config data
    json << "\"config\":{";
    json << "\"title\":\"" << escapeJson(aircraft.config.title) << "\",";
    json << "\"model\":\"" << escapeJson(aircraft.config.model) << "\",";
    json << "\"panel\":\"" << escapeJson(aircraft.config.panel) << "\",";
    json << "\"sound\":\"" << escapeJson(aircraft.config.sound) << "\",";
    json << "\"texture\":\"" << escapeJson(aircraft.config.texture) << "\",";
    json << "\"atcType\":\"" << escapeJson(aircraft.config.atcType) << "\",";
    json << "\"atcModel\":\"" << escapeJson(aircraft.config.atcModel) << "\",";
    json << "\"atcId\":\"" << escapeJson(aircraft.config.atcId) << "\",";
    json << "\"atcAirline\":\"" << escapeJson(aircraft.config.atcAirline) << "\",";
    json << "\"uiManufacturer\":\"" << escapeJson(aircraft.config.uiManufacturer) << "\",";
    json << "\"uiType\":\"" << escapeJson(aircraft.config.uiType) << "\",";
    json << "\"uiVariation\":\"" << escapeJson(aircraft.config.uiVariation) << "\",";
    json << "\"icaoAirline\":\"" << escapeJson(aircraft.config.icaoAirline) << "\",";
    json << "\"generalAtcType\":\"" << escapeJson(aircraft.config.generalAtcType) << "\",";
    json << "\"generalAtcModel\":\"" << escapeJson(aircraft.config.generalAtcModel) << "\",";
    json << "\"editable\":\"" << escapeJson(aircraft.config.editable) << "\",";
    json << "\"performance\":\"" << escapeJson(aircraft.config.performance) << "\",";
    json << "\"category\":\"" << escapeJson(aircraft.config.category) << "\",";
    json << "\"raw\":\"" << escapeJson(aircraft.config.rawContent) << "\"";
    json << "}";

    json << "}}";
    return json.str();
}

std::string AircraftIndexer::toNotFoundResponse(const std::string& requestId) {
    std::ostringstream json;
    json << "{";
    json << "\"type\":\"aircraftDataResponse\",";
    json << "\"requestId\":\"" << requestId << "\",";
    json << "\"data\":{\"found\":false}";
    json << "}";
    return json.str();
}

std::string AircraftIndexer::toPathsInfoResponse() const {
    std::lock_guard<std::mutex> lock(m_indexMutex);

    auto escapeJson = [](const std::string& s) -> std::string {
        std::string result;
        result.reserve(s.length() * 2);
        for (char c : s) {
            switch (c) {
                case '"': result += "\\\""; break;
                case '\\': result += "\\\\"; break;
                case '\n': result += "\\n"; break;
                case '\r': result += "\\r"; break;
                case '\t': result += "\\t"; break;
                default: result += c;
            }
        }
        return result;
    };

    std::ostringstream json;
    json << "{";
    json << "\"type\":\"msfsPaths\",";
    json << "\"data\":{";
    json << "\"userCfgOptPath\":\"" << escapeJson(m_userCfgOptPath) << "\",";
    json << "\"configFilePath\":\"" << escapeJson(m_configFilePath) << "\",";
    json << "\"indexedAircraftCount\":" << m_titleIndex.size() << ",";
    json << "\"searchPaths\":[";

    for (size_t i = 0; i < m_searchPaths.size(); i++) {
        json << "\"" << escapeJson(m_searchPaths[i]) << "\"";
        if (i < m_searchPaths.size() - 1) json << ",";
    }

    json << "]";
    json << "}}";
    return json.str();
}
