using MyTelegram.ReadModel.Impl;

namespace MyTelegram.QueryHandlers.InMemory.QrCode;

public class GetQrCodeByAuthKeyIdQueryHandler(IQueryOnlyReadModelStore<QrCodeReadModel> store)
    : IQueryHandler<GetQrCodeByAuthKeyIdQuery, IQrCodeReadModel?>
{
    public async Task<IQrCodeReadModel?> ExecuteQueryAsync(GetQrCodeByAuthKeyIdQuery query, CancellationToken cancellationToken)
    {
        return await store.FirstOrDefaultAsync(p => p.TempAuthKeyId == query.TempAuthKeyId || p.PermAuthKeyId == query.PermAuthKeyId, cancellationToken);
    }
}
