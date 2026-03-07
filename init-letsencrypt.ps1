# init-letsencrypt.ps1
# Run this ONCE to obtain the initial Let's Encrypt certificate.
# Requirements:
#   - Port 80 must be publicly accessible from the internet (router/firewall forwarded to this machine)
#   - Domain gamelist.ddns.net must point to your public IP
#   - Docker must be running

param(
    [string]$Email = "",
    [switch]$Staging  # Use Let's Encrypt staging server (no rate limits, for testing)
)

if (-not $Email) {
    $Email = Read-Host "Enter your email for Let's Encrypt notifications"
}

$Domain = "gamelist.ddns.net"
$StagingFlag = if ($Staging) { "--staging" } else { "" }

Write-Host "`n[1/4] Starting with HTTP-only nginx config..." -ForegroundColor Cyan

# Note: nginx/conf.d/app.conf is already the HTTP-only config
# nginx/conf.d/app.https.conf.template contains the HTTPS config

Write-Host "[2/4] Starting containers (db + web + nginx)..." -ForegroundColor Cyan
docker-compose up -d db web nginx

Start-Sleep -Seconds 10

Write-Host "[3/4] Requesting Let's Encrypt certificate for $Domain..." -ForegroundColor Cyan

docker run --rm `
    -v gamelist-dotnet_certbot_www:/var/www/certbot `
    -v gamelist-dotnet_certbot_certs:/etc/letsencrypt `
    certbot/certbot certonly `
    --webroot `
    --webroot-path=/var/www/certbot `
    --email $Email `
    --agree-tos `
    --no-eff-email `
    $StagingFlag `
    -d $Domain

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] Certbot failed. Check that:" -ForegroundColor Red
    Write-Host "  - Port 80 is forwarded to this machine in your router" -ForegroundColor Red
    Write-Host "  - $Domain resolves to your public IP" -ForegroundColor Red
    Write-Host "  - No firewall is blocking port 80" -ForegroundColor Red
    # Restore HTTP config for next attempt (no changes needed, app.conf is already HTTP-only)
    exit 1
}

Write-Host "[4/4] Certificate obtained! Switching to HTTPS config and reloading nginx..." -ForegroundColor Cyan

# Activate full HTTPS config (copy template to active conf)
Copy-Item "nginx\conf.d\app.https.conf.template" "nginx\conf.d\app.conf" -Force

# Start certbot renewal service and reload nginx
docker-compose up -d certbot
docker-compose exec nginx nginx -s reload

Write-Host "`n[OK] HTTPS is ready at https://${Domain}:50443" -ForegroundColor Green
Write-Host "     Certificates will auto-renew every 12 hours." -ForegroundColor Green
