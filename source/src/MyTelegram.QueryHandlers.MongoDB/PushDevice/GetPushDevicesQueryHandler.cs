namespace MyTelegram.QueryHandlers.MongoDB.PushDevice;

public class GetPushDevicesQueryHandler(IQueryOnlyReadModelStore<PushDeviceReadModel> store)
    : IQueryHandler<GetPushDevicesQuery, IReadOnlyCollection<IPushDeviceReadModel>>
{
    public Task<IReadOnlyCollection<IPushDeviceReadModel>> ExecuteQueryAsync(GetPushDevicesQuery query, CancellationToken cancellationToken)
        => store.FindAsync(
            p => p.UserId == query.UserId,
            p => (IPushDeviceReadModel)p,
            cancellationToken: cancellationToken);
}
