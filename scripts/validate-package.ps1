param([string]$PackagePath)
. "$PSScriptRoot/common.ps1"
if (!$PackagePath) { $PackagePath = Join-Path $script:RepoRoot 'artifacts/Dragonslayer-0.1.1-Custom-v1.4.8.zip' }
$temp = Join-Path $script:RepoRoot ('artifacts/package-check-' + [Guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath $PackagePath -DestinationPath $temp
$module = Join-Path $temp 'Modules/Dragonslayer'
$manifest = Get-Content "$module/.dragonslayer-install.json" -Raw | ConvertFrom-Json
if ($manifest.module -cne 'Dragonslayer') { throw 'Wrong module identity.' }
foreach ($file in $manifest.files) {
    $path = Get-OwnedPath $module $file.path
    if ((Get-FileHash -LiteralPath $path).Hash -ne $file.sha256) { throw "Package checksum mismatch: $($file.path)" }
}
$allFiles = @(Get-ChildItem $module -Recurse -File -Force)
if ($allFiles.Count -ne $manifest.files.Count + 1) { throw 'Unmanifested module files.' }
foreach ($file in Get-ChildItem $temp -Recurse -File -Force) {
    if (($file.Extension -eq '.dll' -and $file.Name -cne 'Dragonslayer.dll') -or $file.Extension -in '.sav','.fbx','.blend' -or $file.Name -like '*.local.*') { throw "Unexpected distributable file: $($file.Name)" }
}
if ($manifest.visual -eq 'Custom') {
    $pieces = Read-Xml "$module/ModuleData/dragonslayer_pieces.xml"
    foreach ($piece in $pieces.CraftingPieces.CraftingPiece) {
        if ($piece.mesh -cne $piece.id) { throw 'Custom package references a temporary native mesh.' }
    }
    if (!(Test-Path "$module/AssetPackages/*.tpac")) { throw 'Custom package missing published assets.' }
}
if (!(Test-Path "$temp/README.md") -or !(Test-Path "$temp/docs/VERIFICATION.md")) { throw 'Missing testing documentation.' }
& "$PSScriptRoot/validate.ps1" -ModulePath $module
Write-Host "PASS: extracted package file set, checksums, custom references and documentation. $PackagePath"
