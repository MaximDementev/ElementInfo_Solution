#Requires -Version 5.1
# Generate ribbon icons for ElementInfo Revit plugin commands (local drawing only).

Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'

# --- drawing helpers ---

function ConvertFrom-ColorHex {
    param([string]$Hex)
    $h = $Hex.TrimStart('#')
    if ($h.Length -eq 6) {
        $r = [Convert]::ToByte($h.Substring(0, 2), 16)
        $g = [Convert]::ToByte($h.Substring(2, 2), 16)
        $b = [Convert]::ToByte($h.Substring(4, 2), 16)
        return [System.Drawing.Color]::FromArgb(255, $r, $g, $b)
    }
    if ($h.Length -eq 8) {
        $a = [Convert]::ToByte($h.Substring(0, 2), 16)
        $r = [Convert]::ToByte($h.Substring(2, 2), 16)
        $g = [Convert]::ToByte($h.Substring(4, 2), 16)
        $b = [Convert]::ToByte($h.Substring(6, 2), 16)
        return [System.Drawing.Color]::FromArgb($a, $r, $g, $b)
    }
    throw "Invalid color hex: $Hex"
}

function Get-Scaled {
    param([double]$Value, [double]$Scale)
    return [int][Math]::Round($Value * $Scale)
}

function Draw-IconShapes {
    param(
        [System.Drawing.Graphics]$Graphics,
        [object[]]$Shapes,
        [double]$Scale
    )

    $Graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $Graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $Graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $Graphics.Clear([System.Drawing.Color]::Transparent)

    foreach ($shape in $Shapes) {
        $type = [string]$shape.type
        $color = ConvertFrom-ColorHex ([string]$shape.color)
        $stroke = 2.0
        if ($null -ne $shape.stroke) { $stroke = [double]$shape.stroke }
        $penWidth = [Math]::Max(1.0, $stroke * $Scale)

        switch ($type) {
            'fillRect' {
                $x = Get-Scaled $shape.x $Scale
                $y = Get-Scaled $shape.y $Scale
                $w = Get-Scaled $shape.w $Scale
                $h = Get-Scaled $shape.h $Scale
                $brush = New-Object System.Drawing.SolidBrush $color
                $Graphics.FillRectangle($brush, $x, $y, $w, $h)
                $brush.Dispose()
            }
            'rect' {
                $x = Get-Scaled $shape.x $Scale
                $y = Get-Scaled $shape.y $Scale
                $w = Get-Scaled $shape.w $Scale
                $h = Get-Scaled $shape.h $Scale
                $pen = New-Object System.Drawing.Pen $color, $penWidth
                $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
                $Graphics.DrawRectangle($pen, $x, $y, $w, $h)
                $pen.Dispose()
            }
            'fillEllipse' {
                $x = Get-Scaled $shape.x $Scale
                $y = Get-Scaled $shape.y $Scale
                $w = Get-Scaled $shape.w $Scale
                $h = Get-Scaled $shape.h $Scale
                $brush = New-Object System.Drawing.SolidBrush $color
                $Graphics.FillEllipse($brush, $x, $y, $w, $h)
                $brush.Dispose()
            }
            'ellipse' {
                $x = Get-Scaled $shape.x $Scale
                $y = Get-Scaled $shape.y $Scale
                $w = Get-Scaled $shape.w $Scale
                $h = Get-Scaled $shape.h $Scale
                $pen = New-Object System.Drawing.Pen $color, $penWidth
                $Graphics.DrawEllipse($pen, $x, $y, $w, $h)
                $pen.Dispose()
            }
            'line' {
                $x1 = Get-Scaled $shape.x1 $Scale
                $y1 = Get-Scaled $shape.y1 $Scale
                $x2 = Get-Scaled $shape.x2 $Scale
                $y2 = Get-Scaled $shape.y2 $Scale
                $pen = New-Object System.Drawing.Pen $color, $penWidth
                $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
                $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
                $Graphics.DrawLine($pen, $x1, $y1, $x2, $y2)
                $pen.Dispose()
            }
            'polygon' {
                $pts = @()
                foreach ($p in $shape.points) {
                    $pts += New-Object System.Drawing.PointF (
                        [single](Get-Scaled $p[0] $Scale),
                        [single](Get-Scaled $p[1] $Scale)
                    )
                }
                if ($shape.fill -eq $true) {
                    $brush = New-Object System.Drawing.SolidBrush $color
                    $Graphics.FillPolygon($brush, $pts)
                    $brush.Dispose()
                }
                else {
                    $pen = New-Object System.Drawing.Pen $color, $penWidth
                    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
                    $Graphics.DrawPolygon($pen, $pts)
                    $pen.Dispose()
                }
            }
            default {
                Write-Warning "Unknown shape type: $type"
            }
        }
    }
}

function Save-CommandIcons {
    param(
        [string]$OutputFolder,
        [string]$CommandName,
        [object[]]$Shapes
    )

    $saved = @()

    foreach ($size in @(32, 16)) {
        $scale = $size / 32.0
        $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        try {
            Draw-IconShapes -Graphics $g -Shapes $Shapes -Scale $scale
            $path = Join-Path $OutputFolder ("{0}_{1}.png" -f $CommandName, $size)
            $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
            $saved += $path
        }
        finally {
            $g.Dispose()
            $bmp.Dispose()
        }
    }

    return $saved
}

# --- editable output folder ---

$outputFolder = 'C:\Users\Dementev\AppData\Roaming\Neuroptera\Plugins\ElementInfo\2.0.0\icons'

# --- command icon definitions (geometry on 32x32 canvas, origin top-left) ---
# Colors: Autodesk-friendly blue + amber accent for light/dark ribbon contrast.

$Blue   = '#0696D7'
$Amber  = '#E36C09'
$Ink    = '#2A2A2A'
$White  = '#FFFFFF'
$Teal   = '#1B7A6E'

$commands = @(
    @{
        # Get info about multiple selected elements
        Name = 'GetMultipleElementsInfo'
        Shapes = @(
            @{ type = 'fillRect'; x = 4;  y = 8;  w = 16; h = 18; color = $White }
            @{ type = 'rect';     x = 4;  y = 8;  w = 16; h = 18; color = $Ink; stroke = 2 }
            @{ type = 'fillRect'; x = 7;  y = 5;  w = 16; h = 18; color = $White }
            @{ type = 'rect';     x = 7;  y = 5;  w = 16; h = 18; color = $Blue; stroke = 2 }
            @{ type = 'line';     x1 = 10; y1 = 11; x2 = 19; y2 = 11; color = $Blue; stroke = 2 }
            @{ type = 'line';     x1 = 10; y1 = 15; x2 = 19; y2 = 15; color = $Blue; stroke = 2 }
            @{ type = 'line';     x1 = 10; y1 = 19; x2 = 16; y2 = 19; color = $Blue; stroke = 2 }
            @{ type = 'fillEllipse'; x = 20; y = 18; w = 10; h = 10; color = $Amber }
            @{ type = 'fillRect'; x = 24; y = 20; w = 2; h = 2; color = $White }
            @{ type = 'fillRect'; x = 24; y = 23; w = 2; h = 4; color = $White }
        )
    },
    @{
        # Get ElementId values from selection
        Name = 'GetElementsId'
        Shapes = @(
            @{ type = 'fillRect'; x = 5;  y = 6;  w = 22; h = 20; color = $White }
            @{ type = 'rect';     x = 5;  y = 6;  w = 22; h = 20; color = $Ink; stroke = 2 }
            @{ type = 'line';     x1 = 11; y1 = 11; x2 = 11; y2 = 21; color = $Blue; stroke = 2.5 }
            @{ type = 'line';     x1 = 16; y1 = 11; x2 = 16; y2 = 21; color = $Blue; stroke = 2.5 }
            @{ type = 'line';     x1 = 8;  y1 = 14; x2 = 19; y2 = 14; color = $Blue; stroke = 2.5 }
            @{ type = 'line';     x1 = 8;  y1 = 18; x2 = 19; y2 = 18; color = $Blue; stroke = 2.5 }
            @{ type = 'fillRect'; x = 20; y = 20; w = 8;  h = 8;  color = $Amber }
            @{ type = 'line';     x1 = 22; y1 = 22; x2 = 26; y2 = 26; color = $White; stroke = 2 }
            @{ type = 'line';     x1 = 26; y1 = 22; x2 = 22; y2 = 26; color = $White; stroke = 2 }
        )
    },
    @{
        # Open a view by its name
        Name = 'OpenViewByName'
        Shapes = @(
            @{ type = 'fillRect'; x = 4;  y = 5;  w = 24; h = 18; color = $White }
            @{ type = 'rect';     x = 4;  y = 5;  w = 24; h = 18; color = $Ink; stroke = 2 }
            @{ type = 'fillRect'; x = 4;  y = 5;  w = 24; h = 5;  color = $Blue }
            @{ type = 'fillEllipse'; x = 7; y = 6; w = 3; h = 3; color = $White }
            @{ type = 'fillEllipse'; x = 12; y = 6; w = 3; h = 3; color = $White }
            @{ type = 'line';     x1 = 8;  y1 = 16; x2 = 16; y2 = 16; color = $Teal; stroke = 2 }
            @{ type = 'line';     x1 = 12; y1 = 12; x2 = 12; y2 = 20; color = $Teal; stroke = 2 }
            @{ type = 'polygon'; fill = $true; color = $Amber; points = @(
                @(18, 12), @(27, 16), @(18, 20)
            ) }
        )
    },
    @{
        # Select elements by ElementId list
        Name = 'SelectElementsById'
        Shapes = @(
            @{ type = 'fillRect'; x = 6;  y = 5;  w = 16; h = 16; color = $White }
            @{ type = 'rect';     x = 6;  y = 5;  w = 16; h = 16; color = $Blue; stroke = 2 }
            @{ type = 'line';     x1 = 6;  y1 = 5;  x2 = 11; y2 = 5;  color = $Amber; stroke = 3 }
            @{ type = 'line';     x1 = 6;  y1 = 5;  x2 = 6;  y2 = 10; color = $Amber; stroke = 3 }
            @{ type = 'line';     x1 = 17; y1 = 5;  x2 = 22; y2 = 5;  color = $Amber; stroke = 3 }
            @{ type = 'line';     x1 = 22; y1 = 5;  x2 = 22; y2 = 10; color = $Amber; stroke = 3 }
            @{ type = 'line';     x1 = 6;  y1 = 16; x2 = 6;  y2 = 21; color = $Amber; stroke = 3 }
            @{ type = 'line';     x1 = 6;  y1 = 21; x2 = 11; y2 = 21; color = $Amber; stroke = 3 }
            @{ type = 'polygon'; fill = $true; color = $Ink; points = @(
                @(16, 16), @(16, 28), @(19, 25), @(21, 29), @(23, 28), @(21, 24), @(25, 24)
            ) }
            @{ type = 'fillRect'; x = 10; y = 9;  w = 8; h = 3; color = $Blue }
            @{ type = 'fillRect'; x = 10; y = 14; w = 5; h = 3; color = $Blue }
        )
    },
    @{
        # Get active document information
        Name = 'GetDocumentInfo'
        Shapes = @(
            @{ type = 'fillRect'; x = 7;  y = 4;  w = 16; h = 22; color = $White }
            @{ type = 'rect';     x = 7;  y = 4;  w = 16; h = 22; color = $Ink; stroke = 2 }
            @{ type = 'polygon'; fill = $true; color = $Blue; points = @(
                @(17, 4), @(23, 10), @(17, 10)
            ) }
            @{ type = 'line';     x1 = 17; y1 = 4;  x2 = 17; y2 = 10; color = $Ink; stroke = 2 }
            @{ type = 'line';     x1 = 17; y1 = 10; x2 = 23; y2 = 10; color = $Ink; stroke = 2 }
            @{ type = 'line';     x1 = 10; y1 = 14; x2 = 18; y2 = 14; color = $Blue; stroke = 2 }
            @{ type = 'line';     x1 = 10; y1 = 18; x2 = 18; y2 = 18; color = $Blue; stroke = 2 }
            @{ type = 'line';     x1 = 10; y1 = 22; x2 = 15; y2 = 22; color = $Blue; stroke = 2 }
            @{ type = 'fillEllipse'; x = 20; y = 18; w = 10; h = 10; color = $Amber }
            @{ type = 'fillRect'; x = 24; y = 20; w = 2; h = 2; color = $White }
            @{ type = 'fillRect'; x = 24; y = 23; w = 2; h = 4; color = $White }
        )
    }
)

# --- run ---

if (-not (Test-Path -LiteralPath $outputFolder)) {
    New-Item -ItemType Directory -Path $outputFolder -Force | Out-Null
    Write-Host "Created output folder: $outputFolder"
}

$allSaved = @()
foreach ($cmd in $commands) {
    Write-Host ("Drawing icons for {0}..." -f $cmd.Name)
    $paths = Save-CommandIcons -OutputFolder $outputFolder -CommandName $cmd.Name -Shapes $cmd.Shapes
    $allSaved += $paths
}

Write-Host ''
Write-Host 'Saved icon files:'
foreach ($p in $allSaved) {
    Write-Host $p
}
Write-Host ''
Write-Host ("Done. {0} files written." -f $allSaved.Count)
