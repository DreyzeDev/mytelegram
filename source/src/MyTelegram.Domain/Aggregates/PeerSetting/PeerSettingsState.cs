namespace MyTelegram.Domain.Aggregates.PeerSetting;

public class PeerSettingsState : AggregateState<PeerSettingsAggregate, PeerSettingsId, PeerSettingsState>,
    IApply<PeerSettingsBarHiddenEvent>,
    IApply<PeerTranslationsToggledEvent>,
    IApply<ChatThemeChangedEvent>,
    IApply<ChatWallPaperChangedEvent>
{
    public PeerSettings PeerSettings { get; private set; } = new();
    public bool HidePeerSettingsBar { get; private set; }
    public long OwnerPeerId { get; private set; }
    public long PeerId { get; private set; }
    public bool TranslationsDisabled { get; private set; }
    public string? ThemeEmoji { get; private set; }
    public string? ThemeGiftSlug { get; private set; }
    public long? WallPaperId { get; private set; }
    public long? WallPaperAccessHash { get; private set; }
    public string? WallPaperSlug { get; private set; }
    public WallPaperSettings? WallPaperSettings { get; private set; }
    public bool WallPaperForBoth { get; private set; }

    public void LoadSnapshot(PeerSettingsSnapshot snapshot)
    {
        PeerSettings = snapshot.PeerSettings;
        HidePeerSettingsBar = snapshot.HidePeerSettingsBar;
        OwnerPeerId = snapshot.OwnerPeerId;
        PeerId = snapshot.PeerId;
        TranslationsDisabled = snapshot.TranslationsDisabled;
        ThemeEmoji = snapshot.ThemeEmoji;
        ThemeGiftSlug = snapshot.ThemeGiftSlug;
        WallPaperId = snapshot.WallPaperId;
        WallPaperAccessHash = snapshot.WallPaperAccessHash;
        WallPaperSlug = snapshot.WallPaperSlug;
        WallPaperSettings = snapshot.WallPaperSettings;
        WallPaperForBoth = snapshot.WallPaperForBoth;
    }

    public void Apply(PeerSettingsBarHiddenEvent aggregateEvent)
    {
        HidePeerSettingsBar = true;
        OwnerPeerId = aggregateEvent.OwnerPeerId;
        PeerId = aggregateEvent.PeerId;
    }

    public void Apply(PeerTranslationsToggledEvent aggregateEvent)
    {
        OwnerPeerId = aggregateEvent.OwnerPeerId;
        PeerId = aggregateEvent.PeerId;
        TranslationsDisabled = aggregateEvent.Disabled;
    }

    public void Apply(ChatThemeChangedEvent aggregateEvent)
    {
        OwnerPeerId = aggregateEvent.OwnerPeerId;
        PeerId = aggregateEvent.PeerId;
        ThemeEmoji = aggregateEvent.ThemeEmoji;
        ThemeGiftSlug = aggregateEvent.ThemeGiftSlug;
    }

    public void Apply(ChatWallPaperChangedEvent aggregateEvent)
    {
        OwnerPeerId = aggregateEvent.OwnerPeerId;
        PeerId = aggregateEvent.PeerId;

        if (aggregateEvent.Reverted)
        {
            WallPaperId = null;
            WallPaperAccessHash = null;
            WallPaperSlug = null;
            WallPaperSettings = null;
            WallPaperForBoth = false;
            return;
        }

        WallPaperId = aggregateEvent.WallPaperId;
        WallPaperAccessHash = aggregateEvent.WallPaperAccessHash;
        WallPaperSlug = aggregateEvent.WallPaperSlug;
        WallPaperSettings = aggregateEvent.WallPaperSettings;
        WallPaperForBoth = aggregateEvent.ForBoth;
    }
}