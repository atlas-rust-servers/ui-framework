param(
    [switch]$Linux
)

$ErrorActionPreference = 'Stop'
$resourcesDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'src/references/Rust'))
$temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$tmpDir = Join-Path $temporaryRoot "Download-References-$([Guid]::NewGuid().ToString('N'))"
$depotDir = Join-Path $tmpDir 'DepotDownloader'
$rustDir = Join-Path $tmpDir 'RustDLLs'
$oxideDir = Join-Path $tmpDir 'Oxide'
$depot = '258551'
$oxideAsset = 'Oxide.Rust.zip'
$steamworksAssembly = 'Facepunch.Steamworks.Win64.dll'
if ($Linux)
{
    $depot = '258552'
    $oxideAsset = 'Oxide.Rust-linux.zip'
    $steamworksAssembly = 'Facepunch.Steamworks.Posix.dll'
}

New-Item -ItemType Directory -Path $depotDir, $rustDir, $oxideDir -Force | Out-Null
try
{
    Invoke-WebRequest -Uri 'https://github.com/SteamRE/DepotDownloader/releases/latest/download/DepotDownloader-windows-x64.zip' -OutFile "$depotDir/DepotDownloader.zip"
    Expand-Archive -LiteralPath "$depotDir/DepotDownloader.zip" -DestinationPath $depotDir
    $fileListPath = Join-Path $rustDir 'filelist.txt'
    'regex:RustDedicated_Data/Managed/.*\.dll' | Set-Content -LiteralPath $fileListPath
    & "$depotDir/DepotDownloader.exe" -app 258550 -depot $depot -filelist $fileListPath -dir $rustDir
    if ($LASTEXITCODE -ne 0)
    {
        throw "DepotDownloader failed with exit code $LASTEXITCODE."
    }

    $managedDir = Join-Path $rustDir 'RustDedicated_Data/Managed'
    Invoke-WebRequest -Uri "https://github.com/OxideMod/Oxide.Rust/releases/latest/download/$oxideAsset" -OutFile "$oxideDir/Oxide.Rust.zip"
    Expand-Archive -LiteralPath "$oxideDir/Oxide.Rust.zip" -DestinationPath $oxideDir
    Copy-Item -Path "$oxideDir/RustDedicated_Data/Managed/*.dll" -Destination $managedDir -Force
    if (-not (Test-Path -LiteralPath (Join-Path $managedDir $steamworksAssembly) -PathType Leaf))
    {
        throw "Downloaded references do not contain $steamworksAssembly."
    }

    if (-not $resourcesDir.StartsWith([IO.Path]::GetFullPath($PSScriptRoot) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))
    {
        throw "References directory is outside the checkout: $resourcesDir"
    }
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
    Get-ChildItem -LiteralPath $resourcesDir -File -Filter '*.dll' | Remove-Item -Force
    Copy-Item -Path "$managedDir/*.dll" -Destination $resourcesDir -Force
}
finally
{
    if (-not [IO.Path]::GetFullPath($tmpDir).StartsWith($temporaryRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))
    {
        throw "Temporary directory is outside the temp root: $tmpDir"
    }
    Remove-Item -LiteralPath $tmpDir -Force -Recurse
}

Write-Output "References from depot $depot and $oxideAsset copied to $resourcesDir."
