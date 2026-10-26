@echo off
setlocal
cd /d "%~dp0"
set DOTNET_CLI_HOME=%CD%\.dotnet
set DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet publish BobbyBoi.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
if errorlevel 1 exit /b 1
dist\BobbyBoi.exe --self-test
if errorlevel 1 exit /b 1
dist\BobbyBoi.exe --mob-test
if errorlevel 1 exit /b 1
dist\BobbyBoi.exe --world-test
if errorlevel 1 exit /b 1
dist\BobbyBoi.exe --chaos-test
if errorlevel 1 exit /b 1
dist\BobbyBoi.exe --sfx-test
if errorlevel 1 exit /b 1
dist\BobbyBoi.exe --interruption-test
if errorlevel 1 exit /b 1
copy /y dist\BobbyBoi.exe BobbyBoi.exe >nul 2>nul
if errorlevel 1 (
 echo New build ready in dist. Choose Restart BobbyBoi in the pet menu to apply it.
 exit /b 0
)
echo BobbyBoi.exe updated.
