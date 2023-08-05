// <copyright file="ConfigureMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Infrastructure.Diamond;
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

        // this should be after UseRouting
        // https://stackoverflow.com/a/71951181/14860947
        app.UseDigitalSignature();

        app.UseAuthorization();

        app.UseCors();

        app.MapHealthChecks("/health");

        app.UseEndpoints(x => { x.MapControllers(); });

        return app;
    }
}