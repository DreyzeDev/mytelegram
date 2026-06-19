namespace MyTelegram.QueryHandlers.MongoDB.StickerSet;

public class GetInstalledStickerSetQueryHandler(IQueryOnlyReadModelStore<InstalledStickerSetReadModel> store)
    : IQueryHandler<GetInstalledStickerSetQuery, IInstalledStickerSetReadModel?>
{
    public async Task<IInstalledStickerSetReadModel?> ExecuteQueryAsync(GetInstalledStickerSetQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.UserId == query.UserId && p.StickerSetId == query.StickerSetId && !p.IsDeleted, cancellationToken);
}
