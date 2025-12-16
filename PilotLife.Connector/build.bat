@echo off
REM Build script for PilotLife.Connector
REM Usage: build.bat [Debug|Release]

setlocal

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

echo =========================================
echo   Building PilotLife.Connector (%CONFIG%)
echo =========================================

set MSBUILD=C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe
set SOLUTION=C:\Users\Callu\RiderProjects\PilotLife\PilotLife.Connector\build\PilotLife.Connector.sln

if not exist "%MSBUILD%" (
    echo Error: MSBuild not found at %MSBUILD%
    exit /b 1
)

if not exist "%SOLUTION%" (
    echo Error: Solution file not found at %SOLUTION%
    echo You may need to run CMake first to generate the solution.
    exit /b 1
)

"%MSBUILD%" "%SOLUTION%" /p:Configuration=%CONFIG% /p:Platform=x64 /t:Build /v:m

if %ERRORLEVEL% equ 0 (
    echo.
    echo =========================================
    echo   Build successful!
    echo   Output: PilotLife.Connector\build\bin\%CONFIG%\PilotLife.Connector.exe
    echo =========================================
) else (
    echo.
    echo Build failed!
    exit /b 1
)

endlocal
