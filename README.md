# DomainScanner

Backend service for storing and monitoring domains.

## Structure

```text
backend/
├── src/
│   ├── DomainScanner.Api
│   ├── DomainScanner.Application
│   ├── DomainScanner.Contracts
│   ├── DomainScanner.Domain
│   ├── DomainScanner.Infrastructure
│   ├── DomainScanner.Shared
│   └── DomainScanner.Worker
└── tests/

localhost/
├── docker-compose.dev.yaml
├── docker-compose.yaml
├── .env.dev.example
├── .env.example
└── setup-dev.ps1
```

## Requirements

- Docker with Docker Compose
- .NET SDK 10
- PowerShell

The default development ports are:

| Service | Port |
| --- | --- |
| PostgreSQL | `5433` |
| Redis | `6380` |
| API HTTP | `8081` |
| API HTTPS | `8444` |

## Running

### Development with PowerShell

The development environment is recommended because it enables Swagger UI.

Run from the repository root:

```powershell
.\localhost\setup-dev.ps1

docker compose `
  --env-file localhost/.env.dev `
  -f localhost/docker-compose.dev.yaml `
  up --build
```

The script creates and trusts a local HTTPS certificate, exports it to
`localhost/certs`, and creates `.env.dev` when necessary.

To use a custom certificate password:

```powershell
.\localhost\setup-dev.ps1 -Password "your-password"
```

Available development URLs:

- Swagger: `https://localhost:8444/swagger`
- API: `https://localhost:8444`
- Health check: `https://localhost:8444/health`

HTTPS is required for authentication and CSRF cookies marked as `Secure`.

Stop the development environment:

```powershell
docker compose `
  --env-file localhost/.env.dev `
  -f localhost/docker-compose.dev.yaml `
  down
```

### Manual HTTPS setup

The PowerShell script is optional. The certificate can be prepared manually:

```powershell
Copy-Item localhost/.env.dev.example localhost/.env.dev
New-Item -ItemType Directory -Force localhost/certs

dotnet dev-certs https --trust
dotnet dev-certs https `
  -ep localhost/certs/domainscanner-dev.pfx `
  -p "local-dev-password"
```

The certificate password must match `HTTPS_CERT_PASSWORD` in
`localhost/.env.dev`. After creating the certificate, use the development
Docker Compose command shown above.

## API Endpoints

Most endpoints require authentication.

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/v1/auth/csrf` | Get a CSRF token |
| `POST` | `/api/v1/auth` | Sign in |
| `POST` | `/api/v1/auth/logout` | Sign out |
| `POST` | `/api/v1/users/register` | Register a user |
| `GET` | `/api/v1/users/me` | Get the current user |
| `GET` | `/api/v1/users/me/domains` | Get the current user's domains |
| `PUT` | `/api/v1/users/me/activate` | Activate the current user |
| `PUT` | `/api/v1/users/me/deactivate` | Deactivate the current user |
| `DELETE` | `/api/v1/users/me` | Delete the current user |
| `GET` | `/api/v1/domains/{id}` | Get a domain |
| `POST` | `/api/v1/domains` | Add a domain |
| `PUT` | `/api/v1/domains/{id}` | Update a domain |
| `DELETE` | `/api/v1/domains/{id}` | Delete a domain |
| `GET` | `/api/v1/domains/{id}/http/check` | Run a basic HTTP check |
| `GET` | `/api/v1/domains/{id}/http/check-details` | Run a detailed HTTP check |
| `POST` | `/api/v1/domains/{id}/send-save` | Run and save a check |
| `GET` | `/health` | Check API health |
