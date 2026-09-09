param([string]$GamePath)
. "$PSScriptRoot/common.ps1"
$game = Find-Game $GamePath
$destination = [IO.Path]::GetFullPath((Join-Path $game 'Modules/Dragonslayer'))
Assert-NoLink $destination
if (!(Test-Path $destination)) { Write-Host 'Dragonslayer is not installed.'; return }
$marker = Join-Path $destination '.dragonslayer-install.json'
$manifest = Get-Content -LiteralPath $marker -Raw | ConvertFrom-Json
if ($manifest.module -ne 'Dragonslayer') { throw 'Not a managed Dragonslayer installation.' }
# Preflight all files: never erase user edits or follow links.
$paths = @()
foreach ($file in $manifest.files) {
    $path = Get-OwnedPath $destination $file.path
    if (Test-Path -LiteralPath $path) {
        if ((Get-FileHash -LiteralPath $path).Hash -ne $file.sha256) { throw "Edited file preserved: $path. Back it up and restore the original before uninstalling, or remove this mod folder manually." }
        $paths += $path
    }
}
foreach ($path in $paths) { Remove-Item -LiteralPath $path }
# Unix PowerShell treats the dot-prefixed ownership marker as hidden.
Remove-Item -LiteralPath $marker -Force
# Remove empty directories only. Unlisted files are preserved.
Get-ChildItem -LiteralPath $destination -Directory -Recurse | Sort-Object { $_.FullName.Length } -Descending | ForEach-Object {
    Assert-NoLink $_.FullName
    if (@(Get-ChildItem -LiteralPath $_.FullName -Force).Count -eq 0) { Remove-Item -LiteralPath $_.FullName }
}
if (@(Get-ChildItem -LiteralPath $destination -Force).Count -eq 0) { Remove-Item -LiteralPath $destination }
Write-Host 'Removed managed Dragonslayer files. Unrelated files and all saves were preserved.'
