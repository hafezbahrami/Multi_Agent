@echo off
setlocal EnableExtensions
set "SCRIPT_DIR=%~dp0"
dotnet format "%SCRIPT_DIR%..\src\MQ.Office.MCP.Server.slnx"
endlocal
exit /b 0
