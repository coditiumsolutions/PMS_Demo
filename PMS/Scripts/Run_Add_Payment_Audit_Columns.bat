@echo off
REM Run Add_Payment_Audit_Columns.sql on the PMS database.
REM Edit the next 4 lines if your server/user/password/database differ.
set SERVER=172.20.229.3
set USER=sa
set PASSWORD=Pakistan@786
set DATABASE=PMS

set SCRIPT=%~dp0Add_Payment_Audit_Columns.sql
if not exist "%SCRIPT%" (
    echo Script not found: %SCRIPT%
    pause
    exit /b 1
)

echo Running %SCRIPT% on %SERVER%\%DATABASE% ...
sqlcmd -S %SERVER% -U %USER% -P %PASSWORD% -d %DATABASE% -i "%SCRIPT%"
if errorlevel 1 (
    echo.
    echo sqlcmd failed. Check that SQL Server is reachable and credentials are correct.
    pause
    exit /b 1
)
echo.
echo Done. Payment audit columns should now exist.
pause
