# scripts/unregister-sparse.ps1
# Unregisters Veqlo Quick sparse package from Windows

param(
    [switch]$RestartExplorer
)

$ErrorActionPreference = "Stop"

Write-Host "=== Veqlo Quick - Sparse Package Removal ===" -ForegroundColor Cyan

$packageName = "Veqlo.Quick"

$installed = Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue

if ($installed) {
    Write-Host "Removing package: $($installed.PackageFullName)..." -ForegroundColor Yellow
    Remove-AppxPackage -Package $installed.PackageFullName
    Write-Host "[OK] Package removed successfully." -ForegroundColor Green
} else {
    Write-Host "Package $packageName is not currently registered." -ForegroundColor DarkGray
}

if ($RestartExplorer) {
    Write-Host "Restarting File Explorer to refresh context menu..." -ForegroundColor Yellow
    Stop-Process -Name explorer -Force
    Start-Sleep -Seconds 1
    Start-Process explorer.exe
    Write-Host "[OK] Explorer restarted." -ForegroundColor Green
}

Write-Host "=== Unregistration Complete ===" -ForegroundColor Cyan
