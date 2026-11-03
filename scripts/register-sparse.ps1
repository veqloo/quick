# scripts/register-sparse.ps1
# Registers Veqlo Quick as a Windows 11 Sparse Package with External Location

param(
    [switch]$RestartExplorer
)

$ErrorActionPreference = "Stop"

Write-Host "=== Veqlo Quick - Sparse Package Registration ===" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$packageDir = Join-Path $projectRoot "package"
$manifestPath = Join-Path $packageDir "AppxManifest.xml"

if (-not (Test-Path $manifestPath)) {
    throw "AppxManifest.xml not found at: $manifestPath"
}

Write-Host "Registering sparse package from manifest: $manifestPath" -ForegroundColor Yellow
Write-Host "External Location: $packageDir" -ForegroundColor Yellow

try {
    Add-AppxPackage -Register $manifestPath -ExternalLocation $packageDir
    Write-Host "[OK] Package registered successfully with Windows Package Manager." -ForegroundColor Green
} catch {
    Write-Error "Failed to register package: $_"
    exit 1
}

if ($RestartExplorer) {
    Write-Host "Restarting File Explorer to refresh modern context menu cache..." -ForegroundColor Yellow
    Stop-Process -Name explorer -Force
    Start-Sleep -Seconds 1
    Start-Process explorer.exe
    Write-Host "[OK] Explorer restarted." -ForegroundColor Green
} else {
    Write-Host "Tip: If the context menu doesn't update immediately, restart explorer.exe or re-login." -ForegroundColor DarkGray
}

Write-Host "=== Registration Complete ===" -ForegroundColor Cyan
