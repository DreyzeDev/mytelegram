namespace MyTelegram.QueryHandlers.MongoDB.Document;

public class GetDocumentByHashQueryHandler(IQueryOnlyReadModelStore<DocumentReadModel> store)
    : IQueryHandler<GetDocumentByHashQuery, IDocumentReadModel?>
{
    public async Task<IDocumentReadModel?> ExecuteQueryAsync(GetDocumentByHashQuery query, CancellationToken cancellationToken)
    {
        // Sha256Hash is a byte[] and can't be reliably compared for equality inside a Mongo expression tree,
        // so narrow down by size/mime type in the query and do the final hash comparison in memory.
        var candidates = await store.FindAsync(p => p.Size == query.Size && p.MimeType == query.MimeType && p.Sha256Hash != null, cancellationToken: cancellationToken);
        return candidates.FirstOrDefault(p => p.Sha256Hash != null && p.Sha256Hash.AsSpan().SequenceEqual(query.Sha256Hash));
    }
}
