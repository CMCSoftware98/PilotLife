#include "ProcessDetector.h"
#include <tlhelp32.h>
#include <string>

const wchar_t* ProcessDetector::MSFS2020_PROCESS_NAME = L"FlightSimulator.exe";
const wchar_t* ProcessDetector::MSFS2024_PROCESS_NAME = L"FlightSimulator2024.exe";

SimulatorType ProcessDetector::detectMSFS() {
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) {
        return SimulatorType::None;
    }

    PROCESSENTRY32W pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32W);

    SimulatorType result = SimulatorType::None;

    if (Process32FirstW(hSnapshot, &pe32)) {
        do {
            std::wstring processName(pe32.szExeFile);

            // Check for MSFS 2024 first (it takes priority)
            if (processName == MSFS2024_PROCESS_NAME) {
                result = SimulatorType::MSFS2024;
                break;
            }
            else if (processName == MSFS2020_PROCESS_NAME) {
                result = SimulatorType::MSFS2020;
                // Don't break - continue checking for 2024
            }
        } while (Process32NextW(hSnapshot, &pe32));
    }

    CloseHandle(hSnapshot);
    return result;
}

DWORD ProcessDetector::getMSFSProcessId() {
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) {
        return 0;
    }

    PROCESSENTRY32W pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32W);
    DWORD pid = 0;
    SimulatorType foundType = SimulatorType::None;

    if (Process32FirstW(hSnapshot, &pe32)) {
        do {
            std::wstring processName(pe32.szExeFile);

            // Prefer MSFS 2024 if both are running
            if (processName == MSFS2024_PROCESS_NAME) {
                pid = pe32.th32ProcessID;
                break; // 2024 takes priority
            }
            else if (processName == MSFS2020_PROCESS_NAME && foundType == SimulatorType::None) {
                pid = pe32.th32ProcessID;
                foundType = SimulatorType::MSFS2020;
                // Continue searching for 2024
            }
        } while (Process32NextW(hSnapshot, &pe32));
    }

    CloseHandle(hSnapshot);
    return pid;
}

std::string ProcessDetector::getSimulatorTypeString(SimulatorType type) {
    switch (type) {
        case SimulatorType::MSFS2020:
            return "MSFS2020";
        case SimulatorType::MSFS2024:
            return "MSFS2024";
        default:
            return "None";
    }
}
