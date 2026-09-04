# GitHub Actions Workflow for Postman Tests
# Place this file at: .github/workflows/postman-tests.yml

name: Postman API Tests

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'postman/**'
      - 'src/**'
      - '.github/workflows/postman-tests.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'postman/**'
      - 'src/**'
  schedule:
    # Run tests daily at 2 AM UTC
    - cron: '0 2 * * *'
  workflow_dispatch:

jobs:
  postman-tests:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: creditrisk
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
      
      redis:
        image: redis:7-alpine
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 6379:6379
      
      rabbitmq:
        image: rabbitmq:3.12-management
        env:
          RABBITMQ_DEFAULT_USER: guest
          RABBITMQ_DEFAULT_PASS: guest
        options: >-
          --health-cmd "rabbitmq-diagnostics -q ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5672:5672
          - 15672:15672

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0'

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '18'

      - name: Install Newman
        run: npm install -g newman

      - name: Restore dependencies
        run: dotnet restore

      - name: Build solution
        run: dotnet build -c Release --no-restore

      - name: Start services in background
        run: |
          # Bureau Mock
          dotnet run -p src/external/CreditRisk.BureauMock.Service -c Release &
          sleep 5
          
          # IAM
          dotnet run -p src/modules/iam/CreditRisk.IAM.Api -c Release &
          sleep 5
          
          # Credit Analysis
          dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api -c Release &
          sleep 5
          
          # Compliance
          dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api -c Release &
          sleep 5
          
          # Operations
          dotnet run -p src/servers/CreditRisk.Operations.Server -c Release &
          sleep 5

      - name: Wait for services to be ready
        run: |
          for i in {1..30}; do
            curl -s http://localhost:8081/health && \
            curl -s http://localhost:5000/health && \
            curl -s http://localhost:5001/health && \
            curl -s http://localhost:5002/health && \
            curl -s http://localhost:5003/health && \
            echo "All services healthy" && break || \
            (echo "Waiting for services... ($i/30)" && sleep 2)
          done

      - name: Run Postman tests with CLI output
        run: |
          cd postman
          newman run Credit-Risk-Compliance-Collection.postman_collection.json \
            -e Credit-Risk-Compliance-Environment.postman_environment.json \
            --bail collection

      - name: Generate HTML Report
        if: always()
        run: |
          cd postman
          newman run Credit-Risk-Compliance-Collection.postman_collection.json \
            -e Credit-Risk-Compliance-Environment.postman_environment.json \
            -r html --reporter-html-export report.html \
            || true

      - name: Upload test report
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: postman-test-report
          path: postman/report.html
          retention-days: 30

      - name: Post results to PR
        if: github.event_name == 'pull_request' && always()
        uses: actions/github-script@v7
        with:
          script: |
            const fs = require('fs');
            const path = require('path');
            
            const reportPath = path.join(process.env.GITHUB_WORKSPACE, 'postman', 'report.html');
            if (fs.existsSync(reportPath)) {
              const comment = `## 📊 Postman API Test Results\n\n[View detailed report](../../actions/runs/${{ github.run_id }})`;
              github.rest.issues.createComment({
                issue_number: context.issue.number,
                owner: context.repo.owner,
                repo: context.repo.repo,
                body: comment
              });
            }

      - name: Cleanup services
        if: always()
        run: pkill -f "dotnet run" || true

---

# Example: Azure Pipelines (azure-pipelines.yml)

trigger:
  - main
  - develop

pr:
  - main
  - develop

schedules:
  - cron: "0 2 * * *"
    displayName: Daily tests at 2 AM UTC
    branches:
      include:
        - main
        - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dotnetVersion: '8.0'

services:
  postgres:
    image: postgres:15
    env:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: creditrisk
  
  redis:
    image: redis:7-alpine
  
  rabbitmq:
    image: rabbitmq:3.12-management

stages:
  - stage: Build
    jobs:
      - job: BuildAndTest
        displayName: Build and Run Postman Tests
        steps:
          - task: UseDotNet@2
            inputs:
              version: $(dotnetVersion)

          - task: UseNode@1
            inputs:
              version: '18.x'

          - script: npm install -g newman
            displayName: 'Install Newman'

          - task: DotNetCoreCLI@2
            inputs:
              command: 'restore'
            displayName: 'Restore dependencies'

          - task: DotNetCoreCLI@2
            inputs:
              command: 'build'
              arguments: '-c $(buildConfiguration)'
            displayName: 'Build solution'

          - script: |
              # Start services in background
              dotnet run -p src/external/CreditRisk.BureauMock.Service -c $(buildConfiguration) &
              sleep 5
              
              dotnet run -p src/modules/iam/CreditRisk.IAM.Api -c $(buildConfiguration) &
              sleep 5
              
              dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api -c $(buildConfiguration) &
              sleep 5
              
              dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api -c $(buildConfiguration) &
              sleep 5
              
              dotnet run -p src/servers/CreditRisk.Operations.Server -c $(buildConfiguration) &
              sleep 5
              
              # Wait for services
              for i in {1..30}; do
                curl -s http://localhost:8081/health && \
                curl -s http://localhost:5000/health && \
                curl -s http://localhost:5001/health && \
                curl -s http://localhost:5002/health && \
                curl -s http://localhost:5003/health && \
                echo "All services healthy" && break || \
                (echo "Waiting for services... ($i/30)" && sleep 2)
              done
            displayName: 'Start services'

          - script: |
              cd postman
              newman run Credit-Risk-Compliance-Collection.postman_collection.json \
                -e Credit-Risk-Compliance-Environment.postman_environment.json \
                -r html --reporter-html-export report.html
            displayName: 'Run Postman tests'

          - task: PublishBuildArtifacts@1
            condition: always()
            inputs:
              PathtoPublish: 'postman/report.html'
              ArtifactName: 'postman-report'
            displayName: 'Publish test report'

          - script: pkill -f "dotnet run" || true
            condition: always()
            displayName: 'Cleanup services'

---

# Example: GitLab CI (.gitlab-ci.yml)

stages:
  - build
  - test

variables:
  DOTNET_VERSION: "8.0"
  NODE_VERSION: "18"

build:
  stage: build
  image: mcr.microsoft.com/dotnet:8.0-sdk-jammy
  script:
    - dotnet restore
    - dotnet build -c Release
  artifacts:
    paths:
      - "**/*.dll"
      - "**/*.exe"

postman_tests:
  stage: test
  image: mcr.microsoft.com/dotnet:8.0-sdk-jammy
  services:
    - postgres:15
    - redis:7-alpine
    - rabbitmq:3.12-management
  variables:
    POSTGRES_PASSWORD: "postgres"
    POSTGRES_DB: "creditrisk"
  before_script:
    - apt-get update && apt-get install -y curl
    - curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
    - apt-get install -y nodejs
    - npm install -g newman
  script:
    # Start services
    - dotnet run -p src/external/CreditRisk.BureauMock.Service -c Release &
    - sleep 5
    - dotnet run -p src/modules/iam/CreditRisk.IAM.Api -c Release &
    - sleep 5
    - dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api -c Release &
    - sleep 5
    - dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api -c Release &
    - sleep 5
    - dotnet run -p src/servers/CreditRisk.Operations.Server -c Release &
    - sleep 5
    
    # Wait for services
    - |
      for i in {1..30}; do
        curl -s http://localhost:8081/health && \
        curl -s http://localhost:5000/health && \
        curl -s http://localhost:5001/health && \
        curl -s http://localhost:5002/health && \
        curl -s http://localhost:5003/health && \
        echo "All services healthy" && break || \
        (echo "Waiting for services... ($i/30)" && sleep 2)
      done
    
    # Run tests
    - cd postman
    - newman run Credit-Risk-Compliance-Collection.postman_collection.json \
        -e Credit-Risk-Compliance-Environment.postman_environment.json \
        -r html --reporter-html-export report.html \
        -r json --reporter-json-export report.json
  artifacts:
    paths:
      - postman/report.html
      - postman/report.json
    reports:
      junit: postman/report.json
    when: always
  after_script:
    - pkill -f "dotnet run" || true
