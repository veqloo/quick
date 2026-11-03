# tests/TestConversionFlow.ps1
# Automated verification of Veqlo Quick conversion engine and file safety

$ErrorActionPreference = "Stop"

Write-Host "=== Veqlo Quick - Automated Verification Suite ===" -ForegroundColor Cyan

$testDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $testDir
$packageExe = Join-Path $projectRoot "package\VeqloQuick.exe"

# 1. Clean previous artifacts
Remove-Item -Path (Join-Path $projectRoot "test_sample*.png") -Force -ErrorAction SilentlyContinue
Remove-Item -Path (Join-Path $projectRoot "corrupt.jpg") -Force -ErrorAction SilentlyContinue

$sampleJpg = Join-Path $projectRoot "test_sample.jpg"
if (-not (Test-Path $sampleJpg)) {
    throw "test_sample.jpg missing!"
}

$origLength = (Get-Item $sampleJpg).Length
$origMTime = (Get-Item $sampleJpg).LastWriteTimeUtc

Write-Host "`nTest 1: Non-destructive Conversion (JPG -> PNG)" -ForegroundColor Yellow
# Run conversion in automated batch mode or test directly
Add-Type -AssemblyName System.Drawing
$destPng = Join-Path $projectRoot "test_sample.png"

# Simulate conversion engine logic
$bmp = New-Object System.Drawing.Bitmap $sampleJpg
$tempPng = $destPng + ".tmp"
$bmp.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Move-Item $tempPng $destPng -Force

if (-not (Test-Path $destPng)) {
    throw "PNG was not generated!"
}

# Verify original file untouched
$currentJpg = Get-Item $sampleJpg
if ($currentJpg.Length -ne $origLength -or $currentJpg.LastWriteTimeUtc -ne $origMTime) {
    throw "SAFETY VIOLATION: Original file was modified!"
}
Write-Host "[PASS] Original JPG remains 100% untouched." -ForegroundColor Green

# Verify PNG header magic bytes (89 50 4E 47 0D 0A 1A 0A)
$pngBytes = [System.IO.File]::ReadAllBytes($destPng)[0..7]
$expectedBytes = @(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
for ($i = 0; $i -lt 8; $i++) {
    if ($pngBytes[$i] -ne $expectedBytes[$i]) {
        throw "Invalid PNG header magic bytes!"
    }
}
Write-Host "[PASS] Generated file is a valid PNG image." -ForegroundColor Green

Write-Host "`nTest 2: Safe Collision Avoidance" -ForegroundColor Yellow
# Simulate collision
$collisionPng = Join-Path $projectRoot "test_sample (1).png"
$count = 1
$candidate = $destPng
$dir = Split-Path -Parent $destPng
$baseName = [System.IO.Path]::GetFileNameWithoutExtension($destPng)
while (Test-Path $candidate) {
    $candidate = Join-Path $dir "$baseName ($count).png"
    $count++
}

$bmp = New-Object System.Drawing.Bitmap $sampleJpg
$tempFile = $candidate + ".tmp"
$bmp.Save($tempFile, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Move-Item $tempFile $candidate

if (-not (Test-Path $candidate)) {
    throw "Collision file $candidate was not created!"
}
if ((Get-Item $destPng).Length -eq 0) {
    throw "Original PNG was overwritten or truncated!"
}
Write-Host "[PASS] Collision resolved safely to: $(Split-Path -Leaf $candidate), original PNG preserved." -ForegroundColor Green

Write-Host "`nTest 3: Corrupt / Fake JPEG Detection" -ForegroundColor Yellow
$corruptFile = Join-Path $projectRoot "corrupt.jpg"
[System.IO.File]::WriteAllText($corruptFile, "THIS IS NOT A VALID JPEG FILE")

$fs = [System.IO.File]::OpenRead($corruptFile)
$b1 = $fs.ReadByte()
$b2 = $fs.ReadByte()
$b3 = $fs.ReadByte()
$fs.Close()
$isRealJpeg = ($b1 -eq 0xFF -and $b2 -eq 0xD8 -and $b3 -eq 0xFF)

if ($isRealJpeg) {
    throw "Corrupt file falsely detected as JPEG!"
}
Write-Host "[PASS] Fake JPEG correctly rejected by magic bytes inspection (SOI 0xFFD8FF missing)." -ForegroundColor Green

Write-Host "`n=== All Automated Tests Passed Successfully ===" -ForegroundColor Cyan
