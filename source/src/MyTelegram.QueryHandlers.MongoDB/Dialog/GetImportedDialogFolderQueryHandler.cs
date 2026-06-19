namespace MyTelegram.QueryHandlers.MongoDB.Dialog;

public class GetImportedDialogFolderQueryHandler(IQueryOnlyReadModelStore<DialogFilterReadModel> store)
    : IQueryHandler<GetImportedDialogFolderQuery, IDialogFilterReadModel?>
{
    public async Task<IDialogFilterReadModel?> ExecuteQueryAsync(GetImportedDialogFolderQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.OwnerUserId == query.UserId && p.ImportedFromSlug == query.Slug, cancellationToken);
}
