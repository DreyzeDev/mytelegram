namespace MyTelegram.QueryHandlers.MongoDB.StarGift;

public class GetAllStarGiftsQueryHandler(IQueryOnlyReadModelStore<StarGiftReadModel> store)
    : IQueryHandler<GetAllStarGiftsQuery, IReadOnlyCollection<IStarGiftReadModel>>
{
    public Task<IReadOnlyCollection<IStarGiftReadModel>> ExecuteQueryAsync(GetAllStarGiftsQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => !p.SoldOut,
            p => (IStarGiftReadModel)p,
            sort: new SortOptions<StarGiftReadModel>(p => p.Stars, SortType.Ascending),
            cancellationToken: cancellationToken);
}
