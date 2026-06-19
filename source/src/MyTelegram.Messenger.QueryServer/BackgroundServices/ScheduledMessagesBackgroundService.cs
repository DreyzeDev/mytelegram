using Microsoft.Extensions.Hosting;
using MyTelegram.Domain.Aggregates.Temp;

namespace MyTelegram.Messenger.QueryServer.BackgroundServices;

public class ScheduledMessagesBackgroundService(
    IQueryProcessor queryProcessor,
    ICommandBus commandBus,
    ILogger<ScheduledMessagesBackgroundService> logger) : BackgroundService
{
    private const int IntervalSeconds = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Scheduled messages background service started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
                await ProcessScheduledMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in scheduled messages background service");
            }
        }
    }

    private async Task ProcessScheduledMessagesAsync(CancellationToken cancellationToken)
    {
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dueMessages = await queryProcessor.ProcessAsync(
            new GetScheduleMessagesByDateQuery(now), cancellationToken);

        if (dueMessages.Count == 0)
            return;

        logger.LogInformation("Processing {Count} scheduled messages", dueMessages.Count);

        foreach (var scheduleItem in dueMessages)
        {
            try
            {
                await SendScheduledMessageAsync(scheduleItem, now, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send scheduled message {MessageId} for user {UserId}",
                    scheduleItem.MessageId, scheduleItem.UserId);
            }
        }
    }

    private async Task SendScheduledMessageAsync(ScheduleItem scheduleItem, int now, CancellationToken cancellationToken)
    {
        var msgReadModel = await queryProcessor.ProcessAsync(
            new GetMessageByPeerIdAndMessageIdQuery(scheduleItem.UserId, scheduleItem.MessageId),
            cancellationToken);

        if (msgReadModel == null)
            return;

        var itemsToDelete = await queryProcessor.ProcessAsync(
            new GetMessageItemListToBeDeletedQuery(scheduleItem.UserId, [scheduleItem.MessageId], Revoke: true),
            cancellationToken);

        var ownerPeerType = scheduleItem.ToPeer.PeerType == PeerType.Channel
            ? PeerType.Channel
            : PeerType.User;

        var messageItem = new MessageItem(
            OwnerPeer: new Peer(ownerPeerType, scheduleItem.UserId),
            ToPeer: scheduleItem.ToPeer,
            SenderPeer: new Peer(PeerType.User, msgReadModel.SenderPeerId),
            SenderUserId: msgReadModel.SenderUserId,
            MessageId: 0,
            Message: msgReadModel.Message ?? string.Empty,
            Date: now,
            RandomId: Random.Shared.NextInt64(),
            IsOut: true,
            SendMessageType: msgReadModel.SendMessageType,
            MessageType: msgReadModel.MessageType,
            Entities: msgReadModel.Entities2,
            Media: msgReadModel.Media2,
            GroupId: scheduleItem.GroupId,
            ReplyMarkup: msgReadModel.ReplyMarkup2,
            Post: msgReadModel.Post,
            PostAuthor: msgReadModel.PostAuthor,
            InvertMedia: msgReadModel.InvertMedia,
            Effect: msgReadModel.Effect,
            ScheduleDate: null
        );

        var requestInfo = new RequestInfo(
            ConnectionId: string.Empty,
            SessionId: 0,
            ReqMsgId: 0,
            UserId: scheduleItem.UserId,
            AccessHashKeyId: 0,
            AuthKeyId: 0,
            PermAuthKeyId: 0,
            RequestId: Guid.NewGuid(),
            Layer: Layers.LayerLatest,
            Date: now
        );

        await commandBus.PublishAsync(
            new StartSendMessageCommand(TempId.New, requestInfo, [new SendMessageItem(messageItem)]),
            cancellationToken);

        if (itemsToDelete.Count > 0)
        {
            await commandBus.PublishAsync(
                new StartDeleteMessagesCommand(
                    TempId.New,
                    requestInfo,
                    itemsToDelete,
                    revoke: true,
                    deleteGroupMessagesForEveryone: false,
                    newTopMessageId: null,
                    newTopMessageIdForOtherParticipant: null),
                cancellationToken);
        }
    }
}
