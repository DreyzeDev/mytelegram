namespace MyTelegram.QueryHandlers.InMemory.StickerSet;

public class GetStickerSetsByIdListQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<GetStickerSetsByIdListQuery, IReadOnlyCollection<IStickerSetReadModel>>
{
    public Task<IReadOnlyCollection<IStickerSetReadModel>> ExecuteQueryAsync(GetStickerSetsByIdListQuery query, CancellationToken cancellationToken)
        => store.FindAsync(p => query.StickerSetIds.Contains(p.StickerSetId), p => (IStickerSetReadModel)p, cancellationToken: cancellationToken);
}
