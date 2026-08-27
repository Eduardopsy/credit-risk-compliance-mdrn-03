using CreditRisk.BureauMock.Service;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// POST /query - Simulate bureau API
app.MapPost("/query", async (QueryRequest request) =>
{
    // Simulate variable response times (50-200ms)
    await Task.Delay(Random.Shared.Next(50, 200));
    
    // Return score based on document (deterministic for testing)
    int score = Math.Abs(request.Document.GetHashCode()) % 1000; // 0-999
    
    var response = new QueryResponse(score, Random.Shared.Next(0, 50000), "Success");
    
    return Results.Ok(response);
})
.WithName("QueryBureau")
.Produces<QueryResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

// GET /health - Health check
app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
})
.WithName("Health")
.Produces(StatusCodes.Status200OK);

// POST /query/error/{statusCode} - Force errors for testing
app.MapPost("/query/error/{statusCode}", (int statusCode) =>
{
    return Results.StatusCode(statusCode);
})
.WithName("QueryError")
.Produces(StatusCodes.Status500InternalServerError);

app.Run("http://0.0.0.0:8081");
