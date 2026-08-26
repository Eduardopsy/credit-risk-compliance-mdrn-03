# run-script.sh — Agent Manager Integration

## Overview

`run-script.sh` is the development environment initialization script for the **Credit Risk Compliance Lab** backend services. It runs automatically when the "Run" button is clicked in Agent Manager and orchestrates:

1. **Pre-flight validation** (dependencies, Docker, .NET SDK)
2. **Docker services initialization** (PostgreSQL, Redis, RabbitMQ, Keycloak)
3. **Solution compilation** (Release build)
4. **Sequential API startup** (IAM → Credit Analysis → Compliance)
5. **Logging & monitoring** (separated logs per service)
6. **Graceful cleanup** (on Ctrl+C)

---

## Script Location

```
.aicockpit/run-script.sh
```

**Status**: ✅ Executable and tested
**Size**: ~500 lines
**Language**: Bash (POSIX-compatible)

---

## How It Works

### Execution Flow

```
Start
  ↓
Pre-flight Checks (.env, Docker, .NET SDK)
  ↓
Docker Services Management (start if offline, health check)
  ↓
Build Solution (dotnet build CreditRiskComplianceLab.sln -c Release)
  ↓
Sequential API Startup:
  • IAM API (port 5001)
  • Credit Analysis API (port 5002)
  • Compliance API (port 5003)
  ↓
Display URLs & Log Locations
  ↓
Await Ctrl+C
  ↓
Cleanup (kill processes, docker-compose down)
  ↓
Exit
```

### Timeline

- **Pre-flight checks**: ~2-3 seconds
- **Docker health checks**: ~5-10 seconds (if offline)
- **Build compilation**: ~15-20 seconds
- **API sequential startup**: ~5-10 seconds
- **Total**: 30-45 seconds

---

## Usage

### Via Agent Manager (Primary)

1. Open VS Code with the project
2. Click **Agent Manager** sidebar icon
3. Select worktree: **backend-unit-tests**
4. Click **Run** button
5. Script executes automatically in terminal

### Manual Testing

```bash
# Set environment variables
export WORKTREE_PATH="$(pwd)/.aicockpit/worktrees/backend-unit-tests"
export REPO_PATH="$(pwd)"

# Execute script
.aicockpit/run-script.sh
```

### Stop Services

Press **Ctrl+C** to:
- Kill all API processes gracefully
- Stop docker-compose services
- Clean up `.run-pids` file
- Free system resources

---

## Output Example

```
╔════════════════════════════════════════════════════════════════╗
║  Credit Risk Compliance Lab — Dev Environment                 ║
║  Worktree: backend-unit-tests                                 ║
╚════════════════════════════════════════════════════════════════╝

────────────────────────────────────────────────────────────────
Pre-flight Checks
────────────────────────────────────────────────────────────────
[✓] .env found
[✓] Docker installed (Docker version 29.1.3)
[✓] Docker Compose installed (docker-compose version 1.29.2)
[✓] .NET SDK available (8.0.130)
[✓] Logs directory ready: logs

────────────────────────────────────────────────────────────────
Docker Services
────────────────────────────────────────────────────────────────
[INFO] Checking service status...
[✓] Services already running and healthy

────────────────────────────────────────────────────────────────
Building Solution
────────────────────────────────────────────────────────────────
[INFO] Building: dotnet build CreditRiskComplianceLab.sln
[✓] Build successful

────────────────────────────────────────────────────────────────
Starting APIs (Sequential)
────────────────────────────────────────────────────────────────
[INFO] [1/3] iam
[✓] iam started (PID: 12345)

[INFO] [2/3] credit
[✓] credit started (PID: 12346)

[INFO] [3/3] compliance
[✓] compliance started (PID: 12347)

────────────────────────────────────────────────────────────────
🚀 Ready for Development
────────────────────────────────────────────────────────────────

URLs:
  • IAM API:              http://localhost:5001
  • Credit Analysis API:  http://localhost:5002
  • Compliance API:       http://localhost:5003

Health Endpoints:
  • IAM:              http://localhost:5001/health
  • Credit Analysis:  http://localhost:5002/health
  • Compliance:       http://localhost:5003/health

Log Files:
  • IAM:              logs/iam.log
  • Credit Analysis:  logs/credit.log
  • Compliance:       logs/compliance.log

Press Ctrl+C to stop all services and cleanup
```

---

## Generated Files & Artifacts

After successful startup:

| File | Purpose |
|------|---------|
| `.run-pids` | Tracks process IDs for cleanup |
| `logs/iam.log` | IAM API output (live) |
| `logs/credit.log` | Credit Analysis API output (live) |
| `logs/compliance.log` | Compliance API output (live) |
| `logs/build.log` | Build compilation output |

---

## Viewing Logs

### Real-time Log Monitoring

In another terminal:

```bash
# IAM API logs
tail -f logs/iam.log

# Credit Analysis API logs
tail -f logs/credit.log

# Compliance API logs
tail -f logs/compliance.log

# Build logs
tail -f logs/build.log
```

### Log File Locations

Logs are stored relative to worktree:

```
.aicockpit/worktrees/backend-unit-tests/logs/
├── iam.log
├── credit.log
├── compliance.log
└── build.log
```

---

## Configuration & Environment

### Required Environment Variables

Set by Agent Manager automatically:

| Variable | Example | Purpose |
|----------|---------|---------|
| `WORKTREE_PATH` | `/path/to/worktree` | Worktree root directory |
| `REPO_PATH` | `/path/to/repo` | Repository root directory |

### .env File

Must exist in repo root:

```bash
# Location
repo-root/.env
```

Copied from worktree during setup-script initialization.

---

## APIs Configuration

### Sequential Startup Order

1. **IAM API** (port 5001)
   - Project: `src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj`
   - Logs: `logs/iam.log`

2. **Credit Analysis API** (port 5002)
   - Project: `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj`
   - Logs: `logs/credit.log`

3. **Compliance API** (port 5003)
   - Project: `src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj`
   - Logs: `logs/compliance.log`

---

## Docker Services

### Auto-managed Services

The script automatically starts if offline:

| Service | Container | Port |
|---------|-----------|------|
| PostgreSQL | `crcl-postgres` | 5432 |
| Redis | `crcl-redis` | 6379 |
| RabbitMQ | `crcl-rabbitmq` | 5672 / 15672 |
| Keycloak | `crcl-keycloak` | 8080 |

### Health Checks

- Postgres: `pg_isready` validation (120s timeout, 2s retry)
- Services marked as "healthy" before proceeding

### Cleanup

On exit (Ctrl+C):
- All API processes terminated
- `docker-compose down` executed (services stop, resources freed)

---

## Troubleshooting

### Issue: ".env not found"

**Solution**: Run `setup-script.sh` first

```bash
.aicockpit/setup-script.sh
```

### Issue: "Docker not installed"

**Solution**: Install Docker

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install docker.io docker-compose

# Verify
docker --version
docker-compose --version
```

### Issue: ".NET SDK not found"

**Solution**: Install .NET SDK 8.0 or higher

```bash
# Visit: https://dotnet.microsoft.com/download/dotnet/8.0

# Verify
dotnet --version
```

### Issue: "Services failed to become ready"

**Possible causes**:
- Docker daemon not running
- Insufficient disk space
- Port conflicts (5432, 5672, 6379, 8080 in use)

**Solution**:
- Check Docker: `docker ps`
- Free disk space: `df -h`
- Check ports: `netstat -tuln | grep LISTEN`

### Issue: Build fails with errors

**Solution**: Check build logs

```bash
tail -f logs/build.log
```

Common issues:
- Missing NuGet packages: `dotnet restore CreditRiskComplianceLab.sln`
- Wrong .NET SDK version: `dotnet --version` (need 8.0+)

### Issue: API fails to start

**Solution**: Check API-specific logs

```bash
# Which API failed?
tail -f logs/iam.log      # or credit.log or compliance.log
```

Common issues:
- Port already in use
- Database connection failed
- Configuration error in appsettings.json

### Issue: Ctrl+C doesn't stop services

**Solution**: Force cleanup

```bash
# In another terminal
pkill -f "dotnet run"
docker-compose down
```

---

## Implementation Details

### Script Structure (9 Sections)

1. **Configuration & Environment**
   - Global variables and constants
   - API array definition
   - Color codes for output

2. **Utility Functions**
   - Logging helpers (log_info, log_success, log_warn, log_error)
   - Section headers
   - Command/file validation

3. **Cleanup & Signal Handling**
   - `cleanup()` function
   - SIGINT trap (Ctrl+C)
   - EXIT trap

4. **Pre-flight Checks**
   - Validate .env exists
   - Check Docker installation
   - Check Docker Compose
   - Verify .NET SDK
   - Create logs directory

5. **Docker Services Management**
   - Status check (`docker-compose ps`)
   - Auto-start if offline
   - Health verification (pg_isready)
   - 120s timeout with retry

6. **Build Compilation**
   - `dotnet build` Release mode
   - Log to file
   - Exit code verification

7. **Sequential API Startup**
   - Loop through APIs array
   - PID tracking in `.run-pids`
   - Process alive verification
   - 2-second readiness wait

8. **Display Feedback**
   - URLs and health endpoints
   - Log file locations
   - Monitoring instructions

9. **Main Execution Loop**
   - Phase orchestration
   - Infinite loop (await Ctrl+C)
   - Automatic trap cleanup

---

## Features

### Logging & Output

- **Colorized logs** for clarity (green/red/yellow/blue/cyan)
- **Timestamped sections** for progress tracking
- **Error visibility** with context
- **Separate log files** per API

### Process Management

- **PID tracking** in `.run-pids` file
- **Process alive checks** before confirmation
- **Graceful termination** (SIGINT handling)
- **Resource cleanup** (docker-compose down)

### Error Handling

- **Pre-flight validation** prevents silent failures
- **Health checks** ensure readiness before API start
- **Exit codes** properly propagated
- **Cleanup on exit** regardless of success/failure

### Development Convenience

- **Auto-start Docker** if offline
- **Auto-stop Docker** on exit (clean shutdown)
- **Full rebuild** each run (safety over speed)
- **Direct log access** via tail commands

---

## Performance Characteristics

| Phase | Duration | Notes |
|-------|----------|-------|
| Pre-flight checks | 2-3s | Validates environment |
| Docker health check | 5-10s | Only if services offline |
| Build compilation | 15-20s | Release mode |
| API startup (3x) | 5-10s | Sequential, 2s per API |
| **Total** | **30-45s** | Typical developer flow |

---

## Integration with Agent Manager

### How Agent Manager Uses It

1. User clicks "Run" button
2. Agent Manager reads `agent-manager.json` config
3. Executes `.aicockpit/run-script.sh` in worktree directory
4. Sets `WORKTREE_PATH` and `REPO_PATH` environment variables
5. Captures output in VS Code terminal
6. User can interact (Ctrl+C to stop)

### Configuration File

```json
{
  "worktrees": [
    {
      "name": "backend-unit-tests",
      "branch": "feature/backend-unit-tests",
      "run_script": ".aicockpit/run-script.sh"
    }
  ]
}
```

---

## Best Practices

### Development Workflow

1. ✅ Always run via Agent Manager "Run" button (sets env vars)
2. ✅ Check logs in separate terminal: `tail -f logs/*.log`
3. ✅ Use Ctrl+C to stop (automatic cleanup)
4. ✅ Verify APIs are healthy before testing

### Debugging

1. ✅ Check pre-flight output first (dependency issues)
2. ✅ Review build logs if compilation fails
3. ✅ Check API logs for runtime errors
4. ✅ Verify ports not in use: `netstat -tuln`

### Common Tasks

**Rebuild without full startup**:
```bash
cd repo-root
dotnet build CreditRiskComplianceLab.sln -c Release
```

**Check API health**:
```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

**Access logs directly**:
```bash
cd .aicockpit/worktrees/backend-unit-tests/logs/
tail -100 iam.log
```

---

## Maintenance & Updates

### Modifying API List

To add/remove APIs, edit the `APIS` array in `run-script.sh`:

```bash
declare -a APIS=(
    "path/to/project.csproj|PORT|name"
    # Add new entries following this pattern
)
```

### Changing Build Configuration

Modify build command in "Build Compilation" section:

```bash
# Change from Release to Debug
dotnet build CreditRiskComplianceLab.sln -c Debug
```

### Custom Environment Variables

Add to script before `main()`:

```bash
export CUSTOM_VAR="value"
```

---

## Support & Troubleshooting

For issues:

1. Check `.aicockpit/RUN_SCRIPT.md` (this file)
2. Review `logs/build.log` for build errors
3. Check API logs: `logs/{iam,credit,compliance}.log`
4. Verify dependencies: `which docker`, `which dotnet`
5. Ensure ports available: `netstat -tuln`

---

## Version

- **Script Version**: 1.0
- **Created**: 2026-08-20
- **Target Environment**: Linux, Bash 4.0+
- **Dependencies**: Docker, Docker Compose, .NET SDK 8.0+, Bash

---

## License

Part of Credit Risk Compliance Lab project.
