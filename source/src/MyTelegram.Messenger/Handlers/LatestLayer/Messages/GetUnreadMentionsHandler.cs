namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Get unread messages where we were mentioned
/// Possible errors
/// Code Type Description
/// 400 CHANNEL_INVALID The provided channel is invalid.
/// 400 CHANNEL_PRIVATE You haven't joined this channel/supergroup.
/// 400 MSG_ID_INVALID Invalid message ID provided.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getUnreadMentions"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetUnreadMentionsHandler(IMessageAppService messageAppService, IQueryProcessor queryProcessor, IPeerHelper peerHelper, IAccessHashHelper accessHashHelper, IChannelAppService channelAppService, IGetHistoryConverterService getHistoryConverterService) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetUnreadMentions, MyTelegram.Schema.Messages.IMessages>
{
    protected override async Task<MyTelegram.Schema.Messages.IMessages> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetUnreadMentions obj)
    {
        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);
        var userId = input.UserId;
        var peer = peerHelper.GetPeer(obj.Peer, userId);
        var ownerPeerId = peer.PeerType == PeerType.Channel ? peer.PeerId : userId;
        if (peer.PeerType == PeerType.Channel)
        {
            var channelMember = await queryProcessor.ProcessAsync(new GetChannelMemberByUserIdQuery(peer.PeerId, input.UserId));
            if (channelMember?.Kicked == true)
            {
                return new TMessages { Chats = [], Messages = [], Users = [], Topics = [] };
            }

            var channelReadModel = await channelAppService.GetAsync(peer.PeerId);
            if (await channelAppService.SendRpcErrorIfNotChannelMemberAsync(input, channelReadModel!))
            {
                return null!;
            }
        }

        var dialogReadModel = await queryProcessor.ProcessAsync(new GetDialogByIdQuery(DialogId.Create(input.UserId, peer).Value));
        var channelHistoryMinId = dialogReadModel?.ChannelHistoryMinId ?? 0;

        var r = await messageAppService.GetHistoryAsync(new GetHistoryInput
        {
            OwnerPeerId = ownerPeerId,
            SelfUserId = userId,
            AddOffset = obj.AddOffset,
            Limit = obj.Limit,
            MaxId = obj.MaxId,
            MinId = obj.MinId,
            OffsetId = obj.OffsetId,
            Peer = peer,
            ChannelHistoryMinId = channelHistoryMinId
        });

        // Note: this filters for messages that mention the current user, but doesn't track a separate
        // "read" state per mention (DialogAggregate's UnreadMentionsCount / MentionRead events are used
        // by ReadMentionsHandler to mark mentions read, but there is no per-message "is this mention still
        // unread" flag to intersect with here), so the result is an approximation of "unread" mentions -
        // it returns mentioning messages for the peer rather than a strictly-unread subset.
        r.MessageList = r.MessageList.Where(p => p.MentionedUserIds?.Contains(userId) ?? false).ToList();

        return getHistoryConverterService.ToMessages(input, r, input.Layer);
    }
}