namespace MyTelegram;

public record AutoDeleteMessageItem(long OwnerPeerId, int MessageId, PeerType ToPeerType, long ToPeerId, int ExpirationTime);