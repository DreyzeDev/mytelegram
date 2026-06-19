namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Join channels and supergroups recently added to a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.
/// Possible errors
/// Code Type Description
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// 400 FILTER_INCLUDE_EMPTY The include_peers vector of the filter is empty.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.joinChatlistUpdates"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class JoinChatlistUpdatesHandler(ICommandBus commandBus, IQueryProcessor queryProcessor, IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestJoinChatlistUpdates, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestJoinChatlistUpdates obj)
    {
        var filter = await queryProcessor.ProcessAsync(new GetDialogFilterByIdQuery(input.UserId, obj.Chatlist.FilterId));
        if (filter == null)
        {
            RpcErrors.RpcErrors400.FilterIdInvalid.ThrowRpcError();
            return default!;
        }

        if (obj.Peers == null || obj.Peers.Count == 0)
            return new TUpdates { Updates = [], Chats = [], Users = [], Date = CurrentDate };

        var existingPeerIds = filter.Filter.IncludePeers.Select(p => p.Peer.PeerId).ToHashSet();
        var updatedIncludePeers = filter.Filter.IncludePeers.ToList();

        foreach (var inputPeer in obj.Peers)
        {
            var peer = peerHelper.GetPeer(inputPeer, input.UserId);
            if (!existingPeerIds.Contains(peer.PeerId))
            {
                long accessHash = inputPeer switch
                {
                    TInputPeerChannel ch => ch.AccessHash,
                    TInputPeerUser u => u.AccessHash,
                    _ => 0
                };
                updatedIncludePeers.Add(new InputPeer(peer, accessHash));
                existingPeerIds.Add(peer.PeerId);
            }
        }

        var updatedFilter = filter.Filter with { IncludePeers = updatedIncludePeers };
        var command = new UpdateDialogFilterCommand(
            DialogFilterId.Create(input.UserId, obj.Chatlist.FilterId),
            input.ToRequestInfo(), input.UserId, updatedFilter);
        await commandBus.PublishAsync(command, default);

        return new TUpdates { Updates = [], Chats = [], Users = [], Date = CurrentDate };
    }
}
