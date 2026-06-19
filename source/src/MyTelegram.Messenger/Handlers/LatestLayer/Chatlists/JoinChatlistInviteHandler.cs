namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Import a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>, joining some or all the chats in the folder.
/// Possible errors
/// Code Type Description
/// 400 CHANNELS_TOO_MUCH You have joined too many channels/supergroups.
/// 400 CHATLISTS_TOO_MUCH You have created too many folder links, hitting the <code>chatlist_invites_limit_default</code>/<code>chatlist_invites_limit_premium</code> <a href="https://corefork.telegram.org/api/config#chatlist-invites-limit-default">limits »</a>.
/// 400 FILTER_INCLUDE_EMPTY The include_peers vector of the filter is empty.
/// 400 INVITE_SLUG_EMPTY The specified invite slug is empty.
/// 400 INVITE_SLUG_EXPIRED The specified chat folder link has expired.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.joinChatlistInvite"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class JoinChatlistInviteHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestJoinChatlistInvite, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestJoinChatlistInvite obj)
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

        var existing = await queryProcessor.ProcessAsync(new GetImportedDialogFolderQuery(input.UserId, obj.Slug));
        if (existing == null)
        {
            var peers = System.Text.Json.JsonSerializer.Deserialize<List<Peer>>(invite.PeersJson) ?? [];
            var includePeers = peers.Select(p => new InputPeer(p, 0)).ToList();
            var filter = new DialogFilter(invite.FilterId, false, false, false, false, false, false, false, false,
                false, new TTextWithEntities { Text = invite.Title, Entities = [] }, invite.Emoticon, null,
                [], includePeers, [], true, obj.Slug);
            var command = new UpdateDialogFilterCommand(
                DialogFilterId.Create(input.UserId, invite.FilterId),
                input.ToRequestInfo(), input.UserId, filter);
            await commandBus.PublishAsync(command, default);
        }

        return new TUpdates { Updates = [], Chats = [], Users = [], Date = CurrentDate };
    }
}