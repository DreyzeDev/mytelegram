namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// List all <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep links »</a> associated to a folder
/// Possible errors
/// Code Type Description
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.getExportedInvites"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetExportedInvitesHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestGetExportedInvites, MyTelegram.Schema.Chatlists.IExportedInvites>
{
    protected override async Task<MyTelegram.Schema.Chatlists.IExportedInvites> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestGetExportedInvites obj)
    {
        var invites = await queryProcessor.ProcessAsync(new GetChatlistInvitesByFilterIdQuery(input.UserId, obj.Chatlist.FilterId));
        var inviteList = new TVector<IExportedChatlistInvite>();
        foreach (var inv in invites)
        {
            var peers = DeserializePeers(inv.PeersJson);
            inviteList.Add(new TExportedChatlistInvite
            {
                Title = inv.Title,
                Url = $"https://t.me/addlist/{inv.Slug}",
                Peers = peers
            });
        }
        return new MyTelegram.Schema.Chatlists.TExportedInvites { Invites = inviteList, Chats = [], Users = [] };
    }

    private static TVector<IPeer> DeserializePeers(string peersJson)
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