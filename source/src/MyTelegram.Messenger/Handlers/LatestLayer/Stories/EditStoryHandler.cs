namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;
/// <summary>
/// Edit an uploaded <a href="https://corefork.telegram.org/api/stories">story</a>May also be used in a <a href="https://corefork.telegram.org/api/bots/connected-business-bots">business connection</a>, <em>not</em> by wrapping the query in <a href="https://corefork.telegram.org/method/invokeWithBusinessConnection">invokeWithBusinessConnection »</a>, but rather by specifying the ID of a controlled business user in <code>peer</code>: in this context, the method can only be used to edit stories posted by the same business bot on behalf of the user with <a href="https://corefork.telegram.org/method/stories.sendStory">stories.sendStory</a>.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 STORY_NOT_MODIFIED The new story information you passed is equal to the previous story information, thus it wasn't modified.
/// <para><c>See <a href="https://corefork.telegram.org/method/stories.editStory"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class EditStoryHandler(
    ICommandBus commandBus,
    IQueryProcessor queryProcessor,
    IMediaHelper mediaHelper,
    IPeerHelper peerHelper,
    IPrivacyAppService privacyAppService)
    : RpcResultObjectHandler<MyTelegram.Schema.Stories.RequestEditStory, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Stories.RequestEditStory obj)
    {
        var ownerPeer = peerHelper.GetPeer(obj.Peer, input.UserId) ?? new Peer(PeerType.User, input.UserId);

        var existing = await queryProcessor.ProcessAsync(
            new GetStoryByIdQuery(ownerPeer.PeerId, obj.Id), default);
        if (existing == null)
            RpcErrors.RpcErrors400.StoryIdInvalid.ThrowRpcError();

        IMessageMedia media = existing!.Media;
        if (obj.Media != null)
        {
            var savedMedia = await mediaHelper.SaveMediaAsync(obj.Media);
            if (savedMedia != null && savedMedia is not TMessageMediaEmpty)
                media = savedMedia;
        }

        var privacyRules = obj.PrivacyRules != null
            ? privacyAppService.GetPrivacyValueDataList(obj.PrivacyRules)
            : existing.PrivacyRules;

        var editedItem = new StoryItem(
            Id: existing.StoryId,
            Peer: ownerPeer,
            Media: media,
            RandomId: existing.RandomId,
            PrivacyRules: privacyRules,
            Date: existing.Date,
            ExpireDate: existing.ExpireDate,
            Caption: obj.Caption ?? existing.Caption,
            MediaAreas: obj.MediaAreas?.ToList() ?? existing.MediaAreas,
            Pinned: existing.Pinned,
            NoForwards: existing.NoForwards,
            Entities: obj.Entities?.ToList() ?? existing.Entities,
            Period: existing.Period,
            FwdFromId: existing.FwdFromId,
            FwdFromStory: existing.FwdFromStory
        );

        var command = new EditStoryCommand(
            StoryId.Create(ownerPeer.PeerId, obj.Id),
            editedItem);
        await commandBus.PublishAsync(command, default);

        var schemaStory = StoryBuilderHelper.BuildFromReadModel(existing, isOwner: true);
        schemaStory.Media = editedItem.Media;
        schemaStory.Caption = editedItem.Caption;
        schemaStory.Entities = editedItem.Entities != null ? new TVector<IMessageEntity>(editedItem.Entities) : null;
        schemaStory.MediaAreas = editedItem.MediaAreas != null ? new TVector<IMediaArea>(editedItem.MediaAreas) : null;
        schemaStory.Edited = true;

        IPeer posterPeer = ownerPeer.PeerType == PeerType.Channel
            ? new TPeerChannel { ChannelId = ownerPeer.PeerId }
            : new TPeerUser { UserId = ownerPeer.PeerId };

        return new TUpdates
        {
            Updates = new TVector<IUpdate>
            {
                new TUpdateStory { Peer = posterPeer, Story = schemaStory }
            },
            Users = [],
            Chats = [],
            Date = CurrentDate,
            Seq = 0
        };
    }
}
