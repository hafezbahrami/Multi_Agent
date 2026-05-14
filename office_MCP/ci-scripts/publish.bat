@echo off
setlocal EnableExtensions
set "SCRIPT_DIR=%~dp0"
set "BUILD_VERSION=%~1"
if "%BUILD_VERSION%"=="" set "BUILD_VERSION=0.0.0-local"

dotnet publish "%SCRIPT_DIR%..\src\MQ.Office.MCP.Server\MQ.Office.MCP.Server.csproj" ^
    -c Release ^
    -r win-x64 --self-contained false ^
    -o "%SCRIPT_DIR%..\TestResults\LocalArtifactPreStaging" ^
    /p:Version=%BUILD_VERSION%
if errorlevel 1 exit /b 1

endlocal
exit /b 0
