$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compiler)) { throw '找不到系统自带的 C# 编译器。' }
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$output = Join-Path $root 'tests\UiSmoke.exe'
$files = @(Get-ChildItem -LiteralPath (Join-Path $root '源码') -Filter '*.cs' | ForEach-Object FullName)
$files += Join-Path $root 'tests\UiSmoke.cs'
& $compiler /nologo /target:exe "/out:$output" /main:UiSmoke /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.dll /reference:System.Core.dll $files
if ($LASTEXITCODE -ne 0) { throw '界面检查编译失败。' }
& $output
if ($LASTEXITCODE -ne 0) { throw '界面检查失败。' }
