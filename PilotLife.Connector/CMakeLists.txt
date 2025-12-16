cmake_minimum_required(VERSION 3.20)
project(PilotLife.Connector VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Static runtime for self-contained executable
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

# Output directories
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

# Find vcpkg packages (static)
find_package(ixwebsocket CONFIG REQUIRED)

# SimConnect SDK path
if(NOT DEFINED SIMCONNECT_SDK_PATH)
    if(DEFINED ENV{MSFS_SDK})
        set(SIMCONNECT_SDK_PATH "$ENV{MSFS_SDK}/SimConnect SDK")
    else()
        # Try common locations
        if(EXISTS "C:/MSFS 2024 SDK/SimConnect SDK/include/SimConnect.h")
            set(SIMCONNECT_SDK_PATH "C:/MSFS 2024 SDK/SimConnect SDK")
        elseif(EXISTS "C:/MSFS SDK/SimConnect SDK/include/SimConnect.h")
            set(SIMCONNECT_SDK_PATH "C:/MSFS SDK/SimConnect SDK")
        else()
            message(FATAL_ERROR "SimConnect SDK not found. Set MSFS_SDK environment variable or pass -DSIMCONNECT_SDK_PATH=<path>")
        endif()
    endif()
endif()

message(STATUS "SimConnect SDK path: ${SIMCONNECT_SDK_PATH}")

# Verify SimConnect.h exists
if(NOT EXISTS "${SIMCONNECT_SDK_PATH}/include/SimConnect.h")
    message(FATAL_ERROR "SimConnect.h not found at ${SIMCONNECT_SDK_PATH}/include/SimConnect.h")
endif()

# Source files
set(SOURCES
    src/main.cpp
    src/SimConnectManager.cpp
    src/ProcessDetector.cpp
    src/WebSocketServer.cpp
    src/FlightData.cpp
    src/AircraftIndexer.cpp
)

set(HEADERS
    src/SimConnectManager.h
    src/ProcessDetector.h
    src/WebSocketServer.h
    src/FlightData.h
    src/AircraftIndexer.h
)

# Create executable
add_executable(${PROJECT_NAME} ${SOURCES} ${HEADERS})

# Include directories
target_include_directories(${PROJECT_NAME} PRIVATE
    ${SIMCONNECT_SDK_PATH}/include
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

# Link directories
target_link_directories(${PROJECT_NAME} PRIVATE
    ${SIMCONNECT_SDK_PATH}/lib
)

# Link libraries
target_link_libraries(${PROJECT_NAME} PRIVATE
    SimConnect
    shlwapi
    user32
    Ws2_32
    bcrypt
    crypt32
    ixwebsocket::ixwebsocket
)

# Windows-specific settings
if(WIN32)
    target_compile_definitions(${PROJECT_NAME} PRIVATE
        WIN32
        _WINDOWS
        _UNICODE
        UNICODE
    )

    # Console application
    set_target_properties(${PROJECT_NAME} PROPERTIES
        WIN32_EXECUTABLE FALSE
    )
endif()

# Copy SimConnect.dll to output directory (still needed as it's dynamically loaded)
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${SIMCONNECT_SDK_PATH}/lib/SimConnect.dll"
    $<TARGET_FILE_DIR:${PROJECT_NAME}>
    COMMENT "Copying SimConnect.dll to output directory"
)

# Also copy to Tauri resources folder
set(TAURI_RESOURCES_DIR "${CMAKE_SOURCE_DIR}/../pilotlife-app/src-tauri/resources")
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E make_directory "${TAURI_RESOURCES_DIR}"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        $<TARGET_FILE:${PROJECT_NAME}>
        "${TAURI_RESOURCES_DIR}/PilotLife.Connector.exe"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "${SIMCONNECT_SDK_PATH}/lib/SimConnect.dll"
        "${TAURI_RESOURCES_DIR}/SimConnect.dll"
    COMMENT "Copying connector to Tauri resources"
)

# Install rules
install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
)

install(FILES "${SIMCONNECT_SDK_PATH}/lib/SimConnect.dll"
    DESTINATION bin
)

# Print configuration summary
message(STATUS "")
message(STATUS "=== PilotLife.Connector Configuration ===")
message(STATUS "  C++ Standard: ${CMAKE_CXX_STANDARD}")
message(STATUS "  SimConnect SDK: ${SIMCONNECT_SDK_PATH}")
message(STATUS "  Output directory: ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")
message(STATUS "  Tauri resources: ${TAURI_RESOURCES_DIR}")
message(STATUS "")
