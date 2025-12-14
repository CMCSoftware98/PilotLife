# PilotLife Build Guide

This guide explains how to build and run all components of the PilotLife project.

## Project Structure

```
PilotLife/
├── PilotLife.API/              # ASP.NET Core Web API
├── PilotLife.Database/         # Entity Framework Core database layer
├── PilotLife.Connector/        # C++ SimConnect bridge (communicates with MSFS)
├── pilotlife-app/              # Tauri + Vue.js desktop application
└── build_connector.bat         # Script to build the C++ connector
```

## Prerequisites

- **Visual Studio 2022** (v18) with C++ workload
- **.NET 8 SDK**
- **Node.js 18+** and **npm**
- **Rust** (for Tauri)
- **Microsoft Flight Simulator** (for SimConnect SDK)

---

## 1. PilotLife.Connector (C++ SimConnect Bridge)

The connector bridges the Tauri app with Microsoft Flight Simulator via SimConnect.

### Building

**Option A: Using the batch script (Recommended)**

```batch
# From the project root
build_connector.bat
```

This will:
1. Build the Release x64 configuration
2. Copy the executable to `pilotlife-app/src-tauri/resources/`

**Option B: Using MSBuild directly**

```batch
"C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
    "PilotLife.Connector\build\PilotLife.Connector.sln" ^
    /p:Configuration=Release /p:Platform=x64 /t:Build /v:m
```

**Option C: Using Visual Studio**

1. Open `PilotLife.Connector/build/PilotLife.Connector.sln`
2. Set configuration to `Release` and platform to `x64`
3. Build the solution (Ctrl+Shift+B)

### Output

The built executable will be at:
```
PilotLife.Connector/build/bin/Release/PilotLife.Connector.exe
```

Copy it to the Tauri resources folder:
```
pilotlife-app/src-tauri/resources/PilotLife.Connector.exe
```

### Regenerating CMake Build Files

If you need to regenerate the Visual Studio solution:

```batch
cd PilotLife.Connector
mkdir build
cd build
cmake .. -G "Visual Studio 17 2022" -A x64
```

---

## 2. PilotLife.API (Backend)

The .NET API handles authentication, jobs, airports, and aircraft management.

### Building

```bash
cd PilotLife.API
dotnet build
```

### Running

**Development mode:**
```bash
cd PilotLife.API
dotnet run
```

**With watch (auto-reload):**
```bash
cd PilotLife.API
dotnet watch run
```

The API will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### Configuration

Configure the database connection in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pilotlife;Username=postgres;Password=yourpassword"
  }
}
```

### Database Migrations

```bash
cd PilotLife.Database

# Create a new migration
dotnet ef migrations add MigrationName --startup-project ../PilotLife.API

# Apply migrations
dotnet ef database update --startup-project ../PilotLife.API
```

---

## 3. pilotlife-app (Tauri Desktop App)

The frontend is a Vue.js application wrapped in Tauri.

### Install Dependencies

```bash
cd pilotlife-app
npm install
```

### Development Mode

```bash
cd pilotlife-app
npm run tauri dev
```

This will:
1. Start the Vite dev server with hot-reload
2. Build and launch the Tauri application
3. Start the PilotLife.Connector automatically

### Production Build

```bash
cd pilotlife-app
npm run tauri build
```

The installer will be created in:
```
pilotlife-app/src-tauri/target/release/bundle/
```

### Frontend Only (No Tauri)

For faster frontend development without Tauri:
```bash
cd pilotlife-app
npm run dev
```

Access at `http://localhost:5173`

---

## Full Development Workflow

1. **Start the API:**
   ```bash
   cd PilotLife.API
   dotnet watch run
   ```

2. **Start the Tauri app (in another terminal):**
   ```bash
   cd pilotlife-app
   npm run tauri dev
   ```

3. **Launch Microsoft Flight Simulator** - The connector will automatically connect when MSFS is running.

---

## Environment Variables

### pilotlife-app/.env

```env
VITE_API_URL=https://localhost:5001
```

---

## Troubleshooting

### Connector not connecting

1. Ensure `PilotLife.Connector.exe` exists in `pilotlife-app/src-tauri/resources/`
2. Check that MSFS is running
3. Check the console for WebSocket connection errors

### API connection issues

1. Verify the API is running on port 5001
2. Check CORS settings in the API
3. Ensure `VITE_API_URL` is set correctly

### Build errors

**C++ Connector:**
- Ensure Visual Studio 2022 with C++ workload is installed
- Check that SimConnect SDK is available in `PilotLife.Connector/include/`

**Tauri:**
- Ensure Rust is installed: `rustup --version`
- Run `npm install` to ensure all dependencies are present

---

## Useful Commands

| Task | Command |
|------|---------|
| Build connector | `build_connector.bat` |
| Run API | `cd PilotLife.API && dotnet run` |
| Run Tauri dev | `cd pilotlife-app && npm run tauri dev` |
| Build Tauri | `cd pilotlife-app && npm run tauri build` |
| Run frontend only | `cd pilotlife-app && npm run dev` |
| Add EF migration | `cd PilotLife.Database && dotnet ef migrations add Name --startup-project ../PilotLife.API` |
| Update database | `cd PilotLife.Database && dotnet ef database update --startup-project ../PilotLife.API` |
