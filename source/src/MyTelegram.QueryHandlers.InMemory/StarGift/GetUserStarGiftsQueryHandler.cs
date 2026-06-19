namespace MyTelegram.QueryHandlers.InMemory.StarGift;

public class GetUserStarGiftsQueryHandler(IQueryOnlyReadModelStore<UserStarGiftReadModel> store)
    : IQueryHandler<GetUserStarGiftsQuery, IReadOnlyCollection<IUserStarGiftReadModel>>
{
    public Task<IReadOnlyCollection<IUserStarGiftReadModel>> ExecuteQueryAsync(GetUserStarGiftsQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.OwnerPeerId == query.OwnerPeerId &&
                 !p.Converted &&
                 (query.ExcludeUnsaved == null || p.Unsaved != query.ExcludeUnsaved.Value),
            p => (IUserStarGiftReadModel)p,
            skip: query.Offset,
            limit: query.Limit > 0 ? query.Limit : 100,
            sort: new SortOptions<UserStarGiftReadModel>(p => p.Date, SortType.Descending),
            cancellationToken: cancellationToken);
}
