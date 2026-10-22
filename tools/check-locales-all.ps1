# 多语言词表校验：依次检查本仓三个程序集的 Locales 与源码消息一致性
# 用法：pwsh -File tools/check-locales-all.ps1
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$script = Join-Path $PSScriptRoot "check-locales.ps1"
$projects = @("NewLife.JT1078", "NewLife.JT1078.Server", "NewLife.JT1078.Web")

$failed = 0
foreach ($project in $projects) {
    Write-Host "=== $project ==="
    & pwsh -NoProfile -File $script -LocaleDir (Join-Path (Join-Path $root $project) "Locales") -SourceDir (Join-Path $root $project)
    if ($LASTEXITCODE -ne 0) { $failed++ }
}

if ($failed -gt 0) {
    Write-Host "多语言词表校验未通过：$failed 个程序集" -ForegroundColor Red
    exit 1
}

# README 徽章上的静态语言数必须与核心库词表一致，防止语言增减后徽章漂移
$readme = Join-Path $root "README.md"
if (Test-Path $readme) {
    $match = [regex]::Match([IO.File]::ReadAllText($readme), 'badge/locales-(\d+)')
    if ($match.Success) {
        $expected = [int]$match.Groups[1].Value
        $actual = @(Get-ChildItem (Join-Path (Join-Path $root "NewLife.JT1078") "Locales") -Filter *.yml | Where-Object { $_.BaseName -ne "en" }).Count
        if ($actual -ne $expected) {
            Write-Host "README 徽章 locales-$expected 与核心库词表 $actual 种语言不一致" -ForegroundColor Red
            exit 1
        }
    }
}

Write-Host "多语言词表校验通过：$($projects.Count) 个程序集" -ForegroundColor Green
