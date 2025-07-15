# install.ps1
# Calls setup.ps1 located in the same directory

$ErrorActionPreference = "Stop"

$setupRclone = Join-Path $PSScriptRoot "steps" "setup_rclone.ps1"
Write-Host "Step 1: Setup rclone"
& $setupRclone
