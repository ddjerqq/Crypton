using Crypton.Application.Interfaces;
using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Crypton.Infrastructure.Persistence.Interceptors;

public class UserMaterializationInterceptor : IMaterializationInterceptor
{
    private readonly ITransactionService transaction;

    public UserMaterializationInterceptor(ITransactionService transaction)
    {
        this.transaction = transaction;
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is User user)
        {
            user.SetBalance(this.transaction.GetUserBalance(user.Id));
            user.SetItems(this.transaction.GetUserItems(user.Id));
        }

        return instance;
    }
}