<#
.SYNOPSIS
    Builds the Docker image for the frontend.

.PARAMETER Tag
    The tag for the Docker image. Default: "null" (reads the tag from the package.json).

.EXAMPLE
    .\build.ps1
    .\build.ps1 -tag 0.1.0
    .\build.ps1 -tag latest
#>
param(
    [Parameter(Mandatory = $false)]
    [string]$tag = $null
)

$IMAGE = "thomasmiller01/kapitelshelf-frontend"

# if tag not provided, read version from package.json
if (-not $Tag) {
    Write-Host "No tag specified:"
    try {
        $pkgJson = Get-Content -Path "./package.json" -Raw | ConvertFrom-Json
        $tag = $pkgJson.version
        Write-Host "> Using version '$tag' from package.json."
    }
    catch {
        Write-Error "❌ Failed to read version from package.json: $_"
        exit 1
    }
}

Write-Host "Building frontend image: '${IMAGE}:$tag'"
try {
    docker build --progress=plain -t "${IMAGE}:$tag" .
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Image '${IMAGE}:$tag' built successfully."
    }
    else {
        Write-Error "❌ Image '${IMAGE}:$tag' build exited with code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}
catch {
    Write-Error "❌ An error occurred during '${IMAGE}:$tag' build: $_"
    exit 1
}