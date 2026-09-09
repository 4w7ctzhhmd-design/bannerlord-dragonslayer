. "$PSScriptRoot/common.ps1"
function Expect-Failure([scriptblock]$Action, [string]$Expected) {
    $caught = $false
    try { & $Action } catch { if ($_.Exception.Message -notlike "*$Expected*") { throw }; $caught = $true }
    if (!$caught) { throw "Expected failure containing: $Expected" }
}
$temp = Join-Path $script:RepoRoot ('artifacts/test-' + [Guid]::NewGuid().ToString('N'))
$fixture = Join-Path $temp 'game'
$version = (Get-Content "$script:RepoRoot/target-game.json" -Raw | ConvertFrom-Json).version
New-Item -ItemType Directory "$fixture/bin/Win64_Shipping_Client" -Force | Out-Null
Set-Content "$fixture/bin/Win64_Shipping_Client/Version.xml" "<Version><Singleplayer Value=`"$version`" /></Version>"
foreach ($id in 'Native','SandBoxCore','SandBox') {
    New-Item -ItemType Directory "$fixture/Modules/$id" -Force | Out-Null
    Set-Content "$fixture/Modules/$id/SubModule.xml" "<Module><Id value=`"$id`"/><Version value=`"$version`"/></Module>"
}
$payload = Join-Path $temp 'payload'
New-Item -ItemType Directory $payload | Out-Null
Set-Content "$payload/owned.txt" 'owned'
$manifest = @{module='Dragonslayer'; target=$version; files=@(@{path='owned.txt'; sha256=(Get-FileHash "$payload/owned.txt").Hash})}
$manifest | ConvertTo-Json -Depth 4 | Set-Content "$payload/.dragonslayer-install.json"
& "$PSScriptRoot/install.ps1" -GamePath $fixture -ModulePath $payload
Expect-Failure { & "$PSScriptRoot/install.ps1" -GamePath $fixture -ModulePath $payload } 'already exists'
$installed = "$fixture/Modules/Dragonslayer"
Set-Content "$installed/owned.txt" 'user edit'
Expect-Failure { & "$PSScriptRoot/uninstall.ps1" -GamePath $fixture } 'Edited file preserved'
Copy-Item "$payload/owned.txt" "$installed/owned.txt" -Force
Set-Content "$installed/unrelated.txt" 'must survive'
& "$PSScriptRoot/uninstall.ps1" -GamePath $fixture
if (!(Test-Path "$installed/unrelated.txt") -or (Test-Path "$installed/owned.txt")) { throw 'Uninstall ownership failed.' }
Expect-Failure { Get-OwnedPath $installed '../../outside.txt' } 'Unsafe manifest path'
$validation = Join-Path $temp 'validation'
Copy-Item "$script:RepoRoot/Modules/Dragonslayer" $validation -Recurse
$statsPath = "$validation/ModuleData/weapon_stats.xml"
$xml = Read-Xml $statsPath
$xml.DragonslayerStats.swingDamage = '501'
$xml.Save($statsPath)
Expect-Failure { & "$PSScriptRoot/validate.ps1" -ModulePath $validation } 'Invalid swingDamage'
$xml.DragonslayerStats.swingDamage = '180'
$xml.Save($statsPath)
$itemPath = "$validation/ModuleData/dragonslayer_items.xml"
$xml = Read-Xml $itemPath
$xml.Items.CraftedItem.Pieces.Piece[0].id = 'missing_blade'
$xml.Save($itemPath)
Expect-Failure { & "$PSScriptRoot/validate.ps1" -ModulePath $validation } 'Missing/mistyped'
Write-Host 'PASS: install, existing-folder refusal, edited-file protection, unrelated-file preservation, path traversal rejection, invalid stats and missing piece rejection.'
Copy-Item "$script:RepoRoot/Modules/Dragonslayer/ModuleData/dragonslayer_items.xml" $itemPath -Force
$descriptionPath = "$validation/ModuleData/dragonslayer_descriptions.xml"
$xml = Read-Xml $descriptionPath
$xml.WeaponDescriptions.WeaponDescription.AvailablePieces.AvailablePiece[0].id = 'not_our_blade'
$xml.Save($descriptionPath)
Expect-Failure { & "$PSScriptRoot/validate.ps1" -ModulePath $validation } 'must allow every selected piece'
Write-Host 'PASS: crafted weapon description eligibility regression.'
Copy-Item "$script:RepoRoot/Modules/Dragonslayer/ModuleData/dragonslayer_descriptions.xml" $descriptionPath -Force
$piecesPath = "$validation/ModuleData/dragonslayer_pieces.xml"
$xml = Read-Xml $piecesPath
$xml.CraftingPieces.CraftingPiece[0].SetAttribute('is_hidden', 'true')
$xml.Save($piecesPath)
Expect-Failure { & "$PSScriptRoot/validate.ps1" -ModulePath $validation } 'Town-order generation requires a visible piece'
Write-Host 'PASS: hidden crafting piece new-campaign crash regression.'
$manifest.module = 'DragonslayerInstantKill'
$manifest | ConvertTo-Json -Depth 4 | Set-Content "$payload/.dragonslayer-install.json"
Expect-Failure { & "$PSScriptRoot/install.ps1" -GamePath $fixture -ModulePath $payload } 'Wrong package identity'
& "$PSScriptRoot/install.ps1" -GamePath $fixture -ModulePath $payload -ModuleId DragonslayerInstantKill
& "$PSScriptRoot/uninstall.ps1" -GamePath $fixture -ModuleId DragonslayerInstantKill
if (!(Test-Path "$installed/unrelated.txt")) { throw 'Add-on uninstall modified the base mod.' }
Write-Host 'PASS: add-on identity isolation and base mod preservation.'
Write-Host "Test fixtures retained in $temp"
