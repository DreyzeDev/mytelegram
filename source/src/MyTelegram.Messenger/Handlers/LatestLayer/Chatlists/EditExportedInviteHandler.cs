namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Edit a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.
/// Possible errors
/// Code Type Description
/// 400 CHANNEL_INVALID The provided channel is invalid.
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// 400 FILTER_NOT_SUPPORTED The specified filter cannot be used in this context.
/// 400 INVITE_SLUG_EMPTY The specified invite slug is empty.
/// 400 INVITE_SLUG_EXPIRED The specified chat folder link has expired.
/// 400 PEERS_LIST_EMPTY The specified list of peers is empty.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.editExportedInvite"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class EditExportedInviteHandler(ICommandBus commandBus, IQueryProcessor queryProcessor, IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestEditExportedInvite, MyTelegram.Schema.IExportedChatlistInvite>
{
    protected override async Task<MyTelegram.Schema.IExportedChatlistInvite> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestEditExportedInvite obj)
    {
        var invite = await queryProcessor.ProcessAsync(new GetChatlistInviteBySlugQuery(obj.Slug));
        if (invite == null || invite.UserId != input.UserId)
        {
            RpcErrors.RpcErrors400.InviteSlugExpired.ThrowRpcError();
            return default!;
        }

        var newTitle = obj.Title ?? invite.Title;
        string? newPeersJson = null;
        if (obj.Peers is { Count: > 0 })
            newPeersJson = System.Text.Json.JsonSerializer.Serialize(
                obj.Peers.Select(p => peerHelper.GetPeer(p, input.UserId)).ToList());

        var command = new EditInviteCommand(
            ChatlistInviteId.Create(input.UserId, obj.Slug),
            newTitle,
            newPeersJson);
        await commandBus.PublishAsync(command, default);

        var peers = BuildPeerList(newPeersJson ?? invite.PeersJson);

        return new TExportedChatlistInvite
        {
            Title = newTitle,
            Url = $"https://t.me/addlist/{obj.Slug}",
            Peers = peers
        };
    }

    private static TVector<IPeer> BuildPeerList(string peersJson)
    {
        var result = new TVector<IPeer>();
        try
        {
            var domainPeers = System.Text.Json.JsonSerializer.Deserialize<List<Peer>>(peersJson);
            if (domainPeers == null) return result;
            foreach (var p in domainPeers)
                result.Add(p.PeerType switch
                {
                    PeerType.User => (IPeer)new TPeerUser { UserId = p.PeerId },
                    PeerType.Chat => new TPeerChat { ChatId = p.PeerId },
                    _ => new TPeerChannel { ChannelId = p.PeerId }
                });
        }
        catch { }
        return result;
    }
}