namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Uploads a <a href="https://corefork.telegram.org/api/stories">Telegram Story</a>.May also be used in a <a href="https://corefork.telegram.org/api/bots/connected-business-bots">business connection</a>, <em>not</em> by wrapping the query in <a href="https://corefork.telegram.org/method/invokeWithBusinessConnection">invokeWithBusinessConnection »</a>, but rather by specifying the ID of a controlled business user in <code>peer</code>.
/// Possible errors
/// Code Type Description
/// 400 MEDIA_EMPTY The provided media object is invalid.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.sendStory"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class SendStoryHandler(
    ICommandBus commandBus,
    IIdGenerator idGenerator,
    IMediaHelper mediaHelper,
    IPeerHelper peerHelper,
    IPrivacyAppService privacyAppService)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestSendStory, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestSendStory obj)
    {
        var media = await mediaHelper.SaveMediaAsync(obj.Media);
        if (media == null || media is TMessageMediaEmpty)
        {
            RpcErrors.RpcErrors400.MediaEmpty.ThrowRpcError();
            return default!;
        }

        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);
        var storyId = (int)await idGenerator.NextIdAsync(IdType.StoryId, ownerPeer.PeerId);
        var date = CurrentDate;
        var period = obj.Period ?? 86400;
        var expireDate = date + period;

        var privacyRules = privacyAppService.GetPrivacyValueDataList(obj.PrivacyRules);

        var storyItem = new StoryItem(
            Id: storyId,
            Peer: ownerPeer,
            Media: media,
            RandomId: obj.RandomId,
            PrivacyRules: privacyRules,
            Date: date,
            ExpireDate: expireDate,
            Caption: obj.Caption,
            MediaAreas: obj.MediaAreas?.ToList(),
            Pinned: obj.Pinned,
            NoForwards: obj.Noforwards,
            Entities: obj.Entities?.ToList(),
            Period: period
        );

        var command = new CreateStoryCommand(
            StoryId.Create(ownerPeer.PeerId, storyId),
            input.ToRequestInfo(),
            storyItem);
        await commandBus.PublishAsync(command, default);

        var schemaStory = BuildStoryItem(storyItem, privacyRules);
        var posterPeer = ownerPeer.PeerType == PeerType.Channel
            ? (IPeer)new TPeerChannel { ChannelId = ownerPeer.PeerId }
            : new TPeerUser { UserId = ownerPeer.PeerId };

        return new TUpdates
        {
            Updates = new TVector<IUpdate>
            {
                new TUpdateStory { Peer = posterPeer, Story = schemaStory }
            },
            Users = [],
            Chats = [],
            Date = date,
            Seq = 0
        };
    }

    private static TStoryItem BuildStoryItem(StoryItem item, List<PrivacyValueData> privacyRules)
    {
        var isPublic = privacyRules.Any(r => r.PrivacyValueType == PrivacyValueType.AllowAll);
        var isContacts = !isPublic && privacyRules.Any(r => r.PrivacyValueType == PrivacyValueType.AllowContacts);
        var isCloseFriends = !isPublic && !isContacts && privacyRules.Any(r => r.PrivacyValueType == PrivacyValueType.AllowCloseFriends);
        var isSelectedContacts = !isPublic && !isContacts && !isCloseFriends && privacyRules.Any(r => r.PrivacyValueType == PrivacyValueType.AllowUsers);

        var schemaPrivacy = privacyRules.Select<PrivacyValueData, IPrivacyRule>(r => r.PrivacyValueType switch
        {
            PrivacyValueType.AllowAll => new TPrivacyValueAllowAll(),
            PrivacyValueType.AllowContacts => new TPrivacyValueAllowContacts(),
            PrivacyValueType.AllowCloseFriends => new TPrivacyValueAllowCloseFriends(),
            PrivacyValueType.AllowUsers => new TPrivacyValueAllowUsers { Users = [] },
            PrivacyValueType.DisallowAll => new TPrivacyValueDisallowAll(),
            PrivacyValueType.DisallowContacts => new TPrivacyValueDisallowContacts(),
            PrivacyValueType.DisallowUsers => new TPrivacyValueDisallowUsers { Users = [] },
            _ => new TPrivacyValueAllowAll()
        }).ToList();

        return new TStoryItem
        {
            Id = item.Id,
            Date = item.Date,
            ExpireDate = item.ExpireDate,
            Media = item.Media,
            Caption = item.Caption,
            Entities = item.Entities != null ? new TVector<IMessageEntity>(item.Entities) : null,
            MediaAreas = item.MediaAreas != null ? new TVector<IMediaArea>(item.MediaAreas) : null,
            Privacy = new TVector<IPrivacyRule>(schemaPrivacy),
            Pinned = item.Pinned,
            Noforwards = item.NoForwards,
            Public = isPublic,
            Contacts = isContacts,
            CloseFriends = isCloseFriends,
            SelectedContacts = isSelectedContacts,
            Out = true,
            Views = new TStoryViews { ViewsCount = 0, ReactionsCount = 0, HasViewers = false }
        };
    }
}
