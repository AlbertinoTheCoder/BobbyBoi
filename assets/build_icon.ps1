# Build the icon from BobbyBoi's actual Blender head, in seven Windows sizes.
$taskRoot = Split-Path $PSScriptRoot -Parent
Push-Location $taskRoot
try {
    $taskIconBuilder = Join-Path $taskRoot 'BobbyBoi.exe'
    Start-Process -FilePath $taskIconBuilder -ArgumentList '--build-icon' -WorkingDirectory $taskRoot -Wait -WindowStyle Hidden
} finally { Pop-Location }
