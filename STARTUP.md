# 🚀 STARTUP GUIDE - Credit Risk Compliance System

This document explains how to start the entire system using the unified `start-all-services.sh` script.

## Quick Start (Recommended)

```bash
./start-all-services.sh
```

This command will:
1. ✅ Start Docker infrastructure (PostgreSQL, RabbitMQ, Redis, Keycloak)
2. ✅ Wait for containers to be ready
3. ✅ Start all 7 application services
4. ✅ Display logs and next steps

## Available Modes

### Default Mode (Full Setup with Auto-Detect)
```bash
./start-all-services.sh
# or
./start-all-services.sh full
```

Starts infrastructure + services (uses tmux if available, otherwise background mode)

### Infrastructure Only
```bash
./start-all-services.sh infra-only
```

Useful if you want to manage services separately:
```bash
# Terminal 1: Start infrastructure
./start-all-services.sh infra-only

# Terminal 2+: Start services
./start-all-services.sh services-only
```

### Services Only
```bash
./start-all-services.sh services-only
```

(Assumes Docker infrastructure is already running)

### Specific Service Mode: TMux
```bash
./start-all-services.sh tmux
```

Start services in interactive tmux windows (requires tmux installed)

### Specific Service Mode: Background
```bash
./start-all-services.sh background
```

Start services as background processes (logs to `/tmp/`)

### Help
```bash
./start-all-services.sh help
```

Display help and usage information

## Services Started

| Service | Port | Type |
|---------|------|------|
| Bureau Mock Service | 8081 | External API Mock |
| IAM API | 5000 | Authentication |
| Credit Analysis API | 5001 | Core Business Logic |
| Compliance API | 5002 | Core Business Logic |
| Operations Server | 5003 | SignalR Hub |
| Credit Analysis Worker | - | Background Service |
| Compliance Worker | - | Background Service |

## Docker Infrastructure

| Service | Port(s) | Status |
|---------|---------|--------|
| PostgreSQL | 5432 | Auto-verified |
| RabbitMQ | 5672, 15672 | Auto-verified |
| Redis | 6379 | Auto-verified |
| Keycloak | 8080 | Running |

## Viewing Logs

### TMux Mode
```bash
tmux attach -t creditrisk
# Switch between windows with Ctrl+B then 0-6
```

### Background Mode
```bash
tail -f /tmp/bureau.log
tail -f /tmp/iam.log
tail -f /tmp/credit.log
tail -f /tmp/compliance.log
tail -f /tmp/operations.log
tail -f /tmp/ca-worker.log
tail -f /tmp/comp-worker.log
```

## Testing Services

After services have started (wait ~30-60 seconds), test health endpoints:

```bash
# Test Credit Analysis API
curl http://localhost:5001/health

# Test Compliance API
curl http://localhost:5002/health

# Test Operations Server
curl http://localhost:5003/health

# Test Bureau Mock
curl http://localhost:8081/health
```

## Stopping Services

### Stop Application Services
```bash
killall dotnet
```

### Stop Docker Infrastructure
```bash
docker-compose down
```

### Stop Everything
```bash
killall dotnet
docker-compose down
```

## Troubleshooting

### Docker containers not starting
```bash
# Check Docker status
docker-compose ps

# View Docker logs
docker-compose logs -f

# Restart containers
docker-compose restart
```

### Services not responding
```bash
# Check if processes are running
ps aux | grep dotnet

# Check if ports are listening
ss -tulpn | grep -E ":(5000|5001|5002|5003|8081)"

# View service logs
tail -f /tmp/credit.log
```

### PostgreSQL connection refused
```bash
# Verify PostgreSQL is running
docker-compose ps | grep postgres

# Wait for PostgreSQL to be ready
docker-compose logs postgres

# Restart if needed
docker-compose restart postgres
```

### RabbitMQ not ready
```bash
# Check RabbitMQ status
docker-compose logs rabbitmq

# Access RabbitMQ management UI
# http://localhost:15672 (guest/guest)
```

## Development Workflow

### Option 1: Interactive Development (TMux)
```bash
# Start everything
./start-all-services.sh tmux

# In another terminal, make code changes
# Services auto-reload on file save (if configured)

# View specific service logs
tmux attach -t creditrisk
Ctrl+B then 2  # Switch to Credit Analysis window
```

### Option 2: Separate Infrastructure and Services
```bash
# Terminal 1: Start infrastructure only
./start-all-services.sh infra-only

# Terminal 2: Start services in background
./start-all-services.sh services-only

# Terminal 3: Run your IDE/editor
code .

# Terminal 4: Monitor logs
tail -f /tmp/credit.log
```

### Option 3: Run Services from IDE
```bash
# Terminal 1: Start infrastructure only
./start-all-services.sh infra-only

# Terminal 2+: Run individual services from your IDE
# In Visual Studio Code:
# - Set startup project to CreditRisk.CreditAnalysis.Api
# - Press F5 to debug
```

## Advanced Usage

### Custom Environment Variables
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://0.0.0.0:5001"
./start-all-services.sh background
```

### Rebuilding Before Starting
```bash
dotnet build -c Debug
./start-all-services.sh background
```

### Running Tests
```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName!~Integration"

# Integration tests
dotnet test tests/integration

# All tests with coverage
dotnet test /p:CollectCoverage=true
```

## Next Steps

After services are running:

1. **Test the APIs**
   ```bash
   curl http://localhost:5001/health
   ```

2. **Read documentation**
   - `START_SERVICES.md` - Detailed service information
   - `docs/API_REFERENCE.md` - API endpoints
   - `docs/ARCHITECTURE.md` - System design

3. **Make sample requests**
   See `docs/API_REFERENCE.md` for example requests

4. **Monitor in real-time**
   - Open browser to `http://localhost:5003` for Operations Dashboard
   - Or use logs: `tail -f /tmp/credit.log`

## Support

For issues or questions:
- Check logs: `tail -f /tmp/*.log`
- Review documentation: `docs/` folder
- Run help: `./start-all-services.sh help`

---

**Happy coding! 🚀**
