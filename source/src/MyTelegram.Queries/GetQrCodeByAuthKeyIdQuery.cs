namespace MyTelegram.Queries;

public class GetQrCodeByAuthKeyIdQuery(long tempAuthKeyId, long permAuthKeyId) : IQuery<IQrCodeReadModel?>
{
    public long TempAuthKeyId { get; } = tempAuthKeyId;
    public long PermAuthKeyId { get; } = permAuthKeyId;
}
