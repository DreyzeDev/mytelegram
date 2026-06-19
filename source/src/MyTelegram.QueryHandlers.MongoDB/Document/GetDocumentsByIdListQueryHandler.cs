namespace MyTelegram.QueryHandlers.MongoDB.Document;

public class GetDocumentsByIdListQueryHandler(IQueryOnlyReadModelStore<DocumentReadModel> store)
    : IQueryHandler<GetDocumentsByIdListQuery, IReadOnlyCollection<IDocumentReadModel>>
{
    public Task<IReadOnlyCollection<IDocumentReadModel>> ExecuteQueryAsync(GetDocumentsByIdListQuery query, CancellationToken cancellationToken)
        => store.FindAsync(p => query.Ids.Contains(p.DocumentId), p => (IDocumentReadModel)p, cancellationToken: cancellationToken);
}
