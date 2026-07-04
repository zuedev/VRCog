# read version from source/version.json
$version = Get-Content -Raw -Path "source/version.json" | ConvertFrom-Json | Select-Object -ExpandProperty version

# zip source/* into vrcog-$version.zip with a top-level VRCog/ folder
$zipFileName = "vrcog-$version.zip"
$tempDir = Join-Path $env:TEMP "VRCog-release"
Remove-Item -Recurse -Force $tempDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path (Join-Path $tempDir "VRCog") | Out-Null
Copy-Item -Path "source/*" -Destination (Join-Path $tempDir "VRCog") -Recurse
Compress-Archive -Path (Join-Path $tempDir "VRCog") -DestinationPath $zipFileName
Remove-Item -Recurse -Force $tempDir