Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path -Parent $PSScriptRoot
$assetsDir = Join-Path $projectRoot "src\CDriveCleanupMaster.App\Assets"
$pngPath = Join-Path $assetsDir "app-icon.png"
$icoPath = Join-Path $assetsDir "app-icon.ico"

New-Item -ItemType Directory -Force -Path $assetsDir | Out-Null

function Draw-LogoBitmap([int]$size) {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

    $accent = [System.Drawing.Color]::FromArgb(255, 230, 126, 34)
    $radius = [int]($size * 0.25)
    $body = New-Object System.Drawing.Drawing2D.GraphicsPath
    $rect = New-Object System.Drawing.Rectangle 0, 0, ($size - 1), ($size - 1)
    $body.AddArc($rect.X, $rect.Y, $radius * 2, $radius * 2, 180, 90)
    $body.AddArc($rect.Right - $radius * 2, $rect.Y, $radius * 2, $radius * 2, 270, 90)
    $body.AddArc($rect.Right - $radius * 2, $rect.Bottom - $radius * 2, $radius * 2, $radius * 2, 0, 90)
    $body.AddArc($rect.X, $rect.Bottom - $radius * 2, $radius * 2, $radius * 2, 90, 90)
    $body.CloseFigure()
    $graphics.FillPath((New-Object System.Drawing.SolidBrush $accent), $body)

    $diskSize = if ($size -le 24) { [int]($size * 0.58) } else { [int]($size * 0.62) }
    $diskX = [int](($size - $diskSize) / 2)
    $diskY = [int](($size - $diskSize) / 2)
    $graphics.FillEllipse([System.Drawing.Brushes]::White, $diskX, $diskY, $diskSize, $diskSize)

    if ($size -le 24) {
        $fontSize = [single]($size * 0.52)
    }
    else {
        $fontSize = [single]($size * 0.42)
    }
    $font = [System.Drawing.Font]::new("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $graphics.DrawString("C", $font, (New-Object System.Drawing.SolidBrush $accent), ($size / 2), ($size / 2 + 1), $format)

    if ($size -gt 24) {
        $sparkPen = New-Object System.Drawing.Pen ([System.Drawing.Brushes]::White), ([float]($size * 0.05))
        $sparkPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
        $sparkPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $sx = $size * 0.72
        $sy = $size * 0.18
        $len = $size * 0.08
        $graphics.DrawLine($sparkPen, $sx, $sy, $sx, $sy + $len)
        $graphics.DrawLine($sparkPen, $sx - $len / 2, $sy + $len / 2, $sx + $len / 2, $sy + $len / 2)
    }

    $graphics.Dispose()
    return $bitmap
}

function Save-MultiSizePngIcon {
    param(
        [string]$OutputPath,
        [int[]]$Sizes = @(16, 24, 32, 48, 64, 128, 256)
    )

    $entries = foreach ($size in $Sizes) {
        $bitmap = Draw-LogoBitmap $size
        $stream = New-Object IO.MemoryStream
        $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        $bytes = $stream.ToArray()
        $bitmap.Dispose()
        $stream.Dispose()
        [PSCustomObject]@{ Size = $size; Bytes = $bytes }
    }

    $output = New-Object IO.MemoryStream
    $writer = New-Object IO.BinaryWriter $output
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$entries.Count)

    $dataOffset = 6 + (16 * $entries.Count)
    foreach ($entry in $entries) {
        $dimension = if ($entry.Size -ge 256) { [byte]0 } else { [byte]$entry.Size }
        $writer.Write($dimension)
        $writer.Write($dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$entry.Bytes.Length)
        $writer.Write([uint32]$dataOffset)
        $dataOffset += $entry.Bytes.Length
    }

    foreach ($entry in $entries) {
        $writer.Write($entry.Bytes)
    }

    [IO.File]::WriteAllBytes($OutputPath, $output.ToArray())
    $writer.Dispose()
    $output.Dispose()
}

$png = Draw-LogoBitmap 256
$png.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$png.Dispose()

Save-MultiSizePngIcon -OutputPath $icoPath

Write-Host "Generated: $pngPath"
Write-Host "Generated: $icoPath"
