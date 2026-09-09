param([string]$GamePath, [string]$ModulePath = (Join-Path $PSScriptRoot '../Modules/Dragonslayer'))
. "$PSScriptRoot/common.ps1"
$module = Read-Xml (Join-Path $ModulePath 'SubModule.xml')
if ($module.Module.Id.value -ne 'Dragonslayer' -or $module.Module.SingleplayerModule.value -ne 'true') { throw 'Invalid module identity or mode.' }
$target = (Get-Content "$script:RepoRoot/target-game.json" -Raw | ConvertFrom-Json).version
foreach ($dep in $module.Module.DependedModules.DependedModule) {
    if ($dep.Id -cnotin @('Native','SandBoxCore','Sandbox') -or $dep.DependentVersion -ne $target) { throw 'Unexpected dependency/version.' }
}
$documents = @{}
foreach ($node in $module.Module.Xmls.XmlNode) {
    if ($node.XmlName.path -notmatch '^dragonslayer_[a-z]+$') { throw 'Unsafe/unexpected XML registration.' }
    $doc = Read-Xml (Join-Path $ModulePath "ModuleData/$($node.XmlName.path).xml")
    $expectedRoot = if ($node.XmlName.id -eq 'GameText') { 'strings' } else { $node.XmlName.id }
    if ($doc.DocumentElement.Name -ne $expectedRoot) { throw 'XML registration root mismatch.' }
    $documents[$node.XmlName.id] = $doc
    if ($node.XmlName.id -ne 'GameText' -and 'Multiplayer' -in $node.IncludedGameTypes.GameType.value) { throw 'Multiplayer is outside scope.' }
}
$items = @($documents.Items.Items.CraftedItem)
if ($items.Count -ne 1 -or $items[0].id -ne 'dragonslayer') { throw 'Expected exactly one distinct Dragonslayer item.' }
$pieces = @($documents.CraftingPieces.CraftingPieces.CraftingPiece)
$pieceIds = @($pieces.id)
if ($pieceIds.Count -ne 4 -or ($pieceIds | Select-Object -Unique).Count -ne 4) { throw 'Invalid/duplicate pieces.' }
foreach ($id in $pieceIds) { if ($id -notmatch '^dragonslayer_') { throw 'Native override detected.' } }
$template = $documents.CraftingTemplates.CraftingTemplates.CraftingTemplate
if ($template.id -ne $items[0].crafting_template -or $template.id -ne 'dragonslayer_two_handed') { throw 'Template reference mismatch.' }
$description = $documents.WeaponDescriptions.WeaponDescriptions.WeaponDescription
if (@($template.WeaponDescriptions.WeaponDescription).Count -ne 1 -or $template.WeaponDescriptions.WeaponDescription.id -ne 'dragonslayer_sword' -or $description.id -ne 'dragonslayer_sword' -or $description.weapon_class -ne 'TwoHandedSword') { throw 'Expected exclusively two-handed sword usage.' }
if ($description.item_usage_features -ne 'twohanded:block:swing:thrust' -or 'NotUsableWithOneHand' -notin $description.WeaponFlags.WeaponFlag.value) { throw 'Incorrect two-handed usage/flags.' }
foreach ($id in $pieceIds) {
    if ($id -notin $description.AvailablePieces.AvailablePiece.id) { throw 'Weapon description must allow every selected piece, or native crafting produces no weapon component.' }
}
foreach ($piece in $items[0].Pieces.Piece) {
    if ($piece.id -notin $pieceIds -or $piece.Type -ne ($pieces | Where-Object id -eq $piece.id).piece_type) { throw 'Missing/mistyped item piece.' }
    if ([int]$piece.scale_factor -lt 50 -or [int]$piece.scale_factor -gt 200) { throw 'Piece scale outside supported 50..200 percent.' }
}
foreach ($piece in $pieces) {
    if ([double]$piece.weight -le 0 -or [double]$piece.weight -gt 10) { throw 'Piece weight outside supported (0,10] kg.' }
    if ($piece.id -notin $template.UsablePieces.UsablePiece.piece_id) { throw 'Piece unavailable in template.' }
}
# Native GetWeaponPieces selects a non-hidden piece for every represented type.
# Its final Enumerable.First throws during new-campaign town orders otherwise.
foreach ($type in $template.PieceDatas.PieceData.piece_type) {
    $eligible = @($pieces | Where-Object { $_.piece_type -eq $type -and $_.id -in $template.UsablePieces.UsablePiece.piece_id -and $_.GetAttribute('is_hidden') -ne 'true' })
    if ($eligible.Count -eq 0) { throw "Town-order generation requires a visible piece for $type." }
}
$stats = (Read-Xml (Join-Path $ModulePath 'ModuleData/weapon_stats.xml')).DocumentElement
if ($stats.Name -ne 'DragonslayerStats') { throw 'Invalid stats root.' }
foreach ($name in 'swingDamage','thrustDamage','swingSpeed','thrustSpeed','handling') {
    $max = if ($name -like '*Damage') { 500 } else { 200 }
    $value = 0
    if (![int]::TryParse($stats.GetAttribute($name), [ref]$value) -or $value -lt 1 -or $value -gt $max) { throw "Invalid $name (1..$max)." }
}
if ($GamePath) {
    $game = Find-Game $GamePath
    foreach ($dep in $module.Module.DependedModules.DependedModule) {
        $installed = Read-Xml (Join-Path $game "Modules/$($dep.Id)/SubModule.xml")
        if ($installed.Module.Id.value -cne $dep.Id) { throw "Dependency ID casing mismatch: $($dep.Id)" }
    }
    foreach ($id in $documents.Keys) {
        $doc = $documents[$id]
        [void]$doc.Schemas.Add($null, (Join-Path $game "XmlSchemas/$id.xsd"))
        $doc.Validate($null)
    }
    $native = Read-Xml (Join-Path $game 'Modules/Native/ModuleData/crafting_pieces.xml')
    foreach ($piece in $pieces) {
        if (!$piece.mesh.StartsWith('dragonslayer_') -and $piece.mesh -notin $native.CraftingPieces.CraftingPiece.mesh) { throw "Unverified native mesh $($piece.mesh)" }
    }
    $desc = Read-Xml (Join-Path $game 'Modules/Native/ModuleData/weapon_descriptions.xml')
    if ('TwoHandedSword' -notin $desc.WeaponDescriptions.WeaponDescription.id) { throw 'Native usage missing.' }
    foreach ($folder in 'Native','SandBoxCore','SandBox') {
        foreach ($f in Get-ChildItem (Join-Path $game "Modules/$folder/ModuleData") -Filter '*.xml' -Recurse) {
            if (Select-String -LiteralPath $f.FullName -Pattern 'id="dragonslayer"' -Quiet) { throw "Item collision in $($f.Name)" }
        }
    }
    Write-Host "PASS: installed $target schemas, native references and distinct item ID."
}
Write-Host 'PASS: XML parsing, registration, dependencies, piece/template links, single-player scope and stats ranges.'
