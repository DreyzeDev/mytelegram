namespace MyTelegram.QueryHandlers.MongoDB.Document;

public class GetDocumentByIdQueryHandler(IQueryOnlyReadModelStore<DocumentReadModel> store)
    : IQueryHandler<GetDocumentByIdQuery, IDocumentReadModel?>
{
    public async Task<IDocumentReadModel?> ExecuteQueryAsync(GetDocumentByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.DocumentId == query.Id, cancellationToken);
}
