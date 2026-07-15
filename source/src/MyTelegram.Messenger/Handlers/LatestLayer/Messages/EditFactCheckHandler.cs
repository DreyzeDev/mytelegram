namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Edit/create a <a href="https://corefork.telegram.org/api/factcheck">fact-check</a> on a message.Can only be used by independent fact-checkers as specified by the <a href="https://corefork.telegram.org/api/config#can-edit-factcheck">appConfig.can_edit_factcheck</a> configuration flag.
/// Possible errors
/// Code Type Description
/// 403 CHAT_ACTION_FORBIDDEN You cannot execute this action.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.editFactCheck"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class EditFactCheckHandler(IQueryProcessor queryProcessor, ICommandBus commandBus, IAccessHashHelper accessHashHelper) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestEditFactCheck, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestEditFactCheck obj)
    {
        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);

        var peer = obj.Peer.ToPeer(input.UserId);
        var ownerPeerId = peer.PeerId;
        if (peer.PeerType != PeerType.Channel)
        {
            ownerPeerId = input.UserId;
        }

        var messageId = MessageId.Create(ownerPeerId, obj.MsgId);
        var messageReadModel = await queryProcessor.ProcessAsync(new GetMessageByIdQuery(messageId.Value));
        if (messageReadModel == null)
        {
            RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        }

        // Note: the fact-checker's own country is not supplied by RequestEditFactCheck, only the text is;
        // the country is derived server-side (not implemented here, so it is left unset).
        var factCheckHash = (long)(uint)(obj.Text.Text ?? string.Empty).GetHashCode();

        var command = new SetFactCheckCommand(messageId, input.ToRequestInfo(), ownerPeerId, obj.MsgId, null, obj.Text, factCheckHash);
        await commandBus.PublishAsync(command);

        return new TUpdates { Chats = [], Updates = [], Users = [], Date = CurrentDate };
    }
}
