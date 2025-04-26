<#
.SYNOPSIS
    Builds the Docker image for the migrator.

.PARAMETER Tag
    The tag for the Docker image. Default: "null" (reads the tag from the KapitelShelf.Data.Migrations.csproj).

.EXAMPLE
    .\build.ps1
    .\build.ps1 -tag 0.1.0 -push
#>
param(
    [Parameter(Mandatory = $false)]
    [string]$tag = $null,

    [Parameter(Mandatory = $false)]
    [switch]$push = $false
)

$IMAGE = "thomasmiller01/kapitelshelf-migrator"

# if tag not provided, read version from KapitelShelf.Data.Migrations.csproj
if (-not $Tag) {
    Write-Host "No tag specified:"
    try {
        # Load XML and extract Version element
        [xml]$xml = Get-Content -Path "./KapitelShelf.Data.Migrations.csproj"
        $versionNode = $xml.Project.PropertyGroup.Version
        $tag = $versionNode.Trim()

        Write-Host "> Using version '$tag' from KapitelShelf.Data.Migrations.csproj."
    } catch {
        Write-Error "❌ Failed to read version from KapitelShelf.Data.Migrations.csproj: $_"
        exit 1
    }
}

Write-Host "Building migrator image: '${IMAGE}:$tag'"
try {
    docker build --progress=plain -t "${IMAGE}:$tag" ../ -f ./Dockerfile
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
    Write-Host "Pushing migrator image: '${IMAGE}:$Tag'"
    try {
        docker push "${IMAGE}:$Tag"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Image '${IMAGE}:$Tag' pushed successfully."
        }
        else {
            Write-Error "❌ Image '${IMAGE}:$tag' push failed with code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
    } catch {
        Write-Error "❌ An error occurred during '${IMAGE}:$tag' push: $_"
        exit 1
    }
}