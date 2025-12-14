@echo off
echo ========================================
echo   Building PilotLife.Connector
echo ========================================
echo.

set MSBUILD="C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe"
set SOLUTION="C:\Users\Callu\RiderProjects\PilotLife\PilotLife.Connector\build\PilotLife.Connector.sln"
set OUTPUT_DIR="C:\Users\Callu\RiderProjects\PilotLife\pilotlife-app\src-tauri\resources"

echo Building Release x64...
%MSBUILD% %SOLUTION% /p:Configuration=Release /p:Platform=x64 /t:Build /v:m

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build FAILED with exit code: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Copying executable to Tauri resources...
copy /Y "C:\Users\Callu\RiderProjects\PilotLife\PilotLife.Connector\build\bin\Release\PilotLife.Connector.exe" %OUTPUT_DIR%

echo.
echo ========================================
echo   Build completed successfully!
echo ========================================
pause
