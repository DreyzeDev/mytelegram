namespace MyTelegram.Messenger.Handlers.LatestLayer.Stories;

internal static class StoryBuilderHelper
{
    internal static TStoryItem BuildFromReadModel(IStoryReadModel rm, bool isOwner = false)
    {
        var privacy = rm.PrivacyRules;
        var isPublic = privacy.Any(r => r.PrivacyValueType == PrivacyValueType.AllowAll);
        var isContacts = !isPublic && privacy.Any(r => r.PrivacyValueType == PrivacyValueType.AllowContacts);
        var isCloseFriends = !isPublic && !isContacts && privacy.Any(r => r.PrivacyValueType == PrivacyValueType.AllowCloseFriends);
        var isSelectedContacts = !isPublic && !isContacts && !isCloseFriends && privacy.Any(r => r.PrivacyValueType == PrivacyValueType.AllowUsers);

        var schemaPrivacy = privacy.Select<PrivacyValueData, IPrivacyRule>(r => r.PrivacyValueType switch
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

        var recentViewers = rm.RecentViewers?.Count > 0
            ? new TVector<long>(rm.RecentViewers)
            : null;

        return new TStoryItem
        {
            Id = rm.StoryId,
            Date = rm.Date,
            ExpireDate = rm.ExpireDate,
            Media = rm.Media,
            Caption = rm.Caption,
            Entities = rm.Entities != null ? new TVector<IMessageEntity>(rm.Entities) : null,
            MediaAreas = rm.MediaAreas != null ? new TVector<IMediaArea>(rm.MediaAreas) : null,
            Privacy = isOwner ? new TVector<IPrivacyRule>(schemaPrivacy) : null,
            Pinned = rm.Pinned,
            Noforwards = rm.NoForwards,
            Public = isPublic,
            Contacts = isContacts,
            CloseFriends = isCloseFriends,
            SelectedContacts = isSelectedContacts,
            Out = isOwner,
            Views = isOwner
                ? new TStoryViews
                {
                    ViewsCount = rm.ViewsCount,
                    HasViewers = rm.ViewsCount > 0,
                    RecentViewers = recentViewers
                }
                : null
        };
    }

    internal static IPeer ToPeer(long peerId, PeerType peerType)
    {
        return peerType == PeerType.Channel
            ? new TPeerChannel { ChannelId = peerId }
            : new TPeerUser { UserId = peerId };
    }
}
