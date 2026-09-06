Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$script:RepoRoot = Split-Path $PSScriptRoot -Parent

function Read-Xml([string]$Path) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($Path, $settings)
    try { $doc = [xml]::new(); $doc.XmlResolver = $null; $doc.Load($reader); return ,$doc }
    finally { $reader.Dispose() }
}

function Find-Game([string]$GamePath) {
    if (!$GamePath) { $GamePath = $env:BANNERLORD_GAME_PATH }
    $local = Join-Path $script:RepoRoot 'local.settings.json'
    if (!$GamePath -and (Test-Path $local)) { $GamePath = (Get-Content $local -Raw | ConvertFrom-Json).gamePath }
    if (!$GamePath -and $IsWindows) {
        $steam = @("${env:ProgramFiles(x86)}/Steam", "$env:ProgramFiles/Steam")
        $registry = Get-ItemProperty 'HKCU:/Software/Valve/Steam' -ErrorAction SilentlyContinue
        if ($registry -and $registry.PSObject.Properties['SteamPath']) { $steam += $registry.SteamPath }
        $libraries = @($steam)
        foreach ($root in $steam) {
            $vdf = Join-Path $root 'steamapps/libraryfolders.vdf'
            if (Test-Path $vdf) {
                foreach ($match in [regex]::Matches((Get-Content $vdf -Raw), '"path"\s+"([^"]+)"')) {
                    $libraries += $match.Groups[1].Value.Replace('\\', '\')
                }
            }
        }
        foreach ($root in ($libraries | Select-Object -Unique)) {
            $candidate = Join-Path $root 'steamapps/common/Mount & Blade II Bannerlord'
            if (Test-Path (Join-Path $candidate 'bin/Win64_Shipping_Client/Version.xml')) { $GamePath = $candidate; break }
        }
    }
    if (!$GamePath) { throw 'Bannerlord not detected. Supply -GamePath, BANNERLORD_GAME_PATH, or local.settings.json (see README).' }
    $resolved = (Resolve-Path -LiteralPath $GamePath).Path
    $expected = (Get-Content (Join-Path $script:RepoRoot 'target-game.json') -Raw | ConvertFrom-Json).version
    $version = (Read-Xml (Join-Path $resolved 'bin/Win64_Shipping_Client/Version.xml')).Version.Singleplayer.Value
    if ($version -ne $expected) { throw "Installed $version differs from target $expected. Inspect APIs and retarget before building/installing." }
    foreach ($id in 'Native','SandBoxCore','SandBox') {
        $module = Read-Xml (Join-Path $resolved "Modules/$id/SubModule.xml")
        if ($module.Module.Version.value -ne $expected) { throw "$id is not $expected; finish/verify the game update." }
    }
    return $resolved
}

function Assert-NoLink([string]$Path) {
    $current = [IO.Path]::GetFullPath($Path)
    while ($current) {
        if (Test-Path -LiteralPath $current) {
            if ((Get-Item -LiteralPath $current -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Refusing reparse point: $current" }
        }
        $current = [IO.Path]::GetDirectoryName($current)
    }
}

function Get-OwnedPath([string]$Root, [string]$Relative) {
    $rootFull = [IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $full = [IO.Path]::GetFullPath((Join-Path $Root $Relative))
    if ([IO.Path]::IsPathRooted($Relative) -or !$full.StartsWith($rootFull, [StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe manifest path: $Relative" }
    Assert-NoLink $full
    return $full
}
