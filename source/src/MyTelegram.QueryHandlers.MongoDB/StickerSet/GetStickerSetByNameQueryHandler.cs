namespace MyTelegram.QueryHandlers.MongoDB.StickerSet;

public class GetStickerSetByNameQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<GetStickerSetByNameQuery, IStickerSetReadModel?>
{
    public async Task<IStickerSetReadModel?> ExecuteQueryAsync(GetStickerSetByNameQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.ShortName == query.ShortName, cancellationToken);
}
