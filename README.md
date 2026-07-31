# DomainScanner

Backend service for storing domains, running HTTP checks, and monitoring their
availability. The API and the Hangfire worker run as separate processes and use
PostgreSQL and Redis.

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
├── nginx/
├── docker-compose.dev.yaml
├── docker-compose.yaml
├── .env.dev.example
├── .env.example
└── setup-dev.ps1
```

## Requirements

- Docker with Docker Compose
- .NET SDK 10
- PowerShell for automatic development certificate setup

The default development ports from `.env.dev.example` are:

| Service | Port |
| --- | --- |
| PostgreSQL | `5432` |
| Redis | `6379` |
| API HTTP | `8080` |
| API HTTPS | `8443` |

Change these values in `localhost/.env.dev` if a port is already occupied.

## Development

The development environment enables HTTPS and Swagger UI.

Run from the repository root:

```powershell
.\localhost\setup-dev.ps1

docker compose `
  --env-file localhost/.env.dev `
  -f localhost/docker-compose.dev.yaml `
  up --build
```

The setup script:

- creates and trusts a local HTTPS certificate;
- exports it to `localhost/certs/domainscanner-dev.pfx`;
- creates `localhost/.env.dev` from `.env.dev.example` when necessary;
- synchronizes the certificate password with `HTTPS_CERT_PASSWORD`.

To use a custom certificate password:

```powershell
.\localhost\setup-dev.ps1 -Password "your-password"
```

Before the first start, replace the placeholder values for `JWT_SECRETKEY` and
`LOGIN_ACCOUNT_KEY_SECRET` in `localhost/.env.dev`. Use independent secrets with
at least 32 bytes of entropy. The login account key secret must be Base64-encoded
and must not reuse the JWT secret.

A suitable Base64 secret can be generated with PowerShell:

```powershell
$bytes = [byte[]]::new(32)
[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

Available development URLs:

- Swagger: `https://localhost:8443/swagger`
- API: `https://localhost:8443`
- Health check: `https://localhost:8443/health`

HTTPS is required for the session and antiforgery cookies marked as `Secure`.

Stop the development environment:

```powershell
docker compose `
  --env-file localhost/.env.dev `
  -f localhost/docker-compose.dev.yaml `
  down
```

### Manual HTTPS setup

The PowerShell setup script is optional. The certificate can be prepared
manually:

```powershell
Copy-Item localhost/.env.dev.example localhost/.env.dev
New-Item -ItemType Directory -Force localhost/certs

dotnet dev-certs https --trust
dotnet dev-certs https `
  -ep localhost/certs/domainscanner-dev.pfx `
  -p "local-dev-password"
```

The certificate password must match `HTTPS_CERT_PASSWORD` in
`localhost/.env.dev`.

## Production Compose

The production Compose configuration exposes the API through Nginx on ports
`80` and `443`. PostgreSQL and Redis are connected through internal Docker
networks, while the Worker has a separate network for outbound HTTP checks.

Prepare the environment file:

```powershell
Copy-Item localhost/.env.example localhost/.env
```

Replace all placeholder credentials and secrets. Install the TLS certificate
and private key at:

```text
localhost/nginx/certs/fullchain.pem
localhost/nginx/certs/privkey.pem
```

Then run:

```powershell
docker compose `
  --env-file localhost/.env `
  -f localhost/docker-compose.yaml `
  up --build -d
```

The production configuration enables forwarded headers only for the trusted
Nginx address configured by `TRUSTED_PROXY_ADDRESS`. This ensures that IP-based
rate limiting uses the original client address.

## Configuration

Configuration defaults are stored separately for the API and Worker:

- `backend/src/DomainScanner.Api/appsettings.json`
- `backend/src/DomainScanner.Worker/appsettings.json`

Docker Compose overrides environment-specific values through environment
variables using the standard .NET `Section__Property` notation.

### API configuration

| Section | Purpose |
| --- | --- |
| `ConnectionStrings` | PostgreSQL and Redis connections |
| `JwtOptions` | JWT issuer, audience, lifetime, and secret key |
| `DataProtection` | Persistent ASP.NET Core data-protection keys |
| `RedisCacheOptions` | Redis cache prefix and expiration |
| `ReverseProxy` | Trusted proxy and forwarded-header processing |
| `LoginAccountKeyOptions` | HMAC secret for non-reversible login account keys |
| `LoginProtectionOptions` | Failed-attempt delay, lockout, and escalation rules |
| `RateLimiting` | Per-policy request and scan concurrency limits |

`JwtOptions`, `LoginAccountKeyOptions`, and login protection are registered only
by the API. The Worker does not require JWT or HMAC secrets.

### Worker configuration

| Section | Purpose |
| --- | --- |
| `ConnectionStrings` | PostgreSQL and Redis connections |
| `DomainChecksWorker` | Recurring job ID, cron expression, queue, and batch size |

### Login protection defaults

Failed login state is stored in Redis using HMAC-derived account keys. Raw email
addresses are not used as Redis keys.

| Setting | Default |
| --- | ---: |
| Failure window | 15 minutes |
| Lockout threshold | 5 attempts |
| Initial lockout | 10 minutes |
| Maximum lockout | 60 minutes |
| Escalation window | 1440 minutes |
| Delay starts at | 3rd failed attempt |
| Initial delay | 500 ms |
| Maximum delay | 2000 ms |

Temporarily blocked login attempts return `429 Too Many Requests` with a
`Retry-After` header.

### Rate limiting defaults

Authenticated requests are partitioned by user ID. Anonymous requests are
partitioned by remote IP address.

| Policy | Limit | Window | Applied to |
| --- | ---: | ---: | --- |
| `read` | 100 | 60 seconds | Read endpoints |
| `write` | 20 | 60 seconds | Create, update, and delete endpoints |
| `auth` | 5 | 60 seconds | Registration, logout, and CSRF token endpoints |
| `login` | 10 | 60 seconds | Login endpoint |
| `scan` | 15 | 60 seconds | Domain HTTP checks |

Scan endpoints additionally allow up to five concurrent requests per client.
Rate-limited responses return `429 Too Many Requests` and include `Retry-After`
when the limiter can calculate it. The `/health` endpoint is excluded from rate
limiting.

## Authentication and CSRF

Successful login stores the JWT in a secure, HTTP-only session cookie. For
state-changing API requests (`POST`, `PUT`, `PATCH`, and `DELETE`):

1. Request a token from `GET /api/v1/auth/csrf`.
2. Preserve the returned antiforgery cookie.
3. Send the returned token in the `X-CSRF-TOKEN` header.

Swagger UI performs this flow automatically in the development environment.

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

## Tests

Run all tests from the repository root:

```powershell
dotnet test backend/src/DomainScanner.sln
```

Infrastructure integration tests use Testcontainers and therefore require a
running Docker daemon.
