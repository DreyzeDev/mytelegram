namespace MyTelegram.QueryHandlers.MongoDB.Messaging;

public class GetAutoDeleteMessagesQueryHandler(IQueryOnlyReadModelStore<MessageReadModel> store)
    : IQueryHandler<GetAutoDeleteMessagesQuery, IReadOnlyCollection<AutoDeleteMessageItem>>
{
    public Task<IReadOnlyCollection<AutoDeleteMessageItem>> ExecuteQueryAsync(
        GetAutoDeleteMessagesQuery query, CancellationToken cancellationToken)
    {
        return store.FindAsync(
            p => p.ExpirationTime != null && p.ExpirationTime <= query.MinDate,
            p => new AutoDeleteMessageItem(p.OwnerPeerId, p.MessageId, p.ToPeerType, p.ToPeerId, p.ExpirationTime.GetValueOrDefault()),
            skip: query.Skip,
            limit: query.Limit,
            cancellationToken: cancellationToken);
    }
}
