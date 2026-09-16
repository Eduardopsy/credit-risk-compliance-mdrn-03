using CreditRisk.BureauMock.Service;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081);
});

var app = builder.Build();

long totalQueries = 0;
long successfulQueries = 0;

// POST /query - Simulate bureau API
app.MapPost("/query", async (QueryRequest request) =>
{
    Interlocked.Increment(ref totalQueries);

    // Simulate variable response times (50-200ms)
    await Task.Delay(Random.Shared.Next(50, 200));
    
    // Return score based on document (deterministic for testing)
    int score = Math.Abs(request.Document.GetHashCode()) % 1000; // 0-999
    
    var response = new QueryResponse(score, Random.Shared.Next(0, 50000), "Success");
    Interlocked.Increment(ref successfulQueries);
    
    return Results.Ok(response);
})
.WithName("QueryBureau")
.Produces<QueryResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

// GET /health - Health check
app.MapGet("/health", () =>
{
    return Results.Ok(new BureauHealthResponse("healthy", DateTimeOffset.UtcNow));
})
.WithName("Health")
.Produces<BureauHealthResponse>(StatusCodes.Status200OK);

// GET /statistics - Bureau usage statistics
app.MapGet("/statistics", () =>
{
    long total = Interlocked.Read(ref totalQueries);
    long success = Interlocked.Read(ref successfulQueries);
    double rate = total == 0 ? 100.0 : Math.Round((double)success / total * 100.0, 2);

    return Results.Ok(new BureauStatisticsResponse(total, rate, DateTimeOffset.UtcNow));
})
.WithName("Statistics")
.Produces<BureauStatisticsResponse>(StatusCodes.Status200OK);

// POST /query/error/{statusCode} - Force errors for testing
app.MapPost("/query/error/{statusCode}", (int statusCode) =>
{
    return Results.StatusCode(statusCode);
})
.WithName("QueryError")
.Produces(StatusCodes.Status500InternalServerError);

app.Run();
