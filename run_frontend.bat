@echo off
cd /d "%~dp0frontend"
echo Installing frontend packages...
npm install
if errorlevel 1 pause && exit /b 1
echo Starting Angular frontend on http://localhost:4200 ...
npm start
pause
