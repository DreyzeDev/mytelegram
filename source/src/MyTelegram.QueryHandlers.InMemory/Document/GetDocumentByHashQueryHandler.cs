namespace MyTelegram.QueryHandlers.InMemory.Document;

public class GetDocumentByHashQueryHandler(IQueryOnlyReadModelStore<DocumentReadModel> store)
    : IQueryHandler<GetDocumentByHashQuery, IDocumentReadModel?>
{
    public async Task<IDocumentReadModel?> ExecuteQueryAsync(GetDocumentByHashQuery query, CancellationToken cancellationToken)
    {
        var candidates = await store.FindAsync(p => p.Size == query.Size && p.MimeType == query.MimeType && p.Sha256Hash != null, cancellationToken: cancellationToken);
        return candidates.FirstOrDefault(p => p.Sha256Hash != null && p.Sha256Hash.AsSpan().SequenceEqual(query.Sha256Hash));
    }
}
