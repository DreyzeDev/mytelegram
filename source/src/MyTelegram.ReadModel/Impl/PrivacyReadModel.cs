namespace MyTelegram.ReadModel.Impl;

public class PrivacyReadModel : ReadModelBase, IPrivacyReadModel,
    IAmReadModelFor<PrivacyAggregate, PrivacyId, PrivacySetEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    public long UserId { get; private set; }
    public PrivacyType PrivacyType { get; private set; }
    public IReadOnlyList<PrivacyValueData> PrivacyValueDataList { get; private set; } = [];

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<PrivacyAggregate, PrivacyId, PrivacySetEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Id = domainEvent.AggregateIdentity.Value;
        UserId = e.UserId;
        PrivacyType = e.PrivacyType;
        PrivacyValueDataList = e.PrivacyValueDataList;
        return Task.CompletedTask;
    }
}
