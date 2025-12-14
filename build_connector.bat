@echo off
echo Building PilotLife.Connector...
"C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe" "C:\Users\Callu\RiderProjects\PilotLife\PilotLife.Connector\PilotLife.Connector.sln" /p:Configuration=Release /p:Platform=x64 /t:Rebuild /v:m
echo Build complete. Exit code: %ERRORLEVEL%
