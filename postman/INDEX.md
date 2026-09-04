# Postman Collection - Complete Index

## 📚 Documentation Files

### Core Files
| File | Purpose | Size | Format |
|------|---------|------|--------|
| `Credit-Risk-Compliance-Collection.postman_collection.json` | Main test collection with 25+ requests | 31 KB | JSON |
| `Credit-Risk-Compliance-Environment.postman_environment.json` | Environment variables template | 1.6 KB | JSON |

### Guides
| File | Purpose | Size | Focus |
|------|---------|------|-------|
| `README.md` | Quick start and overview | 7.3 KB | Getting started |
| `POSTMAN_GUIDE.md` | Complete usage guide | 8.5 KB | How to import and use |
| `TEST_SCENARIOS.md` | Detailed test case documentation | 20 KB | Expected behaviors |
| `CI-CD-INTEGRATION.md` | Integration with CI/CD systems | 8 KB | Automation |

### Scripts
| File | Purpose | Type | Executable |
|------|---------|------|-----------|
| `run-postman-tests.sh` | Newman CLI runner with reports | Bash | ✅ Yes |

---

## 🎯 Quick Navigation

### For First Time Users
1. Start here: **[README.md](README.md)**
2. Detailed guide: **[POSTMAN_GUIDE.md](POSTMAN_GUIDE.md)**
3. Import collection into Postman

### For Test Execution
1. Import collection and environment into Postman
2. Select environment from dropdown
3. Run requests folder by folder

### For Understanding Test Cases
1. Read: **[TEST_SCENARIOS.md](TEST_SCENARIOS.md)**
2. For each test, understand:
   - Request format
   - Expected response
   - Test assertions
   - Variable dependencies

### For Automation/CI-CD
1. Install Newman: `npm install -g newman`
2. Read: **[CI-CD-INTEGRATION.md](CI-CD-INTEGRATION.md)**
3. Run via script: `./run-postman-tests.sh --run`

---

## 📋 Collection Structure

```
Credit Risk Compliance System
│
├─ Setup & Health Checks (5 requests)
│  ├─ Health - Bureau Mock (8081)
│  ├─ Health - IAM (5000)
│  ├─ Health - Credit Analysis (5001)
│  ├─ Health - Compliance (5002)
│  └─ Health - Operations Server (5003)
│
├─ Authentication (4 requests)
│  ├─ 1. Create User
│  ├─ 2. Login
│  ├─ 3. Get User
│  └─ 4. Logout
│
├─ Credit Analysis (3 requests)
│  ├─ 1. Create Proposal
│  ├─ 2. Get Proposal
│  └─ 3. Submit Proposal
│
├─ Compliance (6 requests)
│  ├─ 1. Ingest Transaction (Normal)
│  ├─ 2. Ingest Transaction (Large - Flagged)
│  ├─ 3. Get Transaction
│  ├─ 4. List Transactions
│  ├─ 5. List Alerts
│  └─ 6. Review Alert
│
└─ Bureau Mock (3 requests)
   ├─ 1. Query Bureau (Success)
   ├─ 2. Query Bureau Error (500)
   └─ 3. Query Bureau Error (503)
```

**Total:** 25 requests across 5 folders

---

## 🔐 Authentication

### Token Flow
```
Create User
    ↓
Login (extract JWT)
    ↓
Use Bearer token in headers
    ↓
Access protected endpoints
    ↓
Logout (invalidate token)
```

### Protected Endpoints
- `GET /api/v1/users/{id}` - Requires token
- `PUT /api/v1/alerts/{id}/review` - Requires token

---

## 🚀 Quick Start Commands

### Using Postman GUI
```bash
# 1. Import collection
Postman → Import → Credit-Risk-Compliance-Collection.postman_collection.json

# 2. Import environment
Postman → Settings → Environments → Import → Credit-Risk-Compliance-Environment.postman_environment.json

# 3. Select environment and run tests
```

### Using Newman CLI
```bash
# Install Newman
npm install -g newman

# Run all tests
cd postman
newman run Credit-Risk-Compliance-Collection.postman_collection.json \
  -e Credit-Risk-Compliance-Environment.postman_environment.json

# Run with HTML report
./run-postman-tests.sh --run --output html

# Run with JSON report
./run-postman-tests.sh --run --output json
```

---

## 📊 Test Coverage

### Endpoints Tested
- ✅ 5 Health check endpoints (all services)
- ✅ 4 Authentication endpoints (IAM)
- ✅ 3 Credit Analysis endpoints
- ✅ 6 Compliance endpoints
- ✅ 3 Bureau Mock endpoints

### Scenarios Covered
- ✅ Happy path (all operations succeed)
- ✅ Token extraction and usage
- ✅ Variable generation and passing
- ✅ Error cases (500, 503)
- ✅ Alert generation and review
- ✅ Transaction categorization

### Test Assertions
- ✅ Status code validation
- ✅ Response structure validation
- ✅ Data type validation
- ✅ Variable extraction

---

## 🔄 Variable Dependencies

```
Setup & Health Checks
    ↓ (no dependencies)
    ↓
Authentication
    ├─ userId ─────────→ Get User
    ├─ bearerToken ────→ Get User, Review Alert
    └─ timestamp ──────→ all flows
    ↓
Credit Analysis
    ├─ proposalId ─────→ Get Proposal, Submit Proposal
    └─ (no auth required)
    ↓
Compliance
    ├─ transactionId ──→ Get Transaction
    ├─ alertId ────────→ Review Alert
    └─ bearerToken ────→ Review Alert (required)
    ↓
Bureau Mock
    └─ (no dependencies)
```

---

## ⚙️ Environment Variables

All variables are defined in `Credit-Risk-Compliance-Environment.postman_environment.json`

| Variable | Initial Value | Auto-Set By | Usage |
|----------|---------------|-------------|-------|
| `baseUrl_Bureau` | http://localhost:8081 | Manual | All Bureau endpoints |
| `baseUrl_IAM` | http://localhost:5000 | Manual | All IAM endpoints |
| `baseUrl_CreditAnalysis` | http://localhost:5001 | Manual | All Credit endpoints |
| `baseUrl_Compliance` | http://localhost:5002 | Manual | All Compliance endpoints |
| `baseUrl_Operations` | http://localhost:5003 | Manual | Operations endpoints |
| `bearerToken` | (empty) | Login | Protected endpoints |
| `userId` | (empty) | Create User | Get User, subsequent flows |
| `proposalId` | (empty) | Create Proposal | Get/Submit Proposal |
| `transactionId` | (empty) | Ingest Transaction | Get Transaction |
| `alertId` | (empty) | List Alerts | Review Alert |
| `timestamp` | (empty) | Pre-request | Unique IDs |

---

## 🧪 Test Scripts

### Pre-request Scripts
Each request may include pre-request scripts that:
- Generate UUIDs for unique identifiers
- Set timestamps
- Prepare dynamic data
- Construct variable values

### Test Scripts
Each request includes test scripts that:
- Validate status codes
- Check response structure
- Extract variables for next requests
- Log results to console

---

## 📖 Documentation Structure

```
postman/
├── README.md ........................... Overview and quick start
├── POSTMAN_GUIDE.md ................... Detailed usage guide
├── TEST_SCENARIOS.md .................. Expected behaviors & test cases
├── CI-CD-INTEGRATION.md ............... Automation setup
│
├── Credit-Risk-Compliance-Collection.postman_collection.json
├── Credit-Risk-Compliance-Environment.postman_environment.json
├── run-postman-tests.sh ............... CLI runner script
│
└── (this file) ........................ You are here
```

---

## 🔧 Customization

### Modify Collection
1. Open in Postman
2. Edit requests, headers, body
3. Export as JSON
4. Replace the collection file

### Modify Environment
1. Create new environment in Postman
2. Update base URLs for your infrastructure
3. Export and use with collection

### Add New Tests
1. Add new folder or request in Postman
2. Configure request (method, URL, headers, body)
3. Add pre-request script (if needed)
4. Add test script (validation)
5. Export collection

---

## 🐛 Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Collection won't import | Check JSON validity: `python3 -m json.tool file.json` |
| Services not responding | Start with `./start-all-services.sh` |
| Variables empty | Run requests sequentially, not in parallel |
| Token invalid (401) | Re-run Login to refresh token |
| 404 Not Found | Create resource first, then retrieve |

---

## 📊 Test Execution Performance

| Aspect | Expected |
|--------|----------|
| Total Requests | 25 |
| Estimated Runtime | 10-15 seconds |
| Pass Rate Target | 100% |
| Assertions | 60+ |

---

## 🎓 Learning Resources

- **Postman Documentation:** https://learning.postman.com/
- **Test Scripts Guide:** https://learning.postman.com/docs/writing-scripts/test-scripts/
- **Pre-request Scripts:** https://learning.postman.com/docs/writing-scripts/pre-request-scripts/
- **Newman CLI:** https://github.com/postmanlabs/newman
- **API Reference:** `../docs/API_REFERENCE.md`
- **Architecture:** `../docs/ARCHITECTURE.md`

---

## 🔗 Related Documentation

Inside this folder:
- [README.md](README.md) - Start here
- [POSTMAN_GUIDE.md](POSTMAN_GUIDE.md) - Complete guide
- [TEST_SCENARIOS.md](TEST_SCENARIOS.md) - Test details
- [CI-CD-INTEGRATION.md](CI-CD-INTEGRATION.md) - Automation

Outside this folder:
- `../docs/API_REFERENCE.md` - Endpoint details
- `../docs/ARCHITECTURE.md` - System design
- `../START_SERVICES.md` - How to start services
- `../docker-compose.yml` - Infrastructure setup

---

## ✅ Checklist

Before running tests:
- [ ] All 5 services running
- [ ] PostgreSQL accessible (docker)
- [ ] RabbitMQ running (docker)
- [ ] Redis running (docker)
- [ ] Collection imported
- [ ] Environment imported and selected
- [ ] Base URLs configured correctly

After running tests:
- [ ] 25/25 requests succeeded
- [ ] All assertions passed
- [ ] Variables extracted correctly
- [ ] No error responses

---

## 🤝 Support

For issues:
1. Check [POSTMAN_GUIDE.md](POSTMAN_GUIDE.md) troubleshooting
2. Review [TEST_SCENARIOS.md](TEST_SCENARIOS.md) expected behaviors
3. Check API logs: `tail -f logs/api.log`
4. Run health checks: `./run-postman-tests.sh`

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-08-27 | Initial release |

---

## 👤 Maintainer

**AI Cockpit** - Last updated: 2026-08-27

---

**🎯 Ready to start testing? Begin with [README.md](README.md) →**
