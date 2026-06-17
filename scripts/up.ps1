# Bring up the full HomeFlow stack (db + api + ui) and open the UI in the browser.
# Run from anywhere: ./scripts/up.ps1
$repoRoot = Split-Path -Parent $PSScriptRoot

docker compose -f (Join-Path $repoRoot "docker-compose.yml") up -d --build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$url = "http://localhost:3000"
Write-Host "Waiting for UI at $url ..."
do {
    Start-Sleep -Milliseconds 500
    try { $ok = (Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200 }
    catch { $ok = $false }
} until ($ok)

Start-Process $url
Write-Host "UI is up: $url"
