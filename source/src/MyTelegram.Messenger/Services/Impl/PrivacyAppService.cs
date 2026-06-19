namespace MyTelegram.Messenger.Services.Impl;

public class PrivacyAppService(
    ICacheManager<GlobalPrivacySettingsCacheItem> cacheManager,
    IQueryProcessor queryProcessor,
    ICommandBus commandBus)
    : BaseAppService, IPrivacyAppService, ITransientDependency
{
    public Task<IReadOnlyCollection<IPrivacyReadModel>> GetPrivacyListAsync(IReadOnlyList<long> userIds)
    {
        var allTypes = Enum.GetValues<PrivacyType>().Where(t => t != PrivacyType.Unknown).ToList();
        return queryProcessor.ProcessAsync(new GetPrivacyListQuery(userIds, allTypes));
    }

    public Task<IReadOnlyCollection<IPrivacyReadModel>> GetPrivacyListAsync(long userId)
        => GetPrivacyListAsync([userId]);

    public async Task<IReadOnlyList<IPrivacyRule>> GetPrivacyRulesAsync(long selfUserId, IInputPrivacyKey key)
    {
        var privacyType = ToPrivacyType(key);
        if (privacyType == PrivacyType.Unknown)
            return [];
        var model = await queryProcessor.ProcessAsync(new GetPrivacyQuery(selfUserId, privacyType));
        return model == null ? [] : ToPrivacyRules(model.PrivacyValueDataList);
    }

    public async Task<SetPrivacyOutput> SetPrivacyAsync(RequestInfo requestInfo, long selfUserId,
        IInputPrivacyKey key, IReadOnlyList<IInputPrivacyRule> ruleList)
    {
        var privacyType = ToPrivacyType(key);
        if (privacyType == PrivacyType.Unknown)
            return new SetPrivacyOutput([]);

        var valueDataList = ruleList.Select(GetPrivacyValueData).ToList();
        var aggregateId = PrivacyId.Create(selfUserId, privacyType);
        await commandBus.PublishAsync(new SetPrivacyCommand(aggregateId, selfUserId, privacyType, valueDataList));
        return new SetPrivacyOutput(ToPrivacyRules(valueDataList));
    }

    public async Task ApplyPrivacyAsync(long selfUserId, long targetUserId,
        Action<PrivacyValueType> executeOnPrivacyNotMatch, PrivacyType privacyType)
    {
        var model = await queryProcessor.ProcessAsync(new GetPrivacyQuery(targetUserId, privacyType));
        if (model == null) return;
        ApplyPrivacyRules(selfUserId, model, executeOnPrivacyNotMatch);
    }

    public async Task ApplyPrivacyAsync(long selfUserId, long targetUserId,
        Action<PrivacyValueType> executeOnPrivacyNotMatch, List<PrivacyType> privacyTypes)
    {
        foreach (var pt in privacyTypes)
            await ApplyPrivacyAsync(selfUserId, targetUserId, executeOnPrivacyNotMatch, pt);
    }

    public async Task ApplyPrivacyListAsync(long selfUserId, IReadOnlyList<long> targetUserIdList,
        Action<PrivacyValueType, long> executeOnPrivacyNotMatch, List<PrivacyType> privacyTypes)
    {
        var models = await queryProcessor.ProcessAsync(new GetPrivacyListQuery(targetUserIdList, privacyTypes));
        foreach (var model in models)
        {
            ApplyPrivacyRules(selfUserId, model,
                valueType => executeOnPrivacyNotMatch(valueType, model.UserId));
        }
    }

    public Task SetGlobalPrivacySettingsAsync(long selfUserId, GlobalPrivacySettings globalPrivacySettings)
        => Task.CompletedTask;

    public async Task<GlobalPrivacySettingsCacheItem?> GetGlobalPrivacySettingsAsync(long userId)
    {
        var cacheKey = GlobalPrivacySettingsCacheItem.GetCacheKey(userId);
        var item = await cacheManager.GetAsync(cacheKey);
        var globalPrivacySettings = await queryProcessor.ProcessAsync(new GetGlobalPrivacySettingsQuery(userId));
        if (globalPrivacySettings != null)
        {
            item = new GlobalPrivacySettingsCacheItem(globalPrivacySettings.ArchiveAndMuteNewNoncontactPeers,
                globalPrivacySettings.KeepArchivedUnmuted, globalPrivacySettings.KeepArchivedFolders,
                globalPrivacySettings.HideReadMarks, globalPrivacySettings.NewNoncontactPeersRequirePremium);
            await cacheManager.SetAsync(cacheKey, item);
        }
        return item;
    }

    public PrivacyValueData GetPrivacyValueData(IInputPrivacyRule rule) => rule switch
    {
        TInputPrivacyValueAllowAll => new PrivacyValueData(PrivacyValueType.AllowAll),
        TInputPrivacyValueAllowContacts => new PrivacyValueData(PrivacyValueType.AllowContacts),
        TInputPrivacyValueAllowCloseFriends => new PrivacyValueData(PrivacyValueType.AllowCloseFriends),
        TInputPrivacyValueAllowPremium => new PrivacyValueData(PrivacyValueType.AllowPremium),
        TInputPrivacyValueAllowBots => new PrivacyValueData(PrivacyValueType.AllowBots),
        TInputPrivacyValueAllowUsers r => new PrivacyValueData(PrivacyValueType.AllowUsers, System.Text.Json.JsonSerializer.Serialize(r.Users?.Select(u => u switch { TInputUser iu => iu.UserId, _ => 0L }).ToList())),
        TInputPrivacyValueAllowChatParticipants r => new PrivacyValueData(PrivacyValueType.AllowChatParticipants, System.Text.Json.JsonSerializer.Serialize(r.Chats?.ToList())),
        TInputPrivacyValueDisallowAll => new PrivacyValueData(PrivacyValueType.DisallowAll),
        TInputPrivacyValueDisallowContacts => new PrivacyValueData(PrivacyValueType.DisallowContacts),
        TInputPrivacyValueDisallowBots => new PrivacyValueData(PrivacyValueType.DisallowBots),
        TInputPrivacyValueDisallowUsers r => new PrivacyValueData(PrivacyValueType.DisallowUsers, System.Text.Json.JsonSerializer.Serialize(r.Users?.Select(u => u switch { TInputUser iu => iu.UserId, _ => 0L }).ToList())),
        TInputPrivacyValueDisallowChatParticipants r => new PrivacyValueData(PrivacyValueType.DisallowChatParticipants, System.Text.Json.JsonSerializer.Serialize(r.Chats?.ToList())),
        _ => new PrivacyValueData(PrivacyValueType.AllowAll)
    };

    public List<PrivacyValueData> GetPrivacyValueDataList(IList<IInputPrivacyRule> rules)
        => rules.Select(GetPrivacyValueData).ToList();

    private static PrivacyType ToPrivacyType(IInputPrivacyKey key) => key switch
    {
        TInputPrivacyKeyStatusTimestamp => PrivacyType.StatusTimestamp,
        TInputPrivacyKeyChatInvite => PrivacyType.ChatInvite,
        TInputPrivacyKeyPhoneCall => PrivacyType.PhoneCall,
        TInputPrivacyKeyPhoneP2P => PrivacyType.PhoneP2P,
        TInputPrivacyKeyForwards => PrivacyType.Forwards,
        TInputPrivacyKeyProfilePhoto => PrivacyType.ProfilePhoto,
        TInputPrivacyKeyPhoneNumber => PrivacyType.PhoneNumber,
        TInputPrivacyKeyAddedByPhone => PrivacyType.AddedByPhone,
        TInputPrivacyKeyVoiceMessages => PrivacyType.VoiceMessages,
        TInputPrivacyKeyAbout => PrivacyType.About,
        TInputPrivacyKeyBirthday => PrivacyType.Birthday,
        TInputPrivacyKeyStarGiftsAutoSave => PrivacyType.StarGiftsAutoSave,
        TInputPrivacyKeyNoPaidMessages => PrivacyType.NoPaidMessages,
        TInputPrivacyKeySavedMusic => PrivacyType.SavedMusic,
        _ => PrivacyType.Unknown
    };

    private static IReadOnlyList<IPrivacyRule> ToPrivacyRules(IEnumerable<PrivacyValueData> values)
        => values.Select<PrivacyValueData, IPrivacyRule>(v => v.PrivacyValueType switch
        {
            PrivacyValueType.AllowAll => new TPrivacyValueAllowAll(),
            PrivacyValueType.AllowContacts => new TPrivacyValueAllowContacts(),
            PrivacyValueType.AllowCloseFriends => new TPrivacyValueAllowCloseFriends(),
            PrivacyValueType.AllowPremium => new TPrivacyValueAllowPremium(),
            PrivacyValueType.AllowBots => new TPrivacyValueAllowBots(),
            PrivacyValueType.AllowUsers => new TPrivacyValueAllowUsers { Users = DeserializeIds(v.JsonData) },
            PrivacyValueType.AllowChatParticipants => new TPrivacyValueAllowChatParticipants { Chats = DeserializeIds(v.JsonData) },
            PrivacyValueType.DisallowAll => new TPrivacyValueDisallowAll(),
            PrivacyValueType.DisallowContacts => new TPrivacyValueDisallowContacts(),
            PrivacyValueType.DisallowBots => new TPrivacyValueDisallowBots(),
            PrivacyValueType.DisallowUsers => new TPrivacyValueDisallowUsers { Users = DeserializeIds(v.JsonData) },
            PrivacyValueType.DisallowChatParticipants => new TPrivacyValueDisallowChatParticipants { Chats = DeserializeIds(v.JsonData) },
            _ => new TPrivacyValueAllowAll()
        }).ToList();

    private static TVector<long> DeserializeIds(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new TVector<long>();
        var list = System.Text.Json.JsonSerializer.Deserialize<List<long>>(json) ?? [];
        return new TVector<long>(list);
    }

    private static void ApplyPrivacyRules(long selfUserId, IPrivacyReadModel model, Action<PrivacyValueType> onNotMatch)
    {
        foreach (var rule in model.PrivacyValueDataList)
        {
            switch (rule.PrivacyValueType)
            {
                case PrivacyValueType.AllowAll:
                    return;
                case PrivacyValueType.DisallowAll:
                    onNotMatch(PrivacyValueType.DisallowAll);
                    return;
                case PrivacyValueType.AllowUsers:
                    var allowIds = System.Text.Json.JsonSerializer.Deserialize<List<long>>(rule.JsonData ?? "[]") ?? [];
                    if (allowIds.Contains(selfUserId)) return;
                    break;
                case PrivacyValueType.DisallowUsers:
                    var disallowIds = System.Text.Json.JsonSerializer.Deserialize<List<long>>(rule.JsonData ?? "[]") ?? [];
                    if (disallowIds.Contains(selfUserId)) onNotMatch(PrivacyValueType.DisallowUsers);
                    return;
            }
        }
    }
}
