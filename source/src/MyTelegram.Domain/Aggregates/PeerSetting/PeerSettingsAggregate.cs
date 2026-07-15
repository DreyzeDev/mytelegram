namespace MyTelegram.Domain.Aggregates.PeerSetting;

[EnableAutoGeneration]
public class PeerSettingsAggregate : SnapshotAggregateRoot<PeerSettingsAggregate, PeerSettingsId, PeerSettingsSnapshot>
{
    private readonly PeerSettingsState _state = new();
    public PeerSettingsAggregate(PeerSettingsId id) : base(id, SnapshotEveryFewVersionsStrategy.Default)
    {
        Register(_state);
    }

    public void HidePeerSettingsBar(RequestInfo requestInfo, long targetPeerId)
    {
        var ownerPeerId = requestInfo.UserId;
        var peerId = targetPeerId;
        Emit(new PeerSettingsBarHiddenEvent(ownerPeerId, peerId));
    }

    public void TogglePeerTranslations(RequestInfo requestInfo, long targetPeerId, bool disabled)
    {
        var ownerPeerId = requestInfo.UserId;
        var peerId = targetPeerId;
        Emit(new PeerTranslationsToggledEvent(requestInfo, ownerPeerId, peerId, disabled));
    }

    public void SetChatTheme(RequestInfo requestInfo, long targetPeerId, string? themeEmoji, string? themeGiftSlug)
    {
        var ownerPeerId = requestInfo.UserId;
        var peerId = targetPeerId;
        Emit(new ChatThemeChangedEvent(requestInfo, ownerPeerId, peerId, themeEmoji, themeGiftSlug));
    }

    public void SetChatWallPaper(RequestInfo requestInfo,
        long targetPeerId,
        long? wallPaperId,
        long? wallPaperAccessHash,
        string? wallPaperSlug,
        WallPaperSettings? wallPaperSettings,
        bool forBoth,
        bool reverted)
    {
        var ownerPeerId = requestInfo.UserId;
        var peerId = targetPeerId;
        Emit(new ChatWallPaperChangedEvent(requestInfo, ownerPeerId, peerId, wallPaperId, wallPaperAccessHash, wallPaperSlug, wallPaperSettings, forBoth, reverted));
    }

    protected override Task<PeerSettingsSnapshot> CreateSnapshotAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new PeerSettingsSnapshot(
            _state.PeerSettings,
            _state.HidePeerSettingsBar,
            _state.OwnerPeerId,
            _state.PeerId,
            _state.TranslationsDisabled,
            _state.ThemeEmoji,
            _state.ThemeGiftSlug,
            _state.WallPaperId,
            _state.WallPaperAccessHash,
            _state.WallPaperSlug,
            _state.WallPaperSettings,
            _state.WallPaperForBoth));
    }

    protected override Task LoadSnapshotAsync(PeerSettingsSnapshot snapshot, ISnapshotMetadata metadata, CancellationToken cancellationToken)
    {
        _state.LoadSnapshot(snapshot);
        return Task.CompletedTask;
    }
}