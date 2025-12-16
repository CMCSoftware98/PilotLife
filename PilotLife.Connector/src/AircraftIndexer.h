#pragma once

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <windows.h>

#include <string>
#include <vector>
#include <map>
#include <mutex>
#include <optional>
#include <filesystem>

// Parsed manifest.json data
struct AircraftManifest {
    std::string packagePath;
    std::string contentType;
    std::string title;
    std::string manufacturer;
    std::string creator;
    std::string packageVersion;
    std::string minimumGameVersion;
    std::string totalPackageSize;
    std::string contentId;
    std::string rawJson;
};

// Parsed Aircraft.cfg data
struct AircraftConfig {
    // [FLTSIM.0] section
    std::string title;
    std::string model;
    std::string panel;
    std::string sound;
    std::string texture;
    std::string atcType;
    std::string atcModel;
    std::string atcId;
    std::string atcAirline;
    std::string uiManufacturer;
    std::string uiType;
    std::string uiVariation;
    std::string icaoAirline;

    // [GENERAL] section
    std::string generalAtcType;
    std::string generalAtcModel;
    std::string editable;
    std::string performance;
    std::string category;

    std::string rawContent;
};

// Combined indexed aircraft data
struct IndexedAircraft {
    AircraftManifest manifest;
    AircraftConfig config;
    bool hasManifest = false;
    bool hasConfig = false;
};

class AircraftIndexer {
public:
    AircraftIndexer();
    ~AircraftIndexer() = default;

    // Initialize and scan for aircraft
    bool initialize();

    // Rescan all aircraft folders
    void rescan();

    // Find aircraft by title (matches SimConnect TITLE variable)
    std::optional<IndexedAircraft> findByTitle(const std::string& title) const;

    // Get indexed count
    size_t getIndexedCount() const;

    // Get all search paths being used
    std::vector<std::string> getSearchPaths() const;

    // Convert IndexedAircraft to JSON response string
    static std::string toJsonResponse(const IndexedAircraft& aircraft, const std::string& requestId);

    // Create a "not found" JSON response
    static std::string toNotFoundResponse(const std::string& requestId);

    // Create a JSON response with MSFS paths info
    std::string toPathsInfoResponse() const;

private:
    // Detect MSFS installation paths by parsing UserCfg.opt files
    std::vector<std::string> detectMSFSInstallPaths();

    // Parse InstalledPackagesPath from UserCfg.opt file
    std::string parseInstalledPackagesPath(const std::string& userCfgPath);

    // Save discovered paths to config file
    void savePathsToConfig(const std::vector<std::string>& paths);

    // Load paths from config file (checks if paths still exist)
    bool loadPathsFromConfig();

    // Load custom paths from config file (legacy, delegates to loadPathsFromConfig)
    bool loadConfigFile();

    // Scan a base path for aircraft packages
    void scanAircraftFolders(const std::string& basePath);

    // Parse manifest.json file
    AircraftManifest parseManifestJson(const std::filesystem::path& filePath);

    // Parse Aircraft.cfg file (returns config for first FLTSIM section)
    AircraftConfig parseAircraftCfg(const std::filesystem::path& filePath);

    // Parse all FLTSIM sections from Aircraft.cfg file
    std::vector<AircraftConfig> parseAllAircraftCfgVariations(const std::filesystem::path& filePath);

    // Build title index from all scanned packages
    void buildTitleIndex();

    // Fallback search: scan all aircraft.cfg files for a specific title
    std::optional<IndexedAircraft> searchForTitle(const std::string& title) const;

    // Helper to normalize title for matching
    static std::string normalizeTitle(const std::string& title);

    // Helper to parse INI-style config value
    static std::string parseConfigValue(const std::string& line);

    // Index by normalized title -> IndexedAircraft
    std::map<std::string, IndexedAircraft> m_titleIndex;

    // All discovered packages
    std::vector<IndexedAircraft> m_packages;

    // Search paths
    std::vector<std::string> m_searchPaths;

    // Config file path
    std::string m_configFilePath;

    // UserCfg.opt path that was used (for info display)
    std::string m_userCfgOptPath;

    // Thread safety
    mutable std::mutex m_indexMutex;
};
