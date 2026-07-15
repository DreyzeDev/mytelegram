namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Mark mentions as read
/// Possible errors
/// Code Type Description
/// 400 CHANNEL_INVALID The provided channel is invalid.
/// 400 CHANNEL_PRIVATE You haven't joined this channel/supergroup.
/// 400 MSG_ID_INVALID Invalid message ID provided.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 SAVED_PEER_INVALID  
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.readMentions"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ReadMentionsHandler(ICommandBus commandBus, IMessageAppService messageAppService, IPeerHelper peerHelper, IAccessHashHelper accessHashHelper, IPtsHelper ptsHelper) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestReadMentions, MyTelegram.Schema.Messages.IAffectedHistory>
{
    protected override async Task<IAffectedHistory> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestReadMentions obj)
    {
        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);
        var userId = input.UserId;
        var peer = peerHelper.GetPeer(obj.Peer, userId);
        var ownerPeerId = peer.PeerType == PeerType.Channel ? peer.PeerId : userId;

        // There's no dedicated per-message "unread mention" flag, so find messages in this peer that
        // mention the current user and mark each one read against DialogAggregate's UnreadMentionsCount
        // (the same aggregate/event pair used to compute messages.getUnreadMentions' backing count).
        var r = await messageAppService.GetHistoryAsync(new GetHistoryInput
        {
            OwnerPeerId = ownerPeerId,
            SelfUserId = userId,
            Limit = 100,
            Peer = peer
        });

        var mentionedMessageIds = r.MessageList
            .Where(p => p.MentionedUserIds?.Contains(userId) ?? false)
            .Select(p => p.MessageId)
            .ToList();

        var dialogId = DialogId.Create(userId, peer);
        foreach (var messageId in mentionedMessageIds)
        {
            var command = new ReadMentionCommand(dialogId, messageId);
            await commandBus.PublishAsync(command);
        }

        var cachedPts = ptsHelper.GetCachedPts(userId);
        return new TAffectedHistory
        {
            Offset = 0,
            Pts = cachedPts,
            PtsCount = 0
        };
    }
}