namespace MyTelegram.Messenger.Handlers.LatestLayer.Chatlists;
/// <summary>
/// Export a <a href="https://corefork.telegram.org/api/folders">folder »</a>, creating a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.
/// Possible errors
/// Code Type Description
/// 400 CHANNEL_INVALID The provided channel is invalid.
/// 400 CHANNEL_PRIVATE You haven't joined this channel/supergroup.
/// 400 CHATLISTS_TOO_MUCH You have created too many folder links, hitting the <code>chatlist_invites_limit_default</code>/<code>chatlist_invites_limit_premium</code> <a href="https://corefork.telegram.org/api/config#chatlist-invites-limit-default">limits »</a>.
/// 400 CHAT_ADMIN_REQUIRED You must be an admin in this chat to do this.
/// 400 FILTER_ID_INVALID The specified filter ID is invalid.
/// 400 FILTER_NOT_SUPPORTED The specified filter cannot be used in this context.
/// 400 INVITES_TOO_MUCH The maximum number of per-folder invites specified by the <code>chatlist_invites_limit_default</code>/<code>chatlist_invites_limit_premium</code> <a href="https://corefork.telegram.org/api/config#chatlist-invites-limit-default">client configuration parameters »</a> was reached.
/// 400 PEERS_LIST_EMPTY The specified list of peers is empty.
/// <para><c>See <a href="https://corefork.telegram.org/method/chatlists.exportChatlistInvite"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ExportChatlistInviteHandler(ICommandBus commandBus, IQueryProcessor queryProcessor, IPeerHelper peerHelper)
    : RpcResultObjectHandler<MyTelegram.Schema.Chatlists.RequestExportChatlistInvite, MyTelegram.Schema.Chatlists.IExportedChatlistInvite>
{
    protected override async Task<MyTelegram.Schema.Chatlists.IExportedChatlistInvite> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Chatlists.RequestExportChatlistInvite obj)
    {
        var filterId = obj.Chatlist.FilterId;
        var filter = await queryProcessor.ProcessAsync(new GetDialogFilterByIdQuery(input.UserId, filterId));
        if (filter == null)
        {
            RpcErrors.RpcErrors400.FilterIdInvalid.ThrowRpcError();
            return default!;
        }

        var slug = Guid.NewGuid().ToString("N")[..16];
        var peersJson = System.Text.Json.JsonSerializer.Serialize(
            obj.Peers.Select(p => peerHelper.GetPeer(p, input.UserId)).ToList());

        var command = new CreateInviteCommand(
            ChatlistInviteId.Create(input.UserId, slug),
            input.ToRequestInfo(),
            input.UserId,
            filterId,
            slug,
            obj.Title,
            peersJson,
            filter.Filter.Emoticon);
        await commandBus.PublishAsync(command, default);

        var peers = BuildPeerList(obj.Peers.Select(p => peerHelper.GetPeer(p, input.UserId)));
        var dialogFilter = BuildDialogFilterChatlist(filter.Filter, hasMyInvites: true);

        return new MyTelegram.Schema.Chatlists.TExportedChatlistInvite
        {
            Filter = dialogFilter,
            Invite = new TExportedChatlistInvite
            {
                Title = obj.Title,
                Url = $"https://t.me/addlist/{slug}",
                Peers = peers
            }
        };
    }

    private static TVector<IPeer> BuildPeerList(IEnumerable<Peer> peers)
    {
        var result = new TVector<IPeer>();
        foreach (var peer in peers)
        {
            result.Add(peer.PeerType switch
            {
                PeerType.User => (IPeer)new TPeerUser { UserId = peer.PeerId },
                PeerType.Chat => new TPeerChat { ChatId = peer.PeerId },
                _ => new TPeerChannel { ChannelId = peer.PeerId }
            });
        }
        return result;
    }

    private static TDialogFilterChatlist BuildDialogFilterChatlist(DialogFilter f, bool hasMyInvites = false)
    {
        var pinnedPeers = new TVector<IInputPeer>(f.PinnedPeers.Select(BuildInputPeer));
        var includePeers = new TVector<IInputPeer>(f.IncludePeers.Select(BuildInputPeer));
        return new TDialogFilterChatlist
        {
            Id = f.Id,
            Title = f.Title,
            Emoticon = f.Emoticon,
            Color = f.Color,
            HasMyInvites = hasMyInvites,
            TitleNoanimate = f.TitleNoAnimate,
            PinnedPeers = pinnedPeers,
            IncludePeers = includePeers
        };
    }

    private static IInputPeer BuildInputPeer(InputPeer p) => p.Peer.PeerType switch
    {
        PeerType.User => new TInputPeerUser { UserId = p.Peer.PeerId, AccessHash = p.AccessHash },
        PeerType.Chat => new TInputPeerChat { ChatId = p.Peer.PeerId },
        _ => new TInputPeerChannel { ChannelId = p.Peer.PeerId, AccessHash = p.AccessHash }
    };
}