# installer/install.ps1
# Scripted installer for Veqlo Quick v1.1

param(
    [string]$TargetDir = "$env:LOCALAPPDATA\Programs\Veqlo Quick",
    [switch]$RestartExplorer
)

$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " Veqlo Quick v1.1 - Automated Installation   " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$packageDir = Join-Path $projectRoot "package"

if (-not (Test-Path (Join-Path $packageDir "VeqloQuick.exe"))) {
    throw "Build package first! Missing VeqloQuick.exe in: $packageDir"
}

# 1. Close running processes
Write-Host "Stopping running instances of VeqloQuick..." -ForegroundColor Yellow
Get-Process -Name "VeqloQuick" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# 2. Prepare target directory
Write-Host "Installing to: $TargetDir" -ForegroundColor Yellow
if (-not (Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}
$targetAssets = Join-Path $TargetDir "Assets"
if (-not (Test-Path $targetAssets)) {
    New-Item -ItemType Directory -Path $targetAssets -Force | Out-Null
}

# 3. Copy files
Write-Host "Copying application binaries and tools..." -ForegroundColor Yellow
Copy-Item (Join-Path $packageDir "VeqloQuick.exe") -Destination $TargetDir -Force
Copy-Item (Join-Path $packageDir "VeqloQuickShell.dll") -Destination $TargetDir -Force
Copy-Item (Join-Path $packageDir "AppxManifest.xml") -Destination $TargetDir -Force
if (Test-Path (Join-Path $packageDir "cwebp.exe")) {
    Copy-Item (Join-Path $packageDir "cwebp.exe") -Destination $TargetDir -Force
}
if (Test-Path (Join-Path $packageDir "dwebp.exe")) {
    Copy-Item (Join-Path $packageDir "dwebp.exe") -Destination $TargetDir -Force
}

$srcAssets = Join-Path $packageDir "Assets"
if (Test-Path $srcAssets) {
    Copy-Item (Join-Path $srcAssets "*") -Destination $targetAssets -Force
}

$setupExe = Join-Path $projectRoot "VeqloQuickSetup.exe"
if (Test-Path $setupExe) {
    Copy-Item $setupExe -Destination (Join-Path $TargetDir "VeqloQuickSetup.exe") -Force
}

# 4. Register Windows 11 Sparse Package
$manifestPath = Join-Path $TargetDir "AppxManifest.xml"
Write-Host "Registering modern Windows 11 context menu package..." -ForegroundColor Yellow

$oldPkg = Get-AppxPackage -Name "Veqlo.Quick" -ErrorAction SilentlyContinue
if ($oldPkg) {
    Write-Host "Removing previous package registration: $($oldPkg.PackageFullName)" -ForegroundColor DarkGray
    Remove-AppxPackage -Package $oldPkg.PackageFullName
}

Add-AppxPackage -Register $manifestPath -ExternalLocation $TargetDir
Write-Host "[OK] Package registered successfully with Windows Package Manager." -ForegroundColor Green

# 5. Register Classic / Windows 10 Explorer Cascading Menu
Write-Host "Registering classic Explorer cascading context menu..." -ForegroundColor Yellow
$appExe = Join-Path $TargetDir "VeqloQuick.exe"
$associations = @(
    "HKCU:\Software\Classes\SystemFileAssociations\image\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.jpg\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.jpeg\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.png\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.webp\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.bmp\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.tiff\shell\VeqloQuick",
    "HKCU:\Software\Classes\SystemFileAssociations\.ico\shell\VeqloQuick"
)

foreach ($assoc in $associations) {
    if (-not (Test-Path $assoc)) { New-Item -Path $assoc -Force | Out-Null }
    Set-ItemProperty -Path $assoc -Name "MUIVerb" -Value "Veqlo Quick"
    Set-ItemProperty -Path $assoc -Name "SubCommands" -Value ""
    Set-ItemProperty -Path $assoc -Name "Icon" -Value "`"$appExe`",0"

    $shellKey = Join-Path $assoc "Shell"
    if (-not (Test-Path $shellKey)) { New-Item -Path $shellKey -Force | Out-Null }

    # Convert
    $convKey = Join-Path $shellKey "1_Convert"
    if (-not (Test-Path $convKey)) { New-Item -Path $convKey -Force | Out-Null }
    Set-ItemProperty -Path $convKey -Name "MUIVerb" -Value "Convert"
    $convCmd = Join-Path $convKey "command"
    if (-not (Test-Path $convCmd)) { New-Item -Path $convCmd -Force | Out-Null }
    Set-ItemProperty -Path $convCmd -Name "(Default)" -Value "`"$appExe`" convert --input `"%1`""

    # Compress
    $compKey = Join-Path $shellKey "2_Compress"
    if (-not (Test-Path $compKey)) { New-Item -Path $compKey -Force | Out-Null }
    Set-ItemProperty -Path $compKey -Name "MUIVerb" -Value "Compress"
    $compCmd = Join-Path $compKey "command"
    if (-not (Test-Path $compCmd)) { New-Item -Path $compCmd -Force | Out-Null }
    Set-ItemProperty -Path $compCmd -Name "(Default)" -Value "`"$appExe`" compress --input `"%1`""

    # Resize
    $resKey = Join-Path $shellKey "3_Resize"
    if (-not (Test-Path $resKey)) { New-Item -Path $resKey -Force | Out-Null }
    Set-ItemProperty -Path $resKey -Name "MUIVerb" -Value "Resize"
    $resCmd = Join-Path $resKey "command"
    if (-not (Test-Path $resCmd)) { New-Item -Path $resCmd -Force | Out-Null }
    Set-ItemProperty -Path $resCmd -Name "(Default)" -Value "`"$appExe`" resize --input `"%1`""

    # Inspect
    $inspKey = Join-Path $shellKey "4_Inspect"
    if (-not (Test-Path $inspKey)) { New-Item -Path $inspKey -Force | Out-Null }
    Set-ItemProperty -Path $inspKey -Name "MUIVerb" -Value "Inspect details"
    $inspCmd = Join-Path $inspKey "command"
    if (-not (Test-Path $inspCmd)) { New-Item -Path $inspCmd -Force | Out-Null }
    Set-ItemProperty -Path $inspCmd -Name "(Default)" -Value "`"$appExe`" inspect --input `"%1`""
}

# 6. Register in Windows Add/Remove Programs
Write-Host "Registering in Windows Add/Remove Programs..." -ForegroundColor Yellow
$regPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\VeqloQuick"
if (-not (Test-Path $regPath)) {
    New-Item -Path $regPath -Force | Out-Null
}
Set-ItemProperty -Path $regPath -Name "DisplayName" -Value "Veqlo Quick"
Set-ItemProperty -Path $regPath -Name "DisplayVersion" -Value "1.1.0"
Set-ItemProperty -Path $regPath -Name "Publisher" -Value "Veqlo"
Set-ItemProperty -Path $regPath -Name "InstallLocation" -Value $TargetDir
Set-ItemProperty -Path $regPath -Name "UninstallString" -Value "`"$TargetDir\VeqloQuickSetup.exe`" --uninstall"
Set-ItemProperty -Path $regPath -Name "DisplayIcon" -Value "$TargetDir\VeqloQuick.exe,0"
Set-ItemProperty -Path $regPath -Name "NoModify" -Value 1 -Type DWord
Set-ItemProperty -Path $regPath -Name "NoRepair" -Value 1 -Type DWord

if ($RestartExplorer) {
    Write-Host "Restarting File Explorer..." -ForegroundColor Yellow
    Stop-Process -Name explorer -Force
    Start-Sleep -Seconds 1
    Start-Process explorer.exe
}

Write-Host "`n[SUCCESS] Veqlo Quick v1.1 installed successfully!" -ForegroundColor Green
Write-Host "Right-click any image file in File Explorer to use Veqlo Quick." -ForegroundColor Cyan
