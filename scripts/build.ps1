param([string]$GamePath, [ValidateSet('Prototype','Custom')][string]$Visual = 'Custom', [string]$PublishedModule)
. "$PSScriptRoot/common.ps1"
$game = Find-Game $GamePath
& "$PSScriptRoot/validate.ps1" -GamePath $game
& dotnet build "$script:RepoRoot/src/Dragonslayer/Dragonslayer.csproj" -c Release "-p:GamePath=$game"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
$output = Join-Path $script:RepoRoot ('artifacts/build-' + [Guid]::NewGuid().ToString('N'))
$module = Join-Path $output 'Modules/Dragonslayer'
New-Item -ItemType Directory -Path "$module/ModuleData","$module/bin/Win64_Shipping_Client" -Force | Out-Null
Copy-Item "$script:RepoRoot/Modules/Dragonslayer/SubModule.xml" $module
Copy-Item "$script:RepoRoot/Modules/Dragonslayer/ModuleData/*.xml" "$module/ModuleData"
Copy-Item "$script:RepoRoot/src/Dragonslayer/bin/Release/net472/Dragonslayer.dll" "$module/bin/Win64_Shipping_Client"
$editorVersionPath = Join-Path $game 'bin/Win64_Shipping_wEditor/Version.xml'
if (Test-Path $editorVersionPath) {
    if ((Read-Xml $editorVersionPath).Version.Singleplayer.Value -ne (Read-Xml (Join-Path $game 'bin/Win64_Shipping_Client/Version.xml')).Version.Singleplayer.Value) { throw 'Editor/game mismatch. Match versions before building editor DLL.' }
    & dotnet build "$script:RepoRoot/src/Dragonslayer/Dragonslayer.csproj" -c Release "-p:GamePath=$game" '-p:GameConfiguration=Win64_Shipping_wEditor' '-p:OutputPath=bin/Editor/'
    if ($LASTEXITCODE -ne 0) { throw 'Editor compilation failed.' }
    New-Item -ItemType Directory "$module/bin/Win64_Shipping_wEditor" -Force | Out-Null
    Copy-Item "$script:RepoRoot/src/Dragonslayer/bin/Editor/Dragonslayer.dll" "$module/bin/Win64_Shipping_wEditor"
}
if ($Visual -eq 'Custom') {
    if (!$PublishedModule) {
        & "$PSScriptRoot/validate-assets.ps1"
        $PublishedModule = Join-Path $script:RepoRoot 'assets/published'
    }
    $published = (Resolve-Path -LiteralPath $PublishedModule).Path
    if ((Read-Xml (Join-Path $published 'SubModule.xml')).Module.Id.value -cne 'Dragonslayer') { throw 'Published module must identify itself as Dragonslayer.' }
    $packages = @(Get-ChildItem (Join-Path $published 'AssetPackages') -Filter '*.tpac' -File -Recurse)
    if ($packages.Count -eq 0) { throw 'No published client TPAC assets; source FBX is not a game asset.' }
    Assert-NoLink $published
    foreach ($file in Get-ChildItem $published -Recurse -Force) { Assert-NoLink $file.FullName }
    Copy-Item (Join-Path $published 'AssetPackages') $module -Recurse
    # Preserve runtime data emitted by the matching editor alongside its client pack.
    if (Test-Path (Join-Path $published 'RuntimeDataCache')) {
        Copy-Item (Join-Path $published 'RuntimeDataCache') $module -Recurse
    }
    $pieces = Read-Xml "$module/ModuleData/dragonslayer_pieces.xml"
    foreach ($piece in $pieces.CraftingPieces.CraftingPiece) { $piece.mesh = $piece.id }
    $pieces.Save("$module/ModuleData/dragonslayer_pieces.xml")
    $items = Read-Xml "$module/ModuleData/dragonslayer_items.xml"
    $items.Items.CraftedItem.name = 'Dragonslayer'
    $items.Save("$module/ModuleData/dragonslayer_items.xml")
    $registration = Read-Xml "$module/SubModule.xml"
    $registration.Module.Name.value = 'Dragonslayer (Custom Visual; Test Build)'
    $registration.Save("$module/SubModule.xml")
}
& "$PSScriptRoot/validate.ps1" -GamePath $game -ModulePath $module
$files = @(Get-ChildItem $module -Recurse -File | ForEach-Object {
    @{ path = [IO.Path]::GetRelativePath($module, $_.FullName).Replace('\','/'); sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash }
})
@{ module = 'Dragonslayer'; target = (Get-Content "$script:RepoRoot/target-game.json" -Raw | ConvertFrom-Json).version; visual = $Visual; files = $files } | ConvertTo-Json -Depth 5 | Set-Content "$module/.dragonslayer-install.json"
$outDir = Join-Path $script:RepoRoot 'artifacts'
$zip = Join-Path $outDir "Dragonslayer-0.1.0-$Visual-v1.4.8.zip"
Copy-Item "$script:RepoRoot/README.md" $output
Copy-Item "$script:RepoRoot/docs" $output -Recurse
New-Item -ItemType Directory "$output/assets/source" -Force | Out-Null
Copy-Item "$script:RepoRoot/assets/source/preview.png" "$output/assets/source"
Compress-Archive -Path "$output/Modules","$output/README.md","$output/docs","$output/assets" -DestinationPath $zip -Force
Set-Content "$outDir/latest-build.txt" $module
Write-Host "BUILD: $module"
Write-Host "PACKAGE: $zip"
