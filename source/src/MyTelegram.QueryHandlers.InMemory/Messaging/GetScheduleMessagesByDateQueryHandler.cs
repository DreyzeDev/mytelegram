namespace MyTelegram.QueryHandlers.InMemory.Messaging;

public class GetScheduleMessagesByDateQueryHandler(IQueryOnlyReadModelStore<MessageReadModel> store)
    : IQueryHandler<GetScheduleMessagesByDateQuery, IReadOnlyCollection<ScheduleItem>>
{
    public Task<IReadOnlyCollection<ScheduleItem>> ExecuteQueryAsync(
        GetScheduleMessagesByDateQuery query, CancellationToken cancellationToken)
    {
        return store.FindAsync(
            p => p.ScheduleDate != null && p.ScheduleDate <= query.MaxScheduleDate && p.Out,
            p => new ScheduleItem(p.OwnerPeerId, new Peer(p.ToPeerType, p.ToPeerId), p.MessageId, p.ScheduleDate.GetValueOrDefault(), p.GroupedId),
            cancellationToken: cancellationToken);
    }
}
