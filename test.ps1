$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compiler)) { throw '找不到系统自带的 C# 编译器。' }
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$output = Join-Path $root 'tests\TimerTests.exe'
& $compiler /nologo /target:exe "/out:$output" /reference:System.Xml.dll (Join-Path $root '源码\TimerEngine.cs') (Join-Path $root '源码\SettingsStore.cs') (Join-Path $root 'tests\TimerTests.cs')
if ($LASTEXITCODE -ne 0) { throw '测试编译失败。' }
& $output
if ($LASTEXITCODE -ne 0) { throw '计时检查失败。' }

