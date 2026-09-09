param([string]$GamePath)
. "$PSScriptRoot/common.ps1"
$game = Find-Game $GamePath
& "$PSScriptRoot/validate-instant-kill.ps1"
$project = "$script:RepoRoot/src/DragonslayerInstantKill/DragonslayerInstantKill.csproj"
$out = Join-Path $script:RepoRoot ('artifacts/instant-kill-' + [Guid]::NewGuid().ToString('N'))
$module = Join-Path $out 'Modules/DragonslayerInstantKill'
New-Item -ItemType Directory $module -Force | Out-Null
Copy-Item "$script:RepoRoot/Modules/DragonslayerInstantKill/SubModule.xml" $module
foreach ($configuration in 'Win64_Shipping_Client','Win64_Shipping_wEditor') {
    if (!(Test-Path "$game/bin/$configuration/Version.xml")) { continue }
    if ((Read-Xml "$game/bin/$configuration/Version.xml").Version.Singleplayer.Value -ne 'v1.4.8') { throw 'Mismatched game/editor.' }
    & dotnet build $project -c Release "-p:GamePath=$game" "-p:GameConfiguration=$configuration" "-p:OutputPath=bin/$configuration/"
    if ($LASTEXITCODE -ne 0) { throw 'Instant Kill compilation failed.' }
    New-Item -ItemType Directory "$module/bin/$configuration" -Force | Out-Null
    Copy-Item "$script:RepoRoot/src/DragonslayerInstantKill/bin/$configuration/DragonslayerInstantKill.dll" "$module/bin/$configuration"
}
if (!(Test-Path "$module/bin/Win64_Shipping_Client/DragonslayerInstantKill.dll")) { throw 'Missing client build.' }
$files = @(Get-ChildItem $module -File -Recurse | ForEach-Object {
    @{path=[IO.Path]::GetRelativePath($module,$_.FullName).Replace('\','/');sha256=(Get-FileHash $_.FullName).Hash}
})
@{module='DragonslayerInstantKill';target='v1.4.8';files=$files} | ConvertTo-Json -Depth 5 | Set-Content "$module/.dragonslayer-install.json"
Copy-Item "$script:RepoRoot/docs/INSTANT_KILL.md" "$out/README.md"
$zip = "$script:RepoRoot/artifacts/DragonslayerInstantKill-0.1.0-v1.4.8.zip"
Compress-Archive -Path "$out/Modules","$out/README.md" -DestinationPath $zip -Force
# Validate the archive itself, including the ownership marker required for removal.
$check = Join-Path $out 'archive-check'
Expand-Archive -LiteralPath $zip -DestinationPath $check
$unpacked = "$check/Modules/DragonslayerInstantKill"
$manifest = Get-Content "$unpacked/.dragonslayer-install.json" -Raw | ConvertFrom-Json
foreach ($file in $manifest.files) {
    if ((Get-FileHash -LiteralPath (Get-OwnedPath $unpacked $file.path)).Hash -ne $file.sha256) { throw 'Archive checksum failure.' }
}
if (@(Get-ChildItem $unpacked -Recurse -File -Force).Count -ne $files.Count + 1) { throw 'Unexpected package files.' }
Set-Content "$script:RepoRoot/artifacts/latest-instant-kill.txt" $module
Write-Host "PACKAGE: $zip (archive verified)"
