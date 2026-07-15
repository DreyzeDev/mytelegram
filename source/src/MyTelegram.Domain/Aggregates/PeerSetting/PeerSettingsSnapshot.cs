namespace MyTelegram.Domain.Aggregates.PeerSetting;

public class PeerSettingsSnapshot(
    PeerSettings peerSettings,
    bool hidePeerSettingsBar,
    long ownerPeerId,
    long peerId,
    bool translationsDisabled = false,
    string? themeEmoji = null,
    string? themeGiftSlug = null,
    long? wallPaperId = null,
    long? wallPaperAccessHash = null,
    string? wallPaperSlug = null,
    WallPaperSettings? wallPaperSettings = null,
    bool wallPaperForBoth = false)
    : ISnapshot
{
    public PeerSettings PeerSettings { get; } = peerSettings;
    public bool HidePeerSettingsBar { get; private set; } = hidePeerSettingsBar;
    public long OwnerPeerId { get; private set; } = ownerPeerId;
    public long PeerId { get; private set; } = peerId;
    public bool TranslationsDisabled { get; private set; } = translationsDisabled;
    public string? ThemeEmoji { get; private set; } = themeEmoji;
    public string? ThemeGiftSlug { get; private set; } = themeGiftSlug;
    public long? WallPaperId { get; private set; } = wallPaperId;
    public long? WallPaperAccessHash { get; private set; } = wallPaperAccessHash;
    public string? WallPaperSlug { get; private set; } = wallPaperSlug;
    public WallPaperSettings? WallPaperSettings { get; private set; } = wallPaperSettings;
    public bool WallPaperForBoth { get; private set; } = wallPaperForBoth;
}