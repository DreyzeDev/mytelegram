namespace MyTelegram.QueryHandlers.InMemory.StarGift;

public class GetUserStarGiftByMsgIdQueryHandler(IQueryOnlyReadModelStore<UserStarGiftReadModel> store)
    : IQueryHandler<GetUserStarGiftByMsgIdQuery, IUserStarGiftReadModel?>
{
    public async Task<IUserStarGiftReadModel?> ExecuteQueryAsync(GetUserStarGiftByMsgIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(
            p => p.OwnerPeerId == query.OwnerPeerId && p.MsgId == query.MsgId,
            cancellationToken);
}
