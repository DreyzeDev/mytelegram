namespace MyTelegram.Queries;

public class GetQrCodeByTokenQuery(byte[] token) : IQuery<IQrCodeReadModel?>
{
    public byte[] Token { get; } = token;
}
