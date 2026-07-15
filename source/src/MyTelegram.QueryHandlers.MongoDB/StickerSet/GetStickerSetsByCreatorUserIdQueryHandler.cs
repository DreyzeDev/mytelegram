namespace MyTelegram.QueryHandlers.MongoDB.StickerSet;

public class GetStickerSetsByCreatorUserIdQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<GetStickerSetsByCreatorUserIdQuery, IReadOnlyCollection<IStickerSetReadModel>>
{
    public Task<IReadOnlyCollection<IStickerSetReadModel>> ExecuteQueryAsync(GetStickerSetsByCreatorUserIdQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.CreatorUserId == query.CreatorUserId && p.StickerSetId > query.OffsetId,
            p => (IStickerSetReadModel)p,
            limit: query.Limit > 0 ? query.Limit : 100,
            sort: new SortOptions<StickerSetReadModel>(p => p.StickerSetId, SortType.Ascending),
            cancellationToken: cancellationToken);
}
