param([string]$GamePath)
. "$PSScriptRoot/common.ps1"
$game = Find-Game $GamePath
& "$PSScriptRoot/validate-guts-powers.ps1"
$project = "$script:RepoRoot/src/DragonslayerGutsPowers/DragonslayerGutsPowers.csproj"
$out = Join-Path $script:RepoRoot ('artifacts/guts-powers-' + [Guid]::NewGuid().ToString('N'))
$module = Join-Path $out 'Modules/DragonslayerGutsPowers'
New-Item -ItemType Directory $module -Force | Out-Null
Copy-Item "$script:RepoRoot/Modules/DragonslayerGutsPowers/SubModule.xml" $module
Copy-Item "$script:RepoRoot/Modules/DragonslayerGutsPowers/ModuleData" $module -Recurse
foreach ($configuration in 'Win64_Shipping_Client','Win64_Shipping_wEditor') {
    if (!(Test-Path "$game/bin/$configuration/Version.xml")) { continue }
    if ((Read-Xml "$game/bin/$configuration/Version.xml").Version.Singleplayer.Value -ne 'v1.4.8') { throw 'Mismatched game/editor.' }
    & dotnet build $project -c Release "-p:GamePath=$game" "-p:GameConfiguration=$configuration" "-p:OutputPath=bin/$configuration/"
    if ($LASTEXITCODE -ne 0) { throw 'Guts Powers compilation failed.' }
    New-Item -ItemType Directory "$module/bin/$configuration" -Force | Out-Null
    Copy-Item "$script:RepoRoot/src/DragonslayerGutsPowers/bin/$configuration/DragonslayerGutsPowers.dll" "$module/bin/$configuration"
}
if (!(Test-Path "$module/bin/Win64_Shipping_Client/DragonslayerGutsPowers.dll")) { throw 'Missing client build.' }
$files = @(Get-ChildItem $module -File -Recurse | ForEach-Object {
    @{path=[IO.Path]::GetRelativePath($module,$_.FullName).Replace('\','/');sha256=(Get-FileHash $_.FullName).Hash}
})
@{module='DragonslayerGutsPowers';target='v1.4.8';files=$files} | ConvertTo-Json -Depth 5 | Set-Content "$module/.dragonslayer-install.json"
Copy-Item "$script:RepoRoot/docs/GUTS_POWERS.md" "$out/README.md"
$zip = "$script:RepoRoot/artifacts/DragonslayerGutsPowers-0.1.0-v1.4.8.zip"
Compress-Archive -Path "$out/Modules","$out/README.md" -DestinationPath $zip -Force
# Validate the archive itself, including the ownership marker required for removal.
$check = Join-Path $out 'archive-check'
Expand-Archive -LiteralPath $zip -DestinationPath $check
$unpacked = "$check/Modules/DragonslayerGutsPowers"
$manifest = Get-Content "$unpacked/.dragonslayer-install.json" -Raw | ConvertFrom-Json
foreach ($file in $manifest.files) {
    if ((Get-FileHash -LiteralPath (Get-OwnedPath $unpacked $file.path)).Hash -ne $file.sha256) { throw 'Archive checksum failure.' }
}
if (@(Get-ChildItem $unpacked -Recurse -File -Force).Count -ne $files.Count + 1) { throw 'Unexpected package files.' }
Set-Content "$script:RepoRoot/artifacts/latest-guts-powers.txt" $module
Write-Host "PACKAGE: $zip (archive verified)"

