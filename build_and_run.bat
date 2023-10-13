@echo off
rem Build the project
dotnet build %cd%

rem Check if the build was successful
if %errorlevel% neq 0 (
    echo Build failed.
    pause
    exit /b %errorlevel%
)

rem Launch the program
dotnet %cd%\bin\Debug\net6.0\Ascendance.dll
