using GymApp.Infrastructure.HealthChecks;
using GymApp.Infrastructure.Security;
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello BetterChoices!");

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
app.SetupHealthChecks();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
app.Run();