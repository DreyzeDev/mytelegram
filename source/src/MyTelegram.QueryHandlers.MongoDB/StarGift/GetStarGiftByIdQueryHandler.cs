namespace MyTelegram.QueryHandlers.MongoDB.StarGift;

public class GetStarGiftByIdQueryHandler(IQueryOnlyReadModelStore<StarGiftReadModel> store)
    : IQueryHandler<GetStarGiftByIdQuery, IStarGiftReadModel?>
{
    public async Task<IStarGiftReadModel?> ExecuteQueryAsync(GetStarGiftByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.GiftId == query.GiftId, cancellationToken);
}
