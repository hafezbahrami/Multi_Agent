@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
set "BUILD_VERSION=%~1"
set "BINARY_VERSION=%BUILD_VERSION%"

if "%BUILD_VERSION%"=="" set "BUILD_VERSION=0.0.0-local"
if "%BINARY_VERSION%"=="" set "BINARY_VERSION=0.0.0.0"

for /f "usebackq delims=" %%I in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%normalize-version.ps1" -Version "%BUILD_VERSION%"`) do set "BINARY_VERSION=%%I"

echo restoring packages
dotnet restore "%SCRIPT_DIR%..\src\MQ.Office.MCP.Server.slnx"
if errorlevel 1 exit /b 1

echo Verifying code formatting
dotnet format "%SCRIPT_DIR%..\src\MQ.Office.MCP.Server.slnx" --verify-no-changes --severity warn
if errorlevel 1 exit /b 1

echo building code
dotnet build "%SCRIPT_DIR%..\src\MQ.Office.MCP.Server.slnx" -c Release ^
    /p:Version=%BUILD_VERSION% ^
    /p:FileVersion=%BINARY_VERSION% ^
    /p:AssemblyVersion=%BINARY_VERSION% ^
    /p:InformationalVersion=%BUILD_VERSION% ^
    /p:EnableNETAnalyzers=true ^
    /p:AnalysisLevel=latest ^
    /p:EnforceCodeStyleInBuild=true
if errorlevel 1 exit /b 1

endlocal
exit /b 0
