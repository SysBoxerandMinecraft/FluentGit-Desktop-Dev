# Building.ps1
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$Package
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
Set-Location $repoRoot

Write-Host "[FluentGit] Building $Configuration..." -ForegroundColor Cyan

# 杀掉残留进程
Get-Process FluentGit -ErrorAction SilentlyContinue | Stop-Process -Force

# 清理
dotnet clean FluentGit.csproj -c $Configuration | Out-Null

if ($Package) {
    dotnet publish FluentGit.csproj -c Release -p:Platform=x64 `
      -p:RuntimeIdentifier=win-x64 `
      -p:WindowsPackageType=MSIX `
      -p:WindowsAppSDKSelfContained=false `
      -p:GenerateAppxPackageOnBuild=true `
      -p:AppxPackageSigningEnabled=false `
      -p:AppxPackageAllowUnsigned=true
} else {
    dotnet build FluentGit.csproj -c $Configuration -p:Platform=x64
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "[FluentGit] Build FAILED (exit $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "[FluentGit] Build succeeded." -ForegroundColor Green