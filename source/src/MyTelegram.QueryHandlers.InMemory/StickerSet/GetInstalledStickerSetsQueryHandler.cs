namespace MyTelegram.QueryHandlers.InMemory.StickerSet;

public class GetInstalledStickerSetsQueryHandler(IQueryOnlyReadModelStore<InstalledStickerSetReadModel> store)
    : IQueryHandler<GetInstalledStickerSetsQuery, IReadOnlyCollection<IInstalledStickerSetReadModel>>
{
    public Task<IReadOnlyCollection<IInstalledStickerSetReadModel>> ExecuteQueryAsync(GetInstalledStickerSetsQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.UserId == query.UserId && p.StickerSetType == query.StickerSetType && !p.IsDeleted,
            p => (IInstalledStickerSetReadModel)p,
            cancellationToken: cancellationToken);
}
