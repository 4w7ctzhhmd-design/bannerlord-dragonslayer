. "$PSScriptRoot/common.ps1"
$root = Join-Path $script:RepoRoot 'assets/published'
$record = Get-Content "$script:RepoRoot/assets/publish-record.json" -Raw | ConvertFrom-Json
if ($record.module -cne 'Dragonslayer' -or $record.target -cne 'Client') { throw 'Unexpected published asset identity.' }
foreach ($file in $record.files) {
    $path = Get-OwnedPath $root $file.path
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.sha256) { throw "Published asset changed: $($file.path). Republish and update the provenance record." }
}
$actual = @(Get-ChildItem $root -Recurse -File)
if ($actual.Count -ne $record.files.Count) { throw 'Unrecorded published files.' }
if (@($actual | Where-Object Extension -eq '.tpac').Count -eq 0) { throw 'Missing client package.' }
Write-Host 'PASS: recorded client asset checksums and file set (not a runtime rendering test).'
