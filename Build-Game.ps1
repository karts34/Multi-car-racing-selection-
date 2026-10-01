$ErrorActionPreference = 'Stop'
$raceProject = $PSScriptRoot
$raceEditor = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $raceEditor)) { throw 'Unity 6000.6.0f1 was not found. Update $raceEditor in this script.' }
New-Item -ItemType Directory -Force -Path (Join-Path $raceProject 'Logs') | Out-Null
$raceLog = Join-Path $raceProject 'Logs\build.log'
$raceArguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $raceProject + '"'), '-executeMethod', 'RaceBuilder.Build', '-logFile', ('"' + $raceLog + '"'))
$raceBuild = Start-Process -FilePath $raceEditor -ArgumentList $raceArguments -WindowStyle Hidden -PassThru -Wait
if ($raceBuild.ExitCode -ne 0) { throw 'Build failed. See Logs\build.log.' }
Write-Host 'Ready: Build-v8\APEX Afterglow.exe'
