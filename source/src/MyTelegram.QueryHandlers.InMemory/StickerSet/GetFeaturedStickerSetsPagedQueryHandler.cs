namespace MyTelegram.QueryHandlers.InMemory.StickerSet;

public class GetFeaturedStickerSetsPagedQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<GetFeaturedStickerSetsPagedQuery, IReadOnlyCollection<IStickerSetReadModel>>
{
    public Task<IReadOnlyCollection<IStickerSetReadModel>> ExecuteQueryAsync(GetFeaturedStickerSetsPagedQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.StickerSetType == query.StickerSetType && p.Featured,
            p => (IStickerSetReadModel)p,
            skip: query.Offset,
            limit: query.Limit > 0 ? query.Limit : 30,
            sort: new SortOptions<StickerSetReadModel>(p => p.StickerSetId, SortType.Descending),
            cancellationToken: cancellationToken);
}
