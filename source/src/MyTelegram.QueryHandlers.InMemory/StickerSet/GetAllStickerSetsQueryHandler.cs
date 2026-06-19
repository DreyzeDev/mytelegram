namespace MyTelegram.QueryHandlers.InMemory.StickerSet;

public class GetAllStickerSetsQueryHandler(IQueryOnlyReadModelStore<StickerSetReadModel> store)
    : IQueryHandler<GetAllStickerSetsQuery, IReadOnlyCollection<IStickerSetReadModel>>
{
    public Task<IReadOnlyCollection<IStickerSetReadModel>> ExecuteQueryAsync(GetAllStickerSetsQuery query, CancellationToken cancellationToken)
        => store.FindAsync(p => p.StickerSetType == StickerSetType.Regular, p => (IStickerSetReadModel)p, cancellationToken: cancellationToken);
}
