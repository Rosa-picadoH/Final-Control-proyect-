$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root 'FinalControl\FinalControl.csproj'
$release = Join-Path $root 'Release\FinalControl'

New-Item -ItemType Directory -Force -Path $release | Out-Null
dotnet publish $project -c Release -r win-x64 --self-contained false -o $release
Copy-Item -LiteralPath (Join-Path $root 'database\FinalControlDb.sql') -Destination (Join-Path $root 'Release\FinalControlDb.sql') -Force
Write-Host "Entrega publicada en: $release"
