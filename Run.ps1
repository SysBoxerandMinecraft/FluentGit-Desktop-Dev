# Run.ps1
# Build and launch FluentGit with live log streaming
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [ValidateSet("x64", "ARM64")]
    [string]$Platform = "x64",

    [switch]$NoBuild,
    [switch]$NoWatch
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
Set-Location $repoRoot

$rid = if ($Platform -eq "ARM64") { "win-arm64" } else { "win-x64" }
$exePath = Join-Path $repoRoot "bin\$Platform\$Configuration\net10.0-windows10.0.26100.0\$rid\FluentGit.exe"
$logFile = Join-Path $env:LOCALAPPDATA "FluentGit\logs\app.log"

Write-Host ""
Write-Host "========================================" -ForegroundColor DarkGray
Write-Host " FluentGit Run Script" -ForegroundColor Cyan
Write-Host " Config   : $Configuration | $Platform" -ForegroundColor DarkGray
Write-Host " Exe      : $exePath" -ForegroundColor DarkGray
Write-Host " LogFile  : $logFile" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor DarkGray
Write-Host ""

# ========== 1. Kill running instance ==========
$running = Get-Process FluentGit -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "[Run] Killing running FluentGit..." -ForegroundColor Yellow
    $running | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

# ========== 2. Build ==========
if (-not $NoBuild) {
    Write-Host "[Run] Building $Configuration | $Platform..." -ForegroundColor Cyan
    dotnet build FluentGit.csproj -c $Configuration -p:Platform=$Platform

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[Run] Build FAILED (exit $LASTEXITCODE)" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "[Run] Build succeeded." -ForegroundColor Green
    Write-Host ""
}

# ========== 3. Check exe ==========
if (-not (Test-Path $exePath)) {
    Write-Host "[Run] exe not found: $exePath" -ForegroundColor Red
    Write-Host "[Run] Please build first: ./Building.ps1" -ForegroundColor Yellow
    exit 1
}

# ========== 4. Clear old log ==========
try {
    $logDir = Split-Path $logFile -Parent
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Set-Content -Path $logFile -Value "" -Encoding UTF8
    Write-Host "[Run] Old log cleared" -ForegroundColor DarkGray
}
catch {
    Write-Host "[Run] Failed to clear log (ignored): $_" -ForegroundColor Yellow
}

# ========== 5. Launch ==========
Write-Host "[Run] Launching app..." -ForegroundColor Green
$proc = Start-Process -FilePath $exePath -PassThru

if (-not $proc) {
    Write-Host "[Run] Launch failed" -ForegroundColor Red
    exit 1
}

Write-Host "[Run] PID: $($proc.Id)" -ForegroundColor DarkGray
Write-Host ""

# ========== 6. Watch log ==========
if (-not $NoWatch) {
    Write-Host "========================================" -ForegroundColor DarkGray
    Write-Host " Live log (Ctrl+C to stop watching)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor DarkGray
    Write-Host ""

    $waited = 0
    while (-not (Test-Path $logFile) -and $waited -lt 20) {
        Start-Sleep -Milliseconds 250
        $waited++
    }

    try {
        Get-Content $logFile -Wait -Tail 0 | ForEach-Object {
            if ($_ -match '\[Error\s*\]') {
                Write-Host $_ -ForegroundColor Red
            }
            elseif ($_ -match '\[Warning\]') {
                Write-Host $_ -ForegroundColor Yellow
            }
            elseif ($_ -match '\[OK\s*\]') {
                Write-Host $_ -ForegroundColor Green
            }
            elseif ($_ -match '\[Info\s*\]') {
                Write-Host $_ -ForegroundColor Gray
            }
            else {
                Write-Host $_
            }
        }
    }
    catch [System.Management.Automation.PipelineStoppedException] {
        # Ctrl+C, normal exit
    }
}

# ========== 7. Wait for app ==========
if (-not $proc.HasExited) {
    Write-Host ""
    Write-Host "[Run] Waiting for app to exit..." -ForegroundColor DarkGray
    $proc.WaitForExit()
}

Write-Host ""
Write-Host "[Run] App exited, code: $($proc.ExitCode)" -ForegroundColor Gray