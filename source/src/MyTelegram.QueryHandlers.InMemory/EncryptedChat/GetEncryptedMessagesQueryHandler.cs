namespace MyTelegram.QueryHandlers.InMemory.EncryptedChat;

public class GetEncryptedMessagesQueryHandler(IQueryOnlyReadModelStore<EncryptedMessageReadModel> store)
    : IQueryHandler<GetEncryptedMessagesQuery, IReadOnlyCollection<IEncryptedMessageReadModel>>
{
    public async Task<IReadOnlyCollection<IEncryptedMessageReadModel>> ExecuteQueryAsync(GetEncryptedMessagesQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(
            p => p.UserId == query.UserId && p.PermAuthKeyId == query.PermAuthKeyId && p.Qts > query.Qts,
            cancellationToken: cancellationToken);
}
