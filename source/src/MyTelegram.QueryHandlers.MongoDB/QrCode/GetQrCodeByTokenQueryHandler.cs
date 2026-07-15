using MyTelegram.ReadModel.Impl;

namespace MyTelegram.QueryHandlers.MongoDB.QrCode;

public class GetQrCodeByTokenQueryHandler(IQueryOnlyReadModelStore<QrCodeReadModel> store)
    : IQueryHandler<GetQrCodeByTokenQuery, IQrCodeReadModel?>
{
    public async Task<IQrCodeReadModel?> ExecuteQueryAsync(GetQrCodeByTokenQuery query, CancellationToken cancellationToken)
    {
        return await store.FirstOrDefaultAsync(p => p.Token == query.Token, cancellationToken);
    }
}
