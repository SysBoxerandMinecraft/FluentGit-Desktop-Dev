# Building.ps1
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [ValidateSet("x64", "ARM64")]
    [string]$Platform = "x64",

    [switch]$Package
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
Set-Location $repoRoot

Write-Host "[FluentGit] Building $Configuration | $Platform..." -ForegroundColor Cyan

# 杀掉可能残留的进程，避免文件占用
Get-Process FluentGit -ErrorAction SilentlyContinue | Stop-Process -Force

# 清理
dotnet clean FluentGit.csproj -c $Configuration | Out-Null

# 根据平台决定 RuntimeIdentifier
$rid = if ($Platform -eq "ARM64") { "win-arm64" } else { "win-x64" }

if ($Package) {
    # 打 MSIX，跳过签名（自包含发布，.NET 运行时已打进包里）
    dotnet publish FluentGit.csproj -c Release -p:Platform=$Platform `
      -p:RuntimeIdentifier=$rid `
      -p:WindowsPackageType=MSIX `
      -p:GenerateAppxPackageOnBuild=true `
      -p:AppxPackageSigningEnabled=false `
      -p:AppxPackageAllowUnsigned=true
} else {
    # 日常编译
    dotnet build FluentGit.csproj -c $Configuration -p:Platform=$Platform
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "[FluentGit] Build FAILED (exit $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "[FluentGit] Build succeeded." -ForegroundColor Green