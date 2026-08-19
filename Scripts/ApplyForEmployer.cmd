@echo off
setlocal
REM اجرا روی سیستم دارای sqlcmd (معمولاً همان سرور SQL یا سیستم ادمین)
REM رمز sa را وقتی پرسید وارد کنید، یا به عنوان آرگومان اول بدهید.

set SERVER=185.255.91.242,2019
set USER=sa
set SCRIPT=%~dp0ApplyForEmployer.sql

if "%~1"=="" (
  set /p SAPASS=SQL sa password:
) else (
  set SAPASS=%~1
)

echo Running %SCRIPT% on %SERVER% ...
sqlcmd -S %SERVER% -U %USER% -P "%SAPASS%" -I -b -i "%SCRIPT%"
if errorlevel 1 (
  echo FAILED
  exit /b 1
)
echo DONE
endlocal
