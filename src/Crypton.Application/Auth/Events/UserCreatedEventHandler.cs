﻿using Crypton.Domain.Events;
using MediatR;

namespace Crypton.Application.Auth.Events;

internal sealed class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        throw new NotImplementedException("Email sending is not implemented.");
    }
}