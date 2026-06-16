# Start the HomeFlow stack and open the UI in the browser.
# Usage: .\start.ps1            (attached, Ctrl-C stops)
#        .\start.ps1 -Detach    (background; stack keeps running)

param(
    [switch]$Detach
)

$UI_URL = "http://localhost:3000"
$API_HEALTH = "http://localhost:5000/api/health"

Write-Host "Starting HomeFlow stack..." -ForegroundColor Cyan

if ($Detach) {
    docker compose up --build -d
} else {
    # Start detached first so we can wait, then we'll stream logs after opening the browser
    docker compose up --build -d
}

# Wait for the API to be healthy (up to 60 s)
Write-Host "Waiting for API to be ready..." -ForegroundColor Yellow
$timeout = 60
$elapsed = 0
while ($elapsed -lt $timeout) {
    try {
        $response = Invoke-WebRequest -Uri $API_HEALTH -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($response.StatusCode -eq 200) { break }
    } catch { }
    Start-Sleep -Seconds 2
    $elapsed += 2
}

if ($elapsed -ge $timeout) {
    Write-Host "API did not become ready in time. Check 'docker compose logs api'." -ForegroundColor Red
} else {
    Write-Host "API is ready." -ForegroundColor Green
}

Write-Host "Opening $UI_URL ..." -ForegroundColor Cyan
Start-Process $UI_URL

if (-not $Detach) {
    Write-Host "Streaming logs (Ctrl-C to stop)..." -ForegroundColor DarkGray
    docker compose logs -f
}
