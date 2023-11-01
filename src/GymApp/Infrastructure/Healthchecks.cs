using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace GymApp.Infrastructure
{
    public static class Healthchecks
    {
        public static void MapHealthChecks(this WebApplication app)
        {
            app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("ready")
            });

            app.MapHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            app.MapHealthChecks("/healthz/startup", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("startup")
            });
        }
    }
}
