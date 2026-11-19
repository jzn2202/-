$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function New-IconPng([int]$side) {
    $bitmap = [System.Drawing.Bitmap]::new($side, $side)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.ScaleTransform($side / 64.0, $side / 64.0)

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

    $stream = [System.IO.MemoryStream]::new()
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $bytes = $stream.ToArray()
    $stream.Dispose()
    $dot.Dispose()
    $hand.Dispose()
    $clock.Dispose()
    $outline.Dispose()
    $fill.Dispose()
    $path.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
    return ,$bytes
}

$sizes = @(16, 32, 48, 256)
$images = @($sizes | ForEach-Object { New-IconPng $_ })
$output = Join-Path $PSScriptRoot 'reminder.ico'
$stream = [System.IO.File]::Create($output)
$writer = [System.IO.BinaryWriter]::new($stream)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$sizes.Count)
    $offset = 6 + 16 * $sizes.Count
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $writer.Write([byte]($sizes[$i] % 256))
        $writer.Write([byte]($sizes[$i] % 256))
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$images[$i].Length)
        $writer.Write([uint32]$offset)
        $offset += $images[$i].Length
    }
    foreach ($bytes in $images) { $writer.Write([byte[]]$bytes) }
}
finally {
    $writer.Dispose()
    $stream.Dispose()
}
Write-Host "已生成 $output"
