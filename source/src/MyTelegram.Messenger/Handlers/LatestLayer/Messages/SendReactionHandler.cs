namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// React to message.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.sendReaction"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class SendReactionHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IPeerHelper peerHelper,
    IAccessHashHelper accessHashHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSendReaction, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(
        IRequestInput input,
        MyTelegram.Schema.Messages.RequestSendReaction obj)
    {
        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);

        var toPeer = peerHelper.GetPeer(obj.Peer, input.UserId);
        var senderPeer = input.UserId.ToUserPeer();

        var reactions = new List<Reaction>();
        if (obj.Reaction != null)
        {
            foreach (var r in obj.Reaction)
            {
                switch (r)
                {
                    case TReactionEmoji emoji:
                        reactions.Add(new Reaction(input.UserId, emoji.Emoticon, null, DateTime.UtcNow.ToTimestamp()));
                        break;
                    case TReactionCustomEmoji custom:
                        reactions.Add(new Reaction(input.UserId, null, custom.DocumentId, DateTime.UtcNow.ToTimestamp()));
                        break;
                }
            }
        }

        var messageId = obj.MsgId;

        if (toPeer.PeerType == PeerType.User)
        {
            var outboxMessageId = MessageId.Create(input.UserId, messageId);
            var outboxCommand = new SendReactionCommand(
                outboxMessageId,
                input.ToRequestInfo(),
                input.UserId,
                messageId,
                senderPeer,
                reactions,
                toPeer,
                obj.Big,
                obj.AddToRecent);
            await commandBus.PublishAsync(outboxCommand);

            var msgReadModel = await queryProcessor.ProcessAsync(
                new GetMessageByIdQuery(MessageId.Create(input.UserId, messageId).Value));
            if (msgReadModel != null)
            {
                var inboxOwnerPeerId = toPeer.PeerId;
                var inboxMessageId = msgReadModel.SenderMessageId;
                var inboxId = MessageId.Create(inboxOwnerPeerId, inboxMessageId);
                var inboxCommand = new SendReactionCommand(
                    inboxId,
                    input.ToRequestInfo(),
                    inboxOwnerPeerId,
                    inboxMessageId,
                    senderPeer,
                    reactions,
                    input.UserId.ToUserPeer(),
                    obj.Big,
                    obj.AddToRecent);
                await commandBus.PublishAsync(inboxCommand);
            }
        }
        else
        {
            var channelMessageId = MessageId.Create(toPeer.PeerId, messageId);
            var command = new SendReactionCommand(
                channelMessageId,
                input.ToRequestInfo(),
                toPeer.PeerId,
                messageId,
                senderPeer,
                reactions,
                toPeer,
                obj.Big,
                obj.AddToRecent);
            await commandBus.PublishAsync(command);
        }

        return null!;
    }
}
