# setup.ps1
# Downloads the latest rclone.exe for Windows x64 and puts it in ./3rd-party

$ErrorActionPreference = "Stop"

$downloadUrl = "https://downloads.rclone.org/rclone-current-windows-amd64.zip"
$targetDir = Join-Path $PSScriptRoot ".." "3rd-party"

if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
}

$tmpZip = Join-Path $targetDir "rclone-download.zip"

Write-Host "Downloading $downloadUrl ..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $tmpZip

Write-Host "Extracting..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($tmpZip, $targetDir)

# Find rclone.exe in extracted folder and move to 3rd-party root
$extractedDir = Get-ChildItem -Path $targetDir -Directory | Where-Object { $_.Name -like "rclone*" } | Select-Object -First 1
if ($null -eq $extractedDir) {
    throw "Extraction failed: rclone directory not found"
}
$srcExe = Join-Path $extractedDir.FullName "rclone.exe"
$destExe = Join-Path $targetDir "rclone.exe"
Move-Item -Path $srcExe -Destination $destExe -Force

# Clean up extracted folder and zip
Remove-Item $extractedDir.FullName -Recurse -Force
Remove-Item $tmpZip -Force

Write-Host "rclone.exe has been downloaded to $destExe"
