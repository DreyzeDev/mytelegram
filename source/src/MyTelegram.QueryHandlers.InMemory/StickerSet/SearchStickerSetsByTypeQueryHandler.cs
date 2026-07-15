namespace MyTelegram.QueryHandlers.InMemory.StickerSet;

public class SearchStickerSetsByTypeQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<SearchStickerSetsByTypeQuery, IReadOnlyCollection<IStickerSetReadModel>>
{
    public Task<IReadOnlyCollection<IStickerSetReadModel>> ExecuteQueryAsync(SearchStickerSetsByTypeQuery query, CancellationToken cancellationToken)
    {
        var q = (query.Q ?? string.Empty).ToLowerInvariant();
        Expression<Func<StickerSetReadModel, bool>> predicate = p => p.StickerSetType == query.StickerSetType;
        predicate = predicate.WhereIf(!string.IsNullOrEmpty(q),
            p => p.Title.ToLower().Contains(q) || p.ShortName.ToLower().Contains(q));

        return store.FindAsync(
            predicate,
            p => (IStickerSetReadModel)p,
            limit: query.Limit > 0 ? query.Limit : 30,
            cancellationToken: cancellationToken);
    }
}
