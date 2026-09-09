. "$PSScriptRoot/common.ps1"
$module = (Read-Xml "$script:RepoRoot/Modules/DragonslayerInstantKill/SubModule.xml").Module
if ($module.Id.value -cne 'DragonslayerInstantKill' -or $module.SingleplayerModule.value -ne 'true') { throw 'Invalid add-on identity/scope.' }
$expected = @{Native='v1.4.8'; SandBoxCore='v1.4.8'; Sandbox='v1.4.8'; Dragonslayer='v0.1.1'}
$deps = @($module.DependedModules.DependedModule)
if ($deps.Count -ne 4 -or @($deps.Id | Select-Object -Unique).Count -ne 4) { throw 'Invalid add-on dependencies.' }
foreach ($dep in $deps) {
    if ($dep.Id -cnotin $expected.Keys -or $dep.DependentVersion -ne $expected[$dep.Id] -or $dep.Optional -ne 'false') { throw 'Incorrect dependency.' }
}
if ($module.SubModules.SubModule.DLLName.value -cne 'DragonslayerInstantKill.dll' -or $module.SubModules.SubModule.SubModuleClassType.value -cne 'DragonslayerInstantKill.SubModule') { throw 'Invalid add-on entry point.' }
Write-Host 'PASS: optional add-on XML, entry point and exact dependencies.'
