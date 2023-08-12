using Crypton.Application.Transactions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Crypton.Infrastructure.ModelBinders;

public sealed class ModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(CreateTransactionCommand))
        {
            return new CreateTransactionCommandModelBinder();
        }

        return null;
    }
}