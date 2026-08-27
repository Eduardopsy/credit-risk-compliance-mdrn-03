# AOT (Ahead-of-Time) Compilation Guide

## Overview

This project is designed to be AOT-compatible, enabling native compilation for improved startup time, reduced memory footprint, and improved security characteristics. All worker projects (CreditAnalysis.Worker, Compliance.Worker, Operations.Server) can be published with `PublishAot=true`.

## AOT Compilation Requirements

### 1. No Reflection-Based Consumer Discovery

**Issue:** MassTransit's reflection-based `AddConsumers()` with assembly scanning breaks AOT compilation.

**Solution:** Use explicit consumer registration:

```csharp
// ❌ NOT AOT-compatible
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
});

// ✅ AOT-compatible
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreditProposalCreatedEventConsumer>();
    x.AddConsumer<TransactionReceivedCommandConsumer>();
    // ... explicitly list all consumers
});
```

### 2. No Dynamic Proxy Libraries

**Issue:** Moq, NSubstitute, and Castle.DynamicProxy generate proxies at runtime.

**Solution:** Use hand-written fakes:

```csharp
// ❌ NOT AOT-compatible
var mockEngine = new Mock<ICreditScoringEngine>();
mockEngine.Setup(x => x.EvaluateAsync(It.IsAny<decimal>()))
    .ReturnsAsync(RiskRating.B);

// ✅ AOT-compatible
public sealed class FakeCreditScoringEngine : ICreditScoringEngine
{
    public Task<RiskRating> EvaluateAsync(decimal score)
        => Task.FromResult(RiskRating.B);
}
```

### 3. No Reflection-Based Deserialization

**Issue:** JsonSerializer with TypeInfo requires metadata generation.

**Solution:** Use source-generated serialization:

```csharp
// ❌ Requires reflection at runtime
var message = JsonSerializer.Deserialize<CreditProposalCreatedEvent>(json);

// ✅ AOT-compatible with JsonSerializerContext
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = CreditRiskSerializerContext.Default
};
var message = JsonSerializer.Deserialize<CreditProposalCreatedEvent>(json, options);

// Define context
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CreditProposalCreatedEvent))]
[JsonSerializable(typeof(CreditProposalEvaluatedEvent))]
[JsonSerializable(typeof(AmlAlertCreatedEvent))]
// ... register all message types
internal partial class CreditRiskSerializerContext : JsonSerializerContext
{
}
```

### 4. No Type.GetType() for Dynamic Loading

**Issue:** Loading types by string name at runtime.

**Solution:** Use a type registry:

```csharp
// ❌ NOT AOT-compatible
var messageType = Type.GetType(message.MessageType);

// ✅ AOT-compatible with type registry
public static class MessageTypeRegistry
{
    private static readonly Dictionary<string, Type> TypeMap = new()
    {
        ["CreditRisk.Shared.Contracts.CreditAnalysis.Events.CreditProposalCreatedEvent"] = typeof(CreditProposalCreatedEvent),
        ["CreditRisk.Shared.Contracts.CreditAnalysis.Events.CreditProposalEvaluatedEvent"] = typeof(CreditProposalEvaluatedEvent),
        ["CreditRisk.Shared.Contracts.Compliance.Events.AmlAlertCreatedEvent"] = typeof(AmlAlertCreatedEvent),
        // ... all message types
    };

    public static bool TryGetType(string fullName, out Type? type)
        => TypeMap.TryGetValue(fullName, out type);
}

// Usage
if (MessageTypeRegistry.TryGetType(message.MessageType, out var messageType))
{
    // Use messageType safely
}
```

### 5. Explicit Constructor Injection Only

**Issue:** Property injection requires reflection to set properties.

**Solution:** Use constructor injection only:

```csharp
// ❌ Property injection not AOT-compatible
public class MyService
{
    public ILogger<MyService> Logger { get; set; }
}

// ✅ Constructor injection
public sealed class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
}
```

## Publishing for AOT

### Step 1: Enable AOT in Project File

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <TrimMode>link</TrimMode>
  <IlcOptimizationPreference>Speed</IlcOptimizationPreference>
  <IlcGenerateStackTraceData>false</IlcGenerateStackTraceData>
</PropertyGroup>
```

### Step 2: Verify Trimming Configuration

Create `TrimmerRootAssembly.xml` if needed:

```xml
<?xml version="1.0" encoding="utf-8"?>
<linker>
  <!-- Preserve MassTransit consumer types -->
  <assembly fullname="CreditRisk.CreditAnalysis.Worker">
    <namespace fullname="CreditRisk.CreditAnalysis.Worker.Consumers" preserve="all"/>
  </assembly>

  <!-- Preserve infrastructure types -->
  <assembly fullname="CreditRisk.Shared.Kernel">
    <namespace fullname="CreditRisk.Shared.Kernel.Outbox" preserve="all"/>
  </assembly>
</linker>
```

### Step 3: Publish with AOT

```bash
# Credit Analysis Worker
cd src/workers/CreditRisk.CreditAnalysis.Worker
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
# Output: bin/Release/net8.0/linux-x64/publish/CreditRisk.CreditAnalysis.Worker

# Compliance Worker
cd src/workers/CreditRisk.Compliance.Worker
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
# Output: bin/Release/net8.0/linux-x64/publish/CreditRisk.Compliance.Worker

# Operations Server
cd src/servers/CreditRisk.Operations.Server
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
# Output: bin/Release/net8.0/linux-x64/publish/CreditRisk.Operations.Server
```

## Performance Benefits

### Startup Time
- **JIT Compilation:** ~2000-3000ms
- **AOT Compilation:** ~100-200ms (10-20x faster)

### Memory Footprint
- **JIT Compilation:** ~150-200MB
- **AOT Compilation:** ~80-100MB (50% reduction)

### Container Size
- **JIT-based image:** ~1GB (with .NET runtime)
- **AOT native executable:** ~50-80MB (statically linked)

## Docker Multistage Build

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -r linux-x64 -p:PublishAot=true

# Stage 2: Runtime (no .NET runtime needed!)
FROM ubuntu:22.04
RUN apt-get update && apt-get install -y libssl3 libgcc1
COPY --from=build /src/src/workers/CreditRisk.CreditAnalysis.Worker/bin/Release/net8.0/linux-x64/publish /app
WORKDIR /app
ENTRYPOINT ["./CreditRisk.CreditAnalysis.Worker"]
```

## AOT Warnings & Errors

### Warning: "Generating native code..."

This is expected and indicates AOT compilation is running. Takes 2-5 minutes depending on code size.

### Error: "IL cannot be compiled..."

Indicates reflection usage incompatible with AOT. Solutions:

1. Use source generation (Serilog logging, EF Core queries, JSON serialization)
2. Suppress with `[RequiresUnreferencedCode]` attribute (mark problematic code)
3. Use runtime fallback for non-hot-path code

### Error: "Module initializer..."

Ensure all static constructors are AOT-friendly (no reflection).

## Verification Checklist

- [ ] All consumers explicitly registered (no assembly scanning)
- [ ] No Moq/NSubstitute/dynamic proxies
- [ ] All JSON serialization uses source-generated context
- [ ] No Type.GetType() for runtime type loading
- [ ] All dependencies injected via constructor
- [ ] PublishAot=true enabled
- [ ] Successful dotnet publish with -p:PublishAot=true
- [ ] Native executable runs without errors
- [ ] Startup time < 500ms
- [ ] Memory footprint < 150MB

## Testing AOT Compilation

```bash
# Verify native binary works
./bin/Release/net8.0/linux-x64/publish/CreditRisk.CreditAnalysis.Worker

# Measure startup time
time ./bin/Release/net8.0/linux-x64/publish/CreditRisk.CreditAnalysis.Worker

# Check memory usage
ps aux | grep CreditRisk.CreditAnalysis.Worker

# Test with Testcontainers
dotnet test --filter "AOT" -c Release
```

## Future: Full Source Generation

As .NET evolves, more framework components support source generation:

- Entity Framework Core 8+ with compiled queries
- Dependency injection with source-generated factories
- Configuration with source-generated binders
- Serialization with multi-format source generators

Target: 100% deterministic compilation, zero reflection.

## Resources

- [AOT Deployment | Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [.NET Trimming | Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)
- [MassTransit AOT Support](https://masstransit.io/documentation)
- [Native AOT Deployment of ASP.NET Core Apps](https://learn.microsoft.com/en-us/aspnet/core/deploying/native-aot)
