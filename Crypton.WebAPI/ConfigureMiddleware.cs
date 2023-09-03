using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI;

public static class ConfigureMiddleware
{
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Database.EnsureCreated();

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }

        return app;
    }

    public static WebApplication ConfigureWebApiMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler("/error");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        app.UseResponseCaching();
        app.UseResponseCompression();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthentication();

        app.UseRouting();

        app.UseRateLimiter();

        // TODO: re-enable for front-end
        // app.UseDigitalSignature();

        app.UseAuthorization();

        app.UseIdempotency();
        app.UseCors();

        app.MapHealthChecks("/health");

        app.UseEndpoints(x => { x.MapControllers(); });

        return app;
    }
}