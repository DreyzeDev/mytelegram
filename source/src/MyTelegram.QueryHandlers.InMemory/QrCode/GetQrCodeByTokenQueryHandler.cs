using MyTelegram.ReadModel.Impl;

namespace MyTelegram.QueryHandlers.InMemory.QrCode;

public class GetQrCodeByTokenQueryHandler(IQueryOnlyReadModelStore<QrCodeReadModel> store)
    : IQueryHandler<GetQrCodeByTokenQuery, IQrCodeReadModel?>
{
    public async Task<IQrCodeReadModel?> ExecuteQueryAsync(GetQrCodeByTokenQuery query, CancellationToken cancellationToken)
    {
        return await store.FirstOrDefaultAsync(p => p.Token.SequenceEqual(query.Token), cancellationToken);
    }
}
