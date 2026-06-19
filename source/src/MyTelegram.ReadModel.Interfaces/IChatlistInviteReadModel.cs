namespace MyTelegram.ReadModel.Interfaces;

public interface IChatlistInviteReadModel : IReadModel
{
    long UserId { get; }
    int FilterId { get; }
    string Slug { get; }
    string Title { get; }
    string PeersJson { get; }
    string? Emoticon { get; }
}
