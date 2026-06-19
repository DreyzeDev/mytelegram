namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Returns identifiers of pinned or always included chats from a chat folder imported using a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>, which are suggested to be left when the chat folder is deleted.
/// Possible errors
/// Code Type Description
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// 400 FILTER_NOT_SUPPORTED The specified filter cannot be used in this context.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.getLeaveChatlistSuggestions"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetLeaveChatlistSuggestionsHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestGetLeaveChatlistSuggestions, TVector<MyTelegram.Schema.IPeer>>
{
    protected override async Task<TVector<MyTelegram.Schema.IPeer>> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestGetLeaveChatlistSuggestions obj)
    {
        var result = new TVector<IPeer>();
        var filter = await queryProcessor.ProcessAsync(new GetDialogFilterByIdQuery(input.UserId, obj.Chatlist.FilterId));
        if (filter == null) return result;

        foreach (var peer in filter.Filter.IncludePeers)
        {
            result.Add(peer.Peer.PeerType switch
            {
                PeerType.User => (IPeer)new TPeerUser { UserId = peer.Peer.PeerId },
                PeerType.Chat => new TPeerChat { ChatId = peer.Peer.PeerId },
                _ => new TPeerChannel { ChannelId = peer.Peer.PeerId }
            });
        }
        return result;
    }
}
