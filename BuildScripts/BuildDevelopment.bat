@echo off
echo ================================================
echo    SynthMind Development Build Script
echo ================================================
echo.

:: Set Unity path - update this to match your Unity installation
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.37f1\Editor\Unity.exe"

:: Check if Unity exists
if not exist %UNITY_PATH% (
    echo ERROR: Unity not found at %UNITY_PATH%
    echo Please update UNITY_PATH in this script to match your Unity installation.
    pause
    exit /b 1
)

:: Get current directory
set PROJECT_PATH=%~dp0..
echo Project Path: %PROJECT_PATH%

:: Set build parameters for development
set BUILD_METHOD=CommandLineBuildHelper.BuildWindows64
set LOG_FILE="%PROJECT_PATH%\BuildLogs\dev_build_log_%date:~-4,4%%date:~-10,2%%date:~-7,2%_%time:~0,2%%time:~3,2%%time:~6,2%.txt"

:: Create log directory if it doesn't exist
if not exist "%PROJECT_PATH%\BuildLogs" mkdir "%PROJECT_PATH%\BuildLogs"

echo Starting Unity DEVELOPMENT build...
echo - Debug symbols enabled
echo - Development console enabled
echo - Faster build time
echo Build log will be saved to: %LOG_FILE%
echo.

:: Execute Unity build with development profile
%UNITY_PATH% -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod %BUILD_METHOD% -logFile %LOG_FILE% -profile Development -version 1.0.0-dev

:: Check build result
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================================
    echo        DEVELOPMENT BUILD COMPLETED!
    echo ================================================
    echo.
    echo Opening build folder...
    explorer "%PROJECT_PATH%\Builds"
) else (
    echo.
    echo ================================================
    echo             BUILD FAILED!
    echo ================================================
    echo.
    echo Check the log file for details: %LOG_FILE%
    echo Opening log file...
    notepad %LOG_FILE%
)

pause