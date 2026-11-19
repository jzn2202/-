$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compiler)) {
    throw '找不到系统自带的 C# 编译器。'
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$source = Join-Path $root '源码'
$output = Join-Path $root '提醒小程序.exe'
$icon = Join-Path $root 'assets\reminder.ico'
$files = @(Get-ChildItem -LiteralPath $source -Filter '*.cs' | ForEach-Object FullName)
& $compiler /nologo /target:winexe /optimize+ /warn:4 /platform:anycpu "/out:$output" "/win32icon:$icon" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.dll /reference:System.Core.dll $files
if ($LASTEXITCODE -ne 0) { throw '编译失败。' }
Write-Host "已生成 $output"

