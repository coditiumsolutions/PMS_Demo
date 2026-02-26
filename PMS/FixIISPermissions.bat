@echo off
REM Fix IIS Permissions for PMS Deployment
REM Run this script as Administrator

echo ========================================
echo IIS Permissions Fix Script
echo ========================================
echo.
echo NOTE: This script must be run as Administrator!
echo Right-click and select "Run as administrator"
echo.
pause

set DEPLOY_PATH=D:\PMSDeploy
set APP_POOL_NAME=PMS

echo Step 1: Checking deployment folder...
if not exist "%DEPLOY_PATH%" (
    echo ERROR: Deployment folder not found: %DEPLOY_PATH%
    pause
    exit /b 1
)
echo [OK] Deployment folder exists
echo.

echo Step 2: Setting folder permissions...
echo Granting permissions to IIS_IUSRS...
icacls "%DEPLOY_PATH%" /grant "IIS_IUSRS:(OI)(CI)F" /T
if %ERRORLEVEL% EQU 0 (
    echo [OK] IIS_IUSRS permissions granted
) else (
    echo [WARNING] Could not grant IIS_IUSRS permissions
)

echo.
echo Granting permissions to Application Pool Identity...
icacls "%DEPLOY_PATH%" /grant "IIS AppPool\%APP_POOL_NAME%:(OI)(CI)F" /T
if %ERRORLEVEL% EQU 0 (
    echo [OK] Application Pool permissions granted
) else (
    echo [WARNING] Could not grant App Pool permissions
    echo Trying NETWORK SERVICE...
    icacls "%DEPLOY_PATH%" /grant "NETWORK SERVICE:(OI)(CI)F" /T
)

echo.
echo Granting permissions to Users group (read/execute)...
icacls "%DEPLOY_PATH%" /grant "Users:(OI)(CI)RX" /T

echo.
echo Step 3: Verifying web.config...
if exist "%DEPLOY_PATH%\web.config" (
    echo [OK] web.config found
) else (
    echo [WARNING] web.config not found!
    if exist "%~dp0web.config" (
        echo Copying web.config...
        copy "%~dp0web.config" "%DEPLOY_PATH%\web.config"
        echo [OK] web.config copied
    ) else (
        echo [ERROR] web.config not found in project directory!
    )
)

echo.
echo ========================================
echo Permissions fix completed!
echo ========================================
echo.
echo Next Steps:
echo 1. Restart the IIS Application Pool: %APP_POOL_NAME%
echo 2. Restart the IIS Site if needed
echo 3. Test the application at: http://172.20.228.2:84/
echo.
echo To restart the App Pool in PowerShell (as Admin):
echo   Restart-WebAppPool -Name '%APP_POOL_NAME%'
echo.
pause
