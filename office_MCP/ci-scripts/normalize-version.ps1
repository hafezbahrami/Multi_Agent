param(
    [Parameter(Mandatory=$true)][string]$Version
)

# Convert a SemVer-style version like "1.2.3-rc.4+sha" into a 4-part
# assembly version "1.2.3.4". Strips pre-release/metadata segments.
$clean = $Version.Split('-')[0].Split('+')[0]
$parts = $clean.Split('.')
while ($parts.Length -lt 4) { $parts += '0' }
($parts[0..3] -join '.')
