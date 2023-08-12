using System.Text.Json;
using System.Text.Json.Serialization;
using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using Crypton.Infrastructure.Policies;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure.ModelBinders;

public class CreateTransactionCommandModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        CreateTransactionCommand? command;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            };
            options.Converters.Add(new JsonStringEnumConverter());

            command = await JsonSerializer.DeserializeAsync<CreateTransactionCommand>(
                bindingContext.HttpContext.Request.Body,
                options);
        }
        catch (JsonException)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        if (command is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        using var scope = bindingContext.HttpContext
            .RequestServices.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var currentUserAccessor = scope.ServiceProvider.GetRequiredService<ICurrentUserAccessor>();

        var success = await command.InitializeAsync(dbContext, currentUserAccessor);
        if (!success)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        bindingContext.Result = ModelBindingResult.Success(command);
        bindingContext.Model = command;
    }
}