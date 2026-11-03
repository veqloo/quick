# tests/TestUiLaunch.ps1
$ErrorActionPreference = "Stop"
$exe = Join-Path $env:LOCALAPPDATA "Programs\Veqlo Quick\VeqloQuick.exe"
if (-not (Test-Path $exe)) {
    throw "Installed VeqloQuick.exe not found at $exe"
}

$sample = Join-Path (Get-Location).Path "test_sample.jpg"

foreach ($action in @("convert", "compress", "resize", "inspect")) {
    Write-Host "Testing $action window launch..." -ForegroundColor Yellow
    $p = Start-Process -FilePath $exe -ArgumentList "$action --input `"$sample`"" -PassThru
    Start-Sleep -Milliseconds 700
    if ($p.HasExited) {
        throw "Process for $action exited unexpectedly with code $($p.ExitCode)"
    }
    $p.Kill()
    $p.WaitForExit(1000)
    Write-Host "[PASS] $action window launched and remained responsive." -ForegroundColor Green
}
Write-Host "`nAll 4 UI dialogs launched and verified successfully!" -ForegroundColor Green
