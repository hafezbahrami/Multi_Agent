@echo off
echo integration test commands
set "ITEST_PROJECT=%~dp0..\src\MQ.Office.MCP.Server.IntegrationTests\MQ.Office.MCP.Server.IntegrationTests.csproj"
set "ITEST_RESULTS=%~dp0..\TestResults\Integration\"

dotnet test "%ITEST_PROJECT%" -c Release --logger:"trx;LogFileName=integration-tests.trx" --results-directory "%ITEST_RESULTS%"
if errorlevel 1 exit /b 1
