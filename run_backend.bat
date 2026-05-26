@echo off
cd /d "%~dp0backend"
echo Restoring backend packages...
dotnet restore
if errorlevel 1 pause && exit /b 1
echo Starting ASP.NET Core backend on https://localhost:7235 ...
dotnet run --launch-profile https
pause
