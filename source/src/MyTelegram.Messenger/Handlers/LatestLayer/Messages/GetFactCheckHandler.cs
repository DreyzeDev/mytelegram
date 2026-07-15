namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Fetch one or more <a href="https://corefork.telegram.org/api/factcheck">factchecks, see here »</a> for the full flow.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getFactCheck"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetFactCheckHandler(IQueryProcessor queryProcessor, IAccessHashHelper accessHashHelper) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetFactCheck, TVector<MyTelegram.Schema.IFactCheck>>
{
    protected override async Task<TVector<MyTelegram.Schema.IFactCheck>> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetFactCheck obj)
    {
        await accessHashHelper.CheckAccessHashAsync(input, obj.Peer);

        var peer = obj.Peer.ToPeer(input.UserId);
        var ownerPeerId = peer.PeerId;
        if (peer.PeerType != PeerType.Channel)
        {
            ownerPeerId = input.UserId;
        }

        var factChecks = new List<IFactCheck>();
        foreach (var msgId in obj.MsgId)
        {
            var messageReadModel = await queryProcessor.ProcessAsync(new GetMessageByIdQuery(MessageId.Create(ownerPeerId, msgId).Value));
            if (messageReadModel == null || messageReadModel.FactCheckText == null)
            {
                // No fact-check attached to this message, return an empty placeholder (hash 0) so that
                // the returned vector still aligns 1:1 with the requested message id list.
                factChecks.Add(new TFactCheck { Country = null, Text = null, Hash = 0 });
                continue;
            }

            factChecks.Add(new TFactCheck
            {
                Country = messageReadModel.FactCheckCountry,
                Text = messageReadModel.FactCheckText,
                Hash = messageReadModel.FactCheckHash
            });
        }

        return new TVector<IFactCheck>(factChecks);
    }
}
