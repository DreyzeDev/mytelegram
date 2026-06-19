using Microsoft.Extensions.Hosting;
using MyTelegram.Domain.Aggregates.Temp;

namespace MyTelegram.Messenger.QueryServer.BackgroundServices;

public class AutoDeleteMessagesBackgroundService(
    IQueryProcessor queryProcessor,
    ICommandBus commandBus,
    ILogger<AutoDeleteMessagesBackgroundService> logger) : BackgroundService
{
    private const int PageSize = 100;
    private const int IntervalSeconds = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Auto-delete messages background service started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
                await ProcessExpiredMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in auto-delete messages background service");
            }
        }
    }

    private async Task ProcessExpiredMessagesAsync(CancellationToken cancellationToken)
    {
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var skip = 0;

        while (true)
        {
            var expiredMessages = await queryProcessor.ProcessAsync(
                new GetAutoDeleteMessagesQuery(now, skip, PageSize), cancellationToken);

            if (expiredMessages.Count == 0)
                break;

            var messageItems = expiredMessages
                .Select(m => new MessageItemToBeDeleted(m.OwnerPeerId, m.MessageId, m.ToPeerType, m.ToPeerId))
                .ToList();

            await commandBus.PublishAsync(
                new StartDeleteMessagesCommand(
                    TempId.New,
                    RequestInfo.Empty,
                    messageItems,
                    revoke: false,
                    deleteGroupMessagesForEveryone: false,
                    newTopMessageId: null,
                    newTopMessageIdForOtherParticipant: null),
                cancellationToken);

            logger.LogInformation("Auto-deleted {Count} expired messages", expiredMessages.Count);

            if (expiredMessages.Count < PageSize)
                break;

            skip += PageSize;
        }
    }
}
