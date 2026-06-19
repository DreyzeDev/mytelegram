namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Fetch new chats associated with an imported <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>. Must be invoked at most every <code>chatlist_update_period</code> seconds (as per the related <a href="https://corefork.telegram.org/api/config#chatlist-update-period">client configuration parameter »</a>).
/// Possible errors
/// Code Type Description
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// 400 FILTER_NOT_SUPPORTED The specified filter cannot be used in this context.
/// 400 INPUT_CHATLIST_INVALID The specified folder is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.getChatlistUpdates"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetChatlistUpdatesHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestGetChatlistUpdates, MyTelegram.Schema.Chatlists.IChatlistUpdates>
{
    protected override async Task<MyTelegram.Schema.Chatlists.IChatlistUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestGetChatlistUpdates obj)
    {
        var missingPeers = new TVector<IPeer>();

        var filter = await queryProcessor.ProcessAsync(new GetDialogFilterByIdQuery(input.UserId, obj.Chatlist.FilterId));
        if (filter?.ImportedFromSlug == null)
            return new MyTelegram.Schema.Chatlists.TChatlistUpdates { MissingPeers = missingPeers, Chats = [], Users = [] };

        var invite = await queryProcessor.ProcessAsync(new GetChatlistInviteBySlugQuery(filter.ImportedFromSlug));
        if (invite == null)
            return new MyTelegram.Schema.Chatlists.TChatlistUpdates { MissingPeers = missingPeers, Chats = [], Users = [] };

        var invitePeers = System.Text.Json.JsonSerializer.Deserialize<List<Peer>>(invite.PeersJson) ?? [];
        var existingPeerIds = filter.Filter.IncludePeers.Select(p => p.Peer.PeerId).ToHashSet();

        foreach (var p in invitePeers)
        {
            if (!existingPeerIds.Contains(p.PeerId))
            {
                missingPeers.Add(p.PeerType switch
                {
                    PeerType.User => (IPeer)new TPeerUser { UserId = p.PeerId },
                    PeerType.Chat => new TPeerChat { ChatId = p.PeerId },
                    _ => new TPeerChannel { ChannelId = p.PeerId }
                });
            }
        }

        return new MyTelegram.Schema.Chatlists.TChatlistUpdates { MissingPeers = missingPeers, Chats = [], Users = [] };
    }
}
