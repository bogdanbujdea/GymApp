using GymApp.Infrastructure.HealthChecks;
using GymApp.Infrastructure.Security;
using GymApp.Infrastructure.Storage;
using GymApp.Infrastructure.Swagger;
using GymApp.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddCheck<StartupHealthCheck>("Startup", tags: new[] { "startup" })
    .AddCheck<ReadyHealthCheck>("Ready", tags: new[] { "ready" });
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration["SQL_CONNECTION_STRING"];
    options.UseSqlServer(connectionString);
});

builder.SetupSwagger();
builder.SetupAuthenticationWithAuth0();
GremlinWrapper.Initialize(builder.Configuration["GREMLIN_SERVER_PASSWORD"]!);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello gymapp!");

app.MapGet("/users", async (AppDbContext dbContext) =>
{
    Console.WriteLine($"Retrieving users from db");
    return await dbContext.Users.ToListAsync();
}).RequireAuthorization("exerciseapp:read-write");

app.MapPost("/users", async (AppUser user, AppDbContext dbContext) =>
{
    Console.WriteLine($"Adding user in SQL db");

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    return user;
});

app.MapPost("/exerciseRoutine", async (string exerciseRoutineName, string? gymName) =>
{
    try
    {
        var exerciseRoutineResult = await GremlinWrapper.CreateEntity("exerciseRoutine", exerciseRoutineName);
        if (gymName == null) return exerciseRoutineResult;
        var gymResult = await GremlinWrapper.CreateEntity("gym", gymName);
        await GremlinWrapper.LinkEntities(exerciseRoutineName, gymName, "availableAt");
        return gymResult;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }
});

app.MapPost("/gyms", async (string gymName) =>
{
    try
    {
        var result = await GremlinWrapper.CreateEntity("gym", gymName);
        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }
});

app.MapPost("/exercise", async (string exerciseName, string? exerciseRoutineName, int? reps, string? gymName) =>
{
    Console.WriteLine($"Adding exercise");

    try
    {
        var result = await GremlinWrapper.CreateEntity("exercise", exerciseName);

        if (string.IsNullOrWhiteSpace(exerciseRoutineName) == false && reps != null)
        {
            if (await GremlinWrapper.VertexExistsAsync(exerciseRoutineName) == false)
            {
                await GremlinWrapper.CreateEntity("exerciseRoutine", exerciseRoutineName);
            }
            await GremlinWrapper.LinkEntities(exerciseRoutineName, exerciseName, "includes");
            if (gymName != null)
            {
                if (await GremlinWrapper.VertexExistsAsync(gymName) == false)
                {
                    await GremlinWrapper.CreateEntity("gym", gymName);
                }
                await GremlinWrapper.LinkEntities(exerciseRoutineName, gymName, "availableAt");
            }
        }

        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }

});


app.SetupHealthChecks();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
app.Run();