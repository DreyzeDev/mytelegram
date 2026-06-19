namespace MyTelegram.QueryHandlers.MongoDB.Dialog;

public class GetDialogFilterByIdQueryHandler(IQueryOnlyReadModelStore<DialogFilterReadModel> store)
    : IQueryHandler<GetDialogFilterByIdQuery, IDialogFilterReadModel?>
{
    public async Task<IDialogFilterReadModel?> ExecuteQueryAsync(GetDialogFilterByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.OwnerUserId == query.OwnerUserId && p.Filter.Id == query.FolderId, cancellationToken);
}
