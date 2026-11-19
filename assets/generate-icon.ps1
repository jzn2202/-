$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$side = 64
$bitmap = [System.Drawing.Bitmap]::new($side, $side)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)

$tile = [System.Drawing.Rectangle]::new(2, 2, 60, 60)
$path = [System.Drawing.Drawing2D.GraphicsPath]::new()
$path.AddArc(2, 2, 18, 18, 180, 90)
$path.AddArc(44, 2, 18, 18, 270, 90)
$path.AddArc(44, 44, 18, 18, 0, 90)
$path.AddArc(2, 44, 18, 18, 90, 90)
$path.CloseFigure()
$fill = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(17, 65, 79))
$outline = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(84, 216, 229), 2.5)
$clock = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(220, 249, 251), 4)
$hand = [System.Drawing.Pen]::new([System.Drawing.Color]::White, 4)
$dot = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(84, 216, 229))
$graphics.FillPath($fill, $path)
$graphics.DrawPath($outline, $path)
$graphics.DrawEllipse($clock, 18, 18, 28, 28)
$graphics.DrawLine($hand, 32, 32, 32, 23)
$graphics.DrawLine($hand, 32, 32, 38, 36)
$graphics.FillEllipse($dot, 29, 10, 6, 6)

$iconHandle = $bitmap.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($iconHandle)
$output = Join-Path $PSScriptRoot 'reminder.ico'
$stream = [System.IO.File]::Create($output)
try { $icon.Save($stream) } finally { $stream.Dispose() }
$icon.Dispose()
$dot.Dispose()
$hand.Dispose()
$clock.Dispose()
$outline.Dispose()
$fill.Dispose()
$path.Dispose()
$graphics.Dispose()
$bitmap.Dispose()
Write-Host "已生成 $output"
