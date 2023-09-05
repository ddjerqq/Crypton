using Crypton.Application.Common;
using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

namespace Crypton.Infrastructure.BackgroundJobs;

// source: https://youtu.be/XALvnX7MPeo?si=dA5Sq9g1FHa-0SwM
[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesBackgroundJob : IJob
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy(),
        },
    };

    private readonly IPublisher _publisher;
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<ProcessOutboxMessagesBackgroundJob> _logger;

    public ProcessOutboxMessagesBackgroundJob(
        IPublisher publisher,
        IAppDbContext dbContext,
        ILogger<ProcessOutboxMessagesBackgroundJob> logger)
    {
        _publisher = publisher;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // 20 oldest messages
        var messages = await _dbContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccuredOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            IDomainEvent? domainEvent = JsonConvert
                .DeserializeObject<IDomainEvent>(message.Content, JsonSerializerSettings);

            if (domainEvent is null)
            {
                _logger.LogWarning("Failed to deserialize message {MessageId}", message.Id);
                continue;
            }

            try
            {
                await _publisher.Publish(domainEvent, context.CancellationToken);
            }
            catch (Exception ex)
            {
                message.Error = ex.ToString();
                _logger.LogError(ex, "Failed to publish message {MessageId}", message.Id);
            }

            message.ProcessedOnUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}