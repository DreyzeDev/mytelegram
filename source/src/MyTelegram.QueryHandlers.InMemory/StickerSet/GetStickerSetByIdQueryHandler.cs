namespace MyTelegram.QueryHandlers.InMemory.StickerSet;

public class GetStickerSetByIdQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<GetStickerSetByIdQuery, IStickerSetReadModel?>
{
    public async Task<IStickerSetReadModel?> ExecuteQueryAsync(GetStickerSetByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.StickerSetId == query.StickerSetId, cancellationToken);
}
