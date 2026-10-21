<#
.SYNOPSIS
  校验发布包：目标框架资产、符号包以及 README/图标/清单文件齐全。
.DESCRIPTION
  在 CI 与本地打包后执行，防止目标框架增减未同步文档，或发布包缺少必要资产。
.PARAMETER Path
  nupkg/snupkg 所在目录，默认 out。
.EXAMPLE
  pwsh -File ./tools/check-packages.ps1
#>
param(
    [String]$Path = 'out'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

# 目标框架资产约定：变更目标框架时必须同步本表与 README、架构设计
$expected = @{
    'NewLife.JT1078'        = @('net461', 'netstandard2.0', 'netstandard2.1', 'net8.0', 'net10.0')
    'NewLife.JT1078.Server' = @('netstandard2.0', 'netstandard2.1', 'net6.0', 'net8.0', 'net10.0')
}

foreach ($id in $expected.Keys) {
    $escaped = [regex]::Escape($id)
    $pkg = Get-ChildItem (Join-Path $Path "$id.*.nupkg") |
        Where-Object { $_.Name -match "^$escaped\.\d+\.\d+" -and $_.Name -notlike '*.snupkg' } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $pkg) { throw "未找到 $id 的 nupkg" }

    $symbolPkg = Get-ChildItem (Join-Path $Path "$id.*.snupkg") |
        Where-Object { $_.Name -match "^$escaped\.\d+\.\d+" } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $symbolPkg) { throw "未找到 $id 的 snupkg 符号包" }

    $archive = [System.IO.Compression.ZipFile]::OpenRead($pkg.FullName)
    try {
        $entries = $archive.Entries | ForEach-Object { $_.FullName }
        $actual = $entries |
            Where-Object { $_ -match "^lib/([^/]+)/$escaped\.dll$" } |
            ForEach-Object { ($_ -split '/')[1] } |
            Sort-Object -Unique

        Write-Host "$id 目标框架：$($actual -join ', ')"
        $missing = $expected[$id] | Where-Object { $actual -notcontains $_ }
        $extra = $actual | Where-Object { $expected[$id] -notcontains $_ }
        if ($missing) { throw "$id 缺少目标框架资产：$($missing -join ', ')" }
        if ($extra) { throw "$id 出现未约定的目标框架资产：$($extra -join ', ')（确认新增时同步本脚本、README 与架构设计）" }
        foreach ($required in @('README.nuget.md', 'leaf.png', "$id.nuspec")) {
            if ($entries -notcontains $required) { throw "$id 包缺少 $required" }
        }
    }
    finally {
        $archive.Dispose()
    }
}

Write-Host 'nupkg 与 snupkg 资产校验通过'
