namespace MyTelegram.ReadModel.Interfaces;

public interface IQrCodeReadModel : IReadModel
{
    long TempAuthKeyId { get; }
    long PermAuthKeyId { get; }
    byte[] Token { get; }
    int ExpireDate { get; }
    bool IsAccepted { get; }
    long UserId { get; }
}
