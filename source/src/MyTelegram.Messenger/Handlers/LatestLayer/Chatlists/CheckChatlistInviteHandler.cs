namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Obtain information about a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.
/// Possible errors
/// Code Type Description
/// 400 INVITE_SLUG_EMPTY The specified invite slug is empty.
/// 400 INVITE_SLUG_EXPIRED The specified chat folder link has expired.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.checkChatlistInvite"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class CheckChatlistInviteHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestCheckChatlistInvite, MyTelegram.Schema.Chatlists.IChatlistInvite>
{
    protected override async Task<MyTelegram.Schema.Chatlists.IChatlistInvite> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestCheckChatlistInvite obj)
    {
        if (string.IsNullOrEmpty(obj.Slug))
        {
            RpcErrors.RpcErrors400.InviteSlugEmpty.ThrowRpcError();
            return default!;
        }

        var invite = await queryProcessor.ProcessAsync(new GetChatlistInviteBySlugQuery(obj.Slug));
        if (invite == null)
        {
            RpcErrors.RpcErrors400.InviteSlugExpired.ThrowRpcError();
            return default!;
        }

        var existingFolder = await queryProcessor.ProcessAsync(new GetImportedDialogFolderQuery(input.UserId, obj.Slug));
        var peers = DeserializePeers(invite.PeersJson);

        if (existingFolder != null)
        {
            return new MyTelegram.Schema.Chatlists.TChatlistInviteAlready
            {
                FilterId = existingFolder.Filter.Id,
                MissingPeers = [],
                AlreadyPeers = peers,
                Chats = [],
                Users = []
            };
        }

        return new MyTelegram.Schema.Chatlists.TChatlistInvite
        {
            Title = new TTextWithEntities { Text = invite.Title, Entities = [] },
            Emoticon = invite.Emoticon,
            Peers = peers,
            Chats = [],
            Users = []
        };
    }

    private static TVector<IPeer> DeserializePeers(string peersJson)
    {
        var result = new TVector<IPeer>();
        try
        {
            var domainPeers = System.Text.Json.JsonSerializer.Deserialize<List<Peer>>(peersJson);
            if (domainPeers == null) return result;
            foreach (var p in domainPeers)
            {
                result.Add(p.PeerType switch
                {
                    PeerType.User => (IPeer)new TPeerUser { UserId = p.PeerId },
                    PeerType.Chat => new TPeerChat { ChatId = p.PeerId },
                    _ => new TPeerChannel { ChannelId = p.PeerId }
                });
            }
        }
        catch { }
        return result;
    }
}