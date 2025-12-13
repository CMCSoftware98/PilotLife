#pragma once

#include <windows.h>
#include <string>

enum class SimulatorType {
    None,
    MSFS2020,
    MSFS2024
};

class ProcessDetector {
public:
    // Check if MSFS is running, returns which version
    static SimulatorType detectMSFS();

    // Get process ID if running
    static DWORD getMSFSProcessId();

    // Get simulator type as string
    static std::string getSimulatorTypeString(SimulatorType type);

private:
    static const wchar_t* MSFS2020_PROCESS_NAME;
    static const wchar_t* MSFS2024_PROCESS_NAME;
};
