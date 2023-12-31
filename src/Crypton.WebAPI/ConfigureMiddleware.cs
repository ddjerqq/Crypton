﻿using Crypton.Infrastructure.Persistence;
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
        app.UseHttpLogging();

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

        app.UseHttpsRedirection();

        if (!app.Environment.IsDevelopment())
        {
            app.UseResponseCompression();
        }

        app.UseResponseCaching();

        // for front-end
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseAuthentication();

        app.UseRouting();

        app.UseRateLimiter();

        // TODO: re-enable for production
        // app.UseDigitalSignature();

        // before authorization
        app.UseCors();

        app.UseAuthorization();

        // TODO: re-enable for production
        // app.UseIdempotency();

        app.MapHealthChecks("/health");

        // for front-end
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }
}