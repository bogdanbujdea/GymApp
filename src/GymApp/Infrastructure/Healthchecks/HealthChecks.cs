using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace GymApp.Infrastructure.HealthChecks
{
    public static class HealthChecks
    {
        public static void SetupHealthChecks(this WebApplication app)
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
