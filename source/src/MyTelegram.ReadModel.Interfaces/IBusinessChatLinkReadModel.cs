namespace MyTelegram.ReadModel.Interfaces;

public interface IBusinessChatLinkReadModel : IReadModel
{
    long UserId { get; }
    string Slug { get; }
    string Message { get; }
    string? EntitiesJson { get; }
    string? Title { get; }
    int Views { get; }
}
