@echo off
setlocal EnableExtensions

echo Running unit tests with coverage (threshold: 100%% line + branch + method)

set "TEST_PROJECT=%~dp0..\src\MQ.Office.MCP.Server.Tests\MQ.Office.MCP.Server.Tests.csproj"
set "COVERAGE_OUTPUT=%~dp0..\TestResults\Coverage\"

dotnet test "%TEST_PROJECT%" -c Release ^
    /p:CollectCoverage=true ^
    /p:CoverletOutput=%COVERAGE_OUTPUT% ^
    /p:CoverletOutputFormat=cobertura ^
    /p:Threshold=100 ^
    /p:ThresholdType=line%%2Cbranch%%2Cmethod ^
    /p:ThresholdStat=total ^
    /p:Exclude="[MQ.Office.MCP.Server]Program"
if errorlevel 1 (
    echo.
    echo *** Unit tests or coverage threshold FAILED ***
    exit /b 1
)

endlocal
exit /b 0
