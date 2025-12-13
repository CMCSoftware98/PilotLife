# PilotLife.Connector - Flight Simulator Integration Tool

## Overview

A native C++ tool that connects directly to Microsoft Flight Simulator (2020 & 2024) via the SimConnect SDK to track real-time flight data and communicate with the PilotLife Tauri application through WebSocket.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Tauri App                                │
│  ┌──────────────┐     ┌──────────────────────────────────────┐ │
│  │   Vue App    │◀───▶│            Rust Backend              │ │
│  │  (Frontend)  │     │  - Find available port               │ │
│  └──────────────┘     │  - Launch connector with --port arg  │ │
│         │             │  - Store port in state               │ │
│         │             └──────────────────────────────────────┘ │
│         │                           │                          │
│         │    WebSocket              │ spawn process            │
│         │    (dynamic port)         │ with --port argument     │
│         ▼                           ▼                          │
│  ┌────────────────────────────────────────────────────────┐    │
│  │              PilotLife.Connector.exe                    │    │
│  │  - Accept port via CLI argument                        │    │
│  │  - Start WebSocket server on that port                 │    │
│  │  - Connect to SimConnect when MSFS detected            │    │
│  │  - Push flight data every 5 seconds                    │    │
│  └────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │      MSFS       │
                    │   SimConnect    │
                    └─────────────────┘
```

## Dynamic Port Negotiation Flow

1. **Tauri app starts** → Rust backend finds an available TCP port
2. **Rust launches connector** → `PilotLife.Connector.exe --port 52341`
3. **C++ connector starts** → WebSocket server binds to port 52341
4. **Vue requests port** → Calls Tauri command `start_connector()` which returns the port
5. **Vue connects** → `new WebSocket('ws://127.0.0.1:52341')`

## Configuration Summary

| Setting | Value |
|---------|-------|
| **Implementation** | C++ with direct SimConnect SDK |
| **IPC Method** | WebSocket on dynamic port (localhost) |
| **Port Selection** | Tauri finds available port, passes to connector |
| **Data Flow** | Push (real-time every 5 seconds) |
| **Process Check** | Every 10 seconds for MSFS |
| **Deployment** | Bundled with Tauri app |

---

## Project Structure

```
PilotLife.Connector/
├── CMakeLists.txt              # CMake build configuration
├── src/
│   ├── main.cpp                # Entry point, CLI args, main loop
│   ├── SimConnectManager.h     # SimConnect wrapper header
│   ├── SimConnectManager.cpp   # SimConnect connection & data handling
│   ├── ProcessDetector.h       # MSFS process detection header
│   ├── ProcessDetector.cpp     # Process detection implementation
│   ├── WebSocketServer.h       # WebSocket server header
│   ├── WebSocketServer.cpp     # WebSocket implementation (IXWebSocket)
│   ├── FlightData.h            # Data structures & enums
│   └── FlightData.cpp          # JSON serialization
├── include/
│   └── (vcpkg managed)
└── lib/
    └── SimConnect.lib          # From MSFS SDK
```

## SimConnect Data Tracked

| Category | Variables |
|----------|-----------|
| **Aircraft Info** | Title |
| **Position** | Latitude, Longitude |
| **Altitude** | Indicated, True, AGL (feet) |
| **Speed** | IAS, TAS, Ground Speed (knots), Mach |
| **Heading** | Magnetic, True, GPS Track (degrees) |
| **Fuel** | Total quantity (LBS & KGS) |
| **Weight** | Total weight, Payload (LBS & KGS) |
| **Communications** | COM1/2, NAV1/2 frequencies (MHz) |

---

## Build & Dependencies

### C++ Dependencies (vcpkg)
```bash
vcpkg install ixwebsocket:x64-windows
```

### SimConnect SDK
- **Environment Variable**: `MSFS_SDK` should point to SDK root
- **Path**: `$(MSFS_SDK)/SimConnect SDK`
- **Libraries**: `SimConnect.lib`, `shlwapi.lib`, `user32.lib`, `Ws2_32.lib`

### Build with CMake
```bash
cd PilotLife.Connector
mkdir build && cd build
cmake .. -DCMAKE_TOOLCHAIN_FILE=[vcpkg root]/scripts/buildsystems/vcpkg.cmake
cmake --build . --config Release
```

### Deployment
1. Build C++ connector with CMake
2. Copy `PilotLife.Connector.exe` + `SimConnect.dll` to `pilotlife-app/src-tauri/resources/`
3. `tauri.conf.json` already configured to bundle these resources

---

## Tauri Commands

| Command | Description |
|---------|-------------|
| `start_connector` | Finds port, launches connector, returns port number |
| `stop_connector` | Kills connector process |
| `get_connector_port` | Returns current port (if running) |
| `is_connector_running` | Returns boolean status |

---

## Vue Usage Example

```typescript
import { connector } from '@/services/connector';

// Start the connector
await connector.start();

// Subscribe to flight data
const unsubscribe = connector.onFlightData((data) => {
    console.log('Position:', data.latitude, data.longitude);
    console.log('Altitude:', data.altitudeIndicated, 'ft');
});

// Subscribe to status changes
connector.onStatus((status) => {
    console.log('Connected:', status.isConnected);
    console.log('Simulator:', status.simulatorVersion);
});

// Stop when done
await connector.stop();
```

---

## Sources

- [SimConnect SDK Documentation](https://docs.flightsimulator.com/html/Programming_Tools/SimConnect/SimConnect_SDK.htm)
- [SimConnect API Reference](https://docs.flightsimulator.com/html/Programming_Tools/SimConnect/API_Reference/SimConnect_API_Reference.htm)
- [IXWebSocket GitHub](https://github.com/machinezone/IXWebSocket)
