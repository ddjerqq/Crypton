﻿using System.Text.Json.Serialization;
using Crypton.Application;
using Crypton.Domain;
using Crypton.Infrastructure.Diamond;
using Crypton.WebAPI.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.ResponseCompression;
using ZymLabs.NSwag.FluentValidation;

namespace Crypton.WebAPI;

public static class ConfigureServices
{
    private static readonly string[] CompressionTypes = { "application/octet-stream" };

    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddHttpContextAccessor();

        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.RequestPath
                                    | HttpLoggingFields.ResponseStatusCode
                                    | HttpLoggingFields.RequestMethod
                                    | HttpLoggingFields.RequestQuery
                                    | HttpLoggingFields.RequestHeaders
                                    | HttpLoggingFields.ResponseHeaders;

            logging.RequestHeaders.Add("X-Idempotency-Key");

            // digital signature
            logging.RequestHeaders.Add(RuleConstants.UserIdHeaderName);
            logging.RequestHeaders.Add(RuleConstants.SignatureHeaderName);
            logging.RequestHeaders.Add(RuleConstants.TimestampHeaderName);
            logging.RequestHeaders.Add(RuleConstants.AppTokenHeaderName);

            logging.ResponseHeaders.Add("X-Client-IP");
            logging.MediaTypeOptions.AddText("application/javascript");

            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
        });

        services.Configure<RouteOptions>(x =>
        {
            x.LowercaseUrls = true;
            x.LowercaseQueryStrings = true;
            x.AppendTrailingSlash = false;
        });

        services
            .AddControllers(o =>
            {
                o.Filters.Add<ClientIpAddressFilter>();
                o.Filters.Add<FluentValidationFilter>();
                o.Filters.Add<ResponseTimeFilter>();
                o.Filters.Add<ErrorHandlingFilterAttribute>();
                o.RespectBrowserAcceptHeader = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy();
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // suppress swagger from showing annoying client error data
                // source: https://stackoverflow.com/a/71965274/14860947
                options.SuppressMapClientErrors = true;
            });

        services.AddSignalR(o => { o.EnableDetailedErrors = env.IsDevelopment(); });

        services.AddValidatorsFromAssembly(DomainAssembly.Assembly);
        services.AddValidatorsFromAssembly(ApplicationAssembly.Assembly);

        services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters();

        services.AddScoped(provider =>
        {
            var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
            var loggerFactory = provider.GetService<ILoggerFactory>();

            return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
        });

        if (env.IsDevelopment())
        {
            services.AddSwagger(env);
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                // change these to the front-end app domain later.
                // so that we tell the browser on that domain, that
                // it is okay to send the API requests.
                policy.WithOrigins("http://localhost:5000", "https://localhost:5001");
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });
        });

        services.AddResponseCaching();
        services.AddResponseCompression(o =>
        {
            o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(CompressionTypes);

            o.Providers.Add<GzipCompressionProvider>();
            o.Providers.Add<BrotliCompressionProvider>();
        });
        return services;
    }
}