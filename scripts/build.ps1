# scripts/build.ps1
# Builds Veqlo Quick v1.1:
# - package\VeqloQuick.exe (WPF Fluent Mini UI & Image Engine)
# - package\VeqloQuickShell.dll (C++20 COM Shell Extension)
# - VeqloQuickSetup.exe (WPF Fluent Standalone Installer)

$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "   Building Veqlo Quick v1.1                 " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$packageDir = Join-Path $projectRoot "package"

$cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$wpfCore = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll"
$wpfFramework = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll"
$wpfBase = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll"

# 1. Compile Mini UI Executable (WPF)
Write-Host "[1/3] Compiling VeqloQuick.exe (WPF Fluent UI)..." -ForegroundColor Yellow
$appOut = Join-Path $packageDir "VeqloQuick.exe"
$appSources = @(
    (Join-Path $projectRoot "src\app\ImageEngine.cs"),
    (Join-Path $projectRoot "src\app\WpfTheme.cs"),
    (Join-Path $projectRoot "src\app\ConvertWindow.cs"),
    (Join-Path $projectRoot "src\app\CompressWindow.cs"),
    (Join-Path $projectRoot "src\app\ResizeWindow.cs"),
    (Join-Path $projectRoot "src\app\InspectWindow.cs"),
    (Join-Path $projectRoot "src\app\Program.cs")
)

& $cscPath /target:winexe /out:$appOut /r:System.dll,System.Drawing.dll,$wpfCore,$wpfFramework,$wpfBase,System.Xaml.dll @appSources
if ($LASTEXITCODE -ne 0) {
    throw "Failed to compile VeqloQuick.exe"
}
Write-Host "[OK] VeqloQuick.exe built successfully ($((Get-Item $appOut).Length) bytes)." -ForegroundColor Green

# 2. Compile Shell Extension DLL (g++)
Write-Host "[2/3] Compiling VeqloQuickShell.dll (C++20 COM Extension)..." -ForegroundColor Yellow
$shellSrcDir = Join-Path $projectRoot "src\shell-extension"
$dllOut = Join-Path $packageDir "VeqloQuickShell.dll"
$defFile = Join-Path $shellSrcDir "ShellExtension.def"

$cppFiles = @(
    (Join-Path $shellSrcDir "DllMain.cpp"),
    (Join-Path $shellSrcDir "ClassFactory.cpp"),
    (Join-Path $shellSrcDir "ExplorerCommand.cpp")
)

& g++ -O2 -shared -s -o $dllOut @cppFiles $defFile -lshlwapi -lole32 -loleaut32 -luuid -static
if ($LASTEXITCODE -ne 0) {
    throw "Failed to compile VeqloQuickShell.dll"
}
Write-Host "[OK] VeqloQuickShell.dll built successfully ($((Get-Item $dllOut).Length) bytes)." -ForegroundColor Green

# 3. Compile Standalone Installer (VeqloQuickSetup.exe)
Write-Host "[3/3] Building self-contained VeqloQuickSetup.exe (WPF Fluent Installer)..." -ForegroundColor Yellow
$tempZip = Join-Path $env:TEMP "veqlo_quick_payload.zip"
if (Test-Path $tempZip) { Remove-Item $tempZip -Force }

Compress-Archive -Path (Join-Path $packageDir "*") -DestinationPath $tempZip -Force

$setupSrc = Join-Path $projectRoot "installer\VeqloQuickSetup.cs"
$setupOut = Join-Path $projectRoot "VeqloQuickSetup.exe"

& $cscPath /target:winexe /out:$setupOut /r:System.dll,System.Windows.Forms.dll,System.IO.Compression.dll,System.IO.Compression.FileSystem.dll,$wpfCore,$wpfFramework,$wpfBase,System.Xaml.dll /resource:"$tempZip,VeqloQuickPayload" $setupSrc
if ($LASTEXITCODE -ne 0) {
    throw "Failed to compile VeqloQuickSetup.exe"
}
if (Test-Path $tempZip) { Remove-Item $tempZip -Force }

Write-Host "[OK] VeqloQuickSetup.exe built successfully ($((Get-Item $setupOut).Length) bytes)." -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "   Build Complete! Deliverable: VeqloQuickSetup.exe" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Cyan
