# Documentation Index

## 📚 Core Documentation

### [README.md](../README.md) 🏠
**Main project overview and quick start guide**
- Feature highlights
- Quick start for local development
- Architecture overview with diagrams
- API usage examples
- Technology stack
- Performance metrics
- Testing strategies
- Deployment options

### [COMPLETION_SUMMARY.md](../COMPLETION_SUMMARY.md) ✅
**Project completion status and achievements**
- All 8 phases completion checklist
- Project statistics and metrics
- Features implemented
- Quality metrics achieved
- Technology stack details
- Future roadmap

## 🔌 API & Integration

### [API_REFERENCE.md](API_REFERENCE.md) 📖
**Complete REST API and WebSocket documentation**
- Credit Analysis API endpoints (Port 5001)
  - POST /proposals - Create credit proposal
  - GET /proposals/{id} - Get proposal details
  - GET /proposals/{id}/scoring - Get scoring result
  - GET /proposals - List proposals
- Compliance API endpoints (Port 5002)
  - POST /transactions - Ingest transaction
  - GET /transactions/{id} - Get transaction details
  - GET /transactions - List transactions
  - GET /alerts - List alerts
  - GET /alerts/{id} - Get alert details
  - PUT /alerts/{id}/review - Review alert
- Operations Server - SignalR Hub
  - ReceiveAmlAlert channel
  - ReceiveUrgentAlert channel
  - ReceiveTransactionFlagged channel
  - ReceiveDashboardUpdate channel
- Example client implementations
- Error response formats
- Rate limiting
- Authentication & authorization
- Pagination and filtering

## 🏛️ Architecture & Design

### [ARCHITECTURE.md](ARCHITECTURE.md) 🏗️
**Complete system architecture documentation**
- System overview diagram
- Data flow diagrams
  - Transaction to Alert flow
  - Credit proposal evaluation flow
- Module structure
  - Credit Analysis Module
  - Compliance Module
  - Shared Kernel
  - Message Contracts
- Layered architecture explanation
- Design patterns used
  - Outbox Pattern
  - Consumer Segregation
  - Graceful Degradation
  - Domain-Driven Design
  - Clean Architecture
- Security & compliance measures
- Performance characteristics
- Kubernetes deployment architecture
- Monitoring & observability
- Future enhancements

## 🚀 Deployment & Operations

### [DEPLOYMENT.md](DEPLOYMENT.md) 🐳
**Comprehensive deployment and operations guide**
- Local development setup
  - Prerequisites
  - Infrastructure setup
  - Build & run instructions
- Docker deployment
  - Building images
  - Docker Compose stacks
  - Environment variables
- Kubernetes deployment
  - Building for Kubernetes
  - Creating manifests
  - Deploying to Kubernetes
  - Verifying deployments
- Environment configuration
  - Configuration files
  - Required settings
  - Secrets management
- Database migrations
  - Running migrations
  - Best practices
- Health checks
  - Endpoint documentation
  - Response format
- Monitoring & logging
  - Structured logging
  - Log aggregation
  - Metrics collection
- Troubleshooting
  - Consumer issues
  - Database connection errors
  - Memory usage
  - SignalR issues
- Performance tuning
  - Database optimization
  - Message broker tuning
  - Cache optimization
- Disaster recovery
  - Backup strategies
  - Restoration procedures
  - Message recovery
- Rollback strategy

## 🧪 Testing

### [TESTING_GUIDE.md](TESTING_GUIDE.md) 🧬
**Unit and integration testing documentation**
- Testing overview
  - Test layers (unit, integration, E2E)
- Unit Tests (29 tests)
  - Test projects structure
  - Running unit tests
  - Test coverage metrics
  - Example: Credit Scoring Engine tests
  - Test naming conventions
- Integration Tests (7 tests)
  - Test project structure
  - Testcontainers setup
  - Running integration tests
  - Individual test descriptions
  - Test performance metrics
- Test isolation & ordering
- Fakes vs Mocks comparison
  - AOT-compatible fakes
  - FluentAssertions usage
- Debugging techniques
- Coverage goals
  - Coverage report generation
  - Namespace-specific coverage
- CI/CD integration
  - GitHub Actions workflow example
- Best practices
  - One assertion per test
  - Arrange-Act-Assert pattern
  - Descriptive test names
- Future test enhancements
  - Property-based testing
  - Performance benchmarking
  - Load testing
  - Mutation testing

## ⚡ Compilation & Optimization

### [AOT_COMPILATION.md](AOT_COMPILATION.md) 🔨
**Ahead-of-Time compilation and native executable generation**
- AOT overview
  - Benefits and characteristics
- AOT compilation requirements
  - No reflection-based consumer discovery
  - No dynamic proxy libraries
  - No reflection-based deserialization
  - No Type.GetType() usage
  - Constructor injection only
- Publishing for AOT
  - Project file configuration
  - Trimming configuration
  - Publishing commands
  - Performance benefits (10-20x faster startup)
- Docker multistage build with AOT
- AOT warnings & errors
  - Common issues
  - Solutions and workarounds
- Verification checklist
  - Pre-compilation requirements
  - Post-deployment validation
- Testing AOT compilation
  - Binary verification
  - Performance measurement
  - Memory profiling
- Future: Full source generation
- Reference materials and resources

## 📊 Additional Resources

### Project Statistics
- 23 projects total
- 150+ C# source files
- ~15,000+ lines of code
- 29 unit tests (all passing)
- 7 integration tests (ready)
- 6 documentation guides
- 0 build errors
- 886 style warnings (CA rules)

### Key Technologies
- C# 13 / .NET 8.0.129
- ASP.NET Core 8.0 / SignalR 8.0
- Entity Framework Core 9.0
- PostgreSQL 16+ / RabbitMQ 3.13+ / Redis 7+
- xUnit 2.9.2 / FluentAssertions 7.0.0
- Testcontainers 3.9.0 / Coverlet 6.0.2
- Serilog 4.2.0 for structured logging

### Quick Links
- [GitHub Repository](#) - (Add your repo URL)
- [Issue Tracker](#) - (Add your issue tracker URL)
- [Project Board](#) - (Add your project board URL)

## 📋 Documentation Roadmap

- [x] README.md - Project overview
- [x] API_REFERENCE.md - API documentation
- [x] ARCHITECTURE.md - System design
- [x] DEPLOYMENT.md - Deployment guide
- [x] TESTING_GUIDE.md - Testing documentation
- [x] AOT_COMPILATION.md - AOT compilation guide
- [x] COMPLETION_SUMMARY.md - Project status
- [x] INDEX.md - This file
- [ ] SECURITY.md - Security hardening guide
- [ ] MONITORING.md - Monitoring & alerting setup
- [ ] CONTRIBUTING.md - Contribution guidelines
- [ ] TROUBLESHOOTING.md - Common issues & solutions

## 🎯 How to Use This Documentation

### For New Developers
1. Start with [README.md](../README.md) for overview
2. Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand system design
3. Review [TESTING_GUIDE.md](TESTING_GUIDE.md) for testing patterns
4. Follow [DEPLOYMENT.md](DEPLOYMENT.md) for local setup

### For DevOps/Platform Engineers
1. Read [DEPLOYMENT.md](DEPLOYMENT.md) for deployment strategies
2. Review [AOT_COMPILATION.md](AOT_COMPILATION.md) for optimization
3. Check [ARCHITECTURE.md](ARCHITECTURE.md) for infrastructure requirements
4. Refer to [TESTING_GUIDE.md](TESTING_GUIDE.md) for CI/CD setup

### For QA/Testers
1. Start with [TESTING_GUIDE.md](TESTING_GUIDE.md) for test understanding
2. Review [API_REFERENCE.md](API_REFERENCE.md) for API testing
3. Check [ARCHITECTURE.md](ARCHITECTURE.md) for data flow understanding
4. Follow [DEPLOYMENT.md](DEPLOYMENT.md) for test environment setup

### For API Consumers
1. Read [API_REFERENCE.md](API_REFERENCE.md) for endpoint documentation
2. Review examples for each API
3. Check error handling sections
4. Understand authentication requirements

## 📞 Support & Contact

- **Documentation Issues**: Check relevant guide first
- **Build Issues**: See [DEPLOYMENT.md](DEPLOYMENT.md) troubleshooting
- **Test Failures**: See [TESTING_GUIDE.md](TESTING_GUIDE.md) debugging
- **API Questions**: See [API_REFERENCE.md](API_REFERENCE.md)
- **Performance**: See [AOT_COMPILATION.md](AOT_COMPILATION.md)

---

**Last Updated:** 2026-08-26  
**Documentation Version:** 1.0  
**Project Version:** 1.0.0  
**License:** MIT
