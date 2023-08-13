using Crypton.Application.Interfaces;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI;

/// <summary>
/// Configure the middleware for the web application.
/// </summary>
public static class ConfigureMiddleware
{
    /// <summary>
    /// Apply any currently pending migrations.
    /// </summary>
    /// <param name="app">WebApplication.</param>
    /// <returns>the WebApplication with the applied migrations.</returns>
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

    /// <summary>
    /// Initialize the transactions. Load them from the database.
    /// </summary>
    /// <param name="app">WebApplication.</param>
    /// <returns>the WebApplication with the initialized transactions.</returns>
    public static WebApplication InitializeTransactions(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var transactions = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        transactions.InitializeAsync()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        return app;
    }

    /// <summary>
    /// Configure the web api middleware.
    /// </summary>
    /// <param name="app">WebApplication.</param>
    /// <returns>the WebApplication with the configured web api middleware.</returns>
    public static WebApplication ConfigureWebApiMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseResponseCaching();
        app.UseResponseCompression();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthentication();

        app.UseRouting();

        app.UseRateLimiter();

        // this should be after UseRouting
        // https://stackoverflow.com/a/71951181/14860947
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