<#
.SYNOPSIS
    Builds the Docker image for the frontend.

.PARAMETER Repository
    The repository for the Docker image. Can be "docker.io" or "ghcr.io". Default: "docker.io"

.PARAMETER Tag
    The tag for the Docker image. Default: "null" (reads the tag from the package.json).

.PARAMETER Push
    Switch, if the image should be pushed or not.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -repository ghcr.io -tag 0.1.0 -push
#>
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("docker.io", "ghcr.io")]
    [string]$repository = "docker.io",

    [Parameter(Mandatory = $false)]
    [string]$tag = $null,

    [Parameter(Mandatory = $false)]
    [switch]$push = $false
)

$IMAGE = "$repository/thomasmiller01/kapitelshelf-frontend"

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

if ($push) {
    Write-Host "Pushing frontend image: '${IMAGE}:$Tag'"
    try {
        docker push "${IMAGE}:$Tag"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Image '${IMAGE}:$Tag' pushed successfully."
        }
        else {
            Write-Error "❌ Image '${IMAGE}:$tag' push failed with code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
    }
    catch {
        Write-Error "❌ An error occurred during '${IMAGE}:$tag' push: $_"
        exit 1
    }
}