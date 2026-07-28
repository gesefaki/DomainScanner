param(
    [string]$Password = "local-dev-password"
)

$certDirectory = Join-Path $PSScriptRoot "certs"
$certPath = Join-Path $certDirectory "domainscanner-dev.pfx"
$envPath = Join-Path $PSScriptRoot ".env.dev"

New-Item -ItemType Directory -Force $certDirectory | Out-Null
Remove-Item $certPath -Force -ErrorAction SilentlyContinue

dotnet dev-certs https --trust
dotnet dev-certs https -ep $certPath -p $Password

if (-not (Test-Path $envPath)) {
    Copy-Item (Join-Path $PSScriptRoot ".env.dev.example") $envPath
}

(Get-Content $envPath) `
    -replace '^HTTPS_CERT_PASSWORD=.*$', "HTTPS_CERT_PASSWORD=$Password" |
    Set-Content $envPath

Write-Host "HTTPS certificate is ready. Run Docker Compose."
