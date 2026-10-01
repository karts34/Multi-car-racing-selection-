@echo off
cd /d "%~dp0"
if exist "Build-v8\APEX Afterglow.exe" (
    start "" "Build-v8\APEX Afterglow.exe"
) else if exist "Build-v7\APEX Afterglow.exe" (
    start "" "Build-v7\APEX Afterglow.exe"
) else if exist "Build-v6\APEX Afterglow.exe" (
    start "" "Build-v6\APEX Afterglow.exe"
) else if exist "Build-v5\APEX Afterglow.exe" (
    start "" "Build-v5\APEX Afterglow.exe"
) else if exist "Build-v4\APEX Afterglow.exe" (
    start "" "Build-v4\APEX Afterglow.exe"
) else if exist "Build-v3\APEX Afterglow.exe" (
    start "" "Build-v3\APEX Afterglow.exe"
) else if exist "Build-v2\APEX Afterglow.exe" (
    start "" "Build-v2\APEX Afterglow.exe"
) else if exist "Build\APEX Afterglow.exe" (
    start "" "Build\APEX Afterglow.exe"
) else (
    echo The Windows game has not been built yet. Run Build-Game.ps1.
    pause
)
