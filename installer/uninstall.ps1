# installer/uninstall.ps1
# Scripted uninstaller for Veqlo Quick v1.1

param(
    [string]$TargetDir = "$env:LOCALAPPDATA\Programs\Veqlo Quick",
    [switch]$RestartExplorer
)

$ErrorActionPreference = "SilentlyContinue"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " Veqlo Quick v1.1 - Automated Uninstallation " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# 1. Stop running processes
Get-Process -Name "VeqloQuick" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# 2. Unregister Windows 11 Package
$pkg = Get-AppxPackage -Name "Veqlo.Quick" -ErrorAction SilentlyContinue
if ($pkg) {
    Write-Host "Unregistering modern Windows 11 package: $($pkg.PackageFullName)" -ForegroundColor Yellow
    Remove-AppxPackage -Package $pkg.PackageFullName
}

# 3. Remove Classic Explorer Context Menu
Write-Host "Removing Explorer context menu registry entries..." -ForegroundColor Yellow
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
    if (Test-Path $assoc) {
        Remove-Item -Path $assoc -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# 4. Remove Add/Remove Programs key
Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\VeqloQuick" -Recurse -Force -ErrorAction SilentlyContinue

# 5. Remove Target Directory
if (Test-Path $TargetDir) {
    Write-Host "Removing installed files from $TargetDir..." -ForegroundColor Yellow
    Remove-Item -Path $TargetDir -Recurse -Force -ErrorAction SilentlyContinue
}

if ($RestartExplorer) {
    Write-Host "Restarting File Explorer..." -ForegroundColor Yellow
    Stop-Process -Name explorer -Force
    Start-Sleep -Seconds 1
    Start-Process explorer.exe
}

Write-Host "[OK] Veqlo Quick uninstalled completely." -ForegroundColor Green
