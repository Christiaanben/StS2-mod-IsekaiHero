# Runtime packaging only: resize generated RGBA art and derive white silhouette masks.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$relicRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../IsekaiHero/images/relics'))
foreach ($name in 'beginners_luck_charm', 'op_smartphone', 'forbidden_walkthrough') {
    $source = [Drawing.Bitmap]::FromFile((Join-Path $PSScriptRoot "../docs/art-prompts/relic-sources/$name.png"))
    try {
        foreach ($size in 94, 256) {
            $bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $graphics = [Drawing.Graphics]::FromImage($bitmap)
            try {
                $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.DrawImage($source, [Drawing.Rectangle]::new(0, 0, $size, $size))
                $relative = if ($size -eq 94) { "$name.png" } else { "big/$name.png" }
                $bitmap.Save((Join-Path $relicRoot $relative), [Drawing.Imaging.ImageFormat]::Png)
                if ($size -eq 94) {
                    # Expand alpha by two pixels for the relic selection silhouette.
                    $outline = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
                    try {
                        for ($y = 0; $y -lt $size; $y++) {
                            for ($x = 0; $x -lt $size; $x++) {
                                $alpha = 0
                                for ($dy = -2; $dy -le 2; $dy++) {
                                    for ($dx = -2; $dx -le 2; $dx++) {
                                        $sx = $x + $dx; $sy = $y + $dy
                                        if ($sx -ge 0 -and $sx -lt $size -and $sy -ge 0 -and $sy -lt $size) {
                                            $alpha = [Math]::Max($alpha, $bitmap.GetPixel($sx, $sy).A)
                                        }
                                    }
                                }
                                $outline.SetPixel($x, $y, [Drawing.Color]::FromArgb($alpha, 255, 255, 255))
                            }
                        }
                        $outline.Save((Join-Path $relicRoot "${name}_outline.png"), [Drawing.Imaging.ImageFormat]::Png)
                    } finally { $outline.Dispose() }
                }
            } finally { $graphics.Dispose(); $bitmap.Dispose() }
        }
    } finally { $source.Dispose() }
}
