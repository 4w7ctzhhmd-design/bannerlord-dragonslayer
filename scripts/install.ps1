param([string]$GamePath, [string]$ModulePath, [ValidateSet('Dragonslayer','DragonslayerInstantKill')][string]$ModuleId = 'Dragonslayer')
. "$PSScriptRoot/common.ps1"
$game = Find-Game $GamePath
if (!$ModulePath) {
    $latest = if ($ModuleId -eq 'Dragonslayer') { 'latest-build.txt' } else { 'latest-instant-kill.txt' }
    $ModulePath = (Get-Content "$script:RepoRoot/artifacts/$latest" -Raw).Trim()
}
$source = (Resolve-Path -LiteralPath $ModulePath).Path
$manifest = Get-Content (Join-Path $source '.dragonslayer-install.json') -Raw | ConvertFrom-Json
if ($manifest.module -ne $ModuleId -or $manifest.target -ne (Get-Content "$script:RepoRoot/target-game.json" -Raw | ConvertFrom-Json).version) { throw 'Wrong package identity/version.' }
$destination = [IO.Path]::GetFullPath((Join-Path $game "Modules/$ModuleId"))
Assert-NoLink $destination
if (Test-Path $destination) { throw 'Dragonslayer folder already exists. Uninstall the previous managed build first, preserving edited/unrelated files.' }
# Check every path and hash before creating the destination.
foreach ($file in $manifest.files) {
    $from = Get-OwnedPath $source $file.path
    [void](Get-OwnedPath $destination $file.path)
    if ((Get-FileHash -LiteralPath $from).Hash -ne $file.sha256) { throw "Package checksum mismatch: $($file.path)" }
}
foreach ($file in $manifest.files) {
    $to = Get-OwnedPath $destination $file.path
    New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Get-OwnedPath $source $file.path) -Destination $to
}
Copy-Item -LiteralPath (Join-Path $source '.dragonslayer-install.json') -Destination $destination
Write-Host "Installed only $destination. Enable $ModuleId after its dependencies in the launcher."
