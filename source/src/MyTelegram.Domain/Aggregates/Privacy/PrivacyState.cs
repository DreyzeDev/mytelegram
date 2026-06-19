namespace MyTelegram.Domain.Aggregates.Privacy;

public class PrivacyState : AggregateState<PrivacyAggregate, PrivacyId, PrivacyState>,
    IApply<PrivacySetEvent>
{
    public void Apply(PrivacySetEvent aggregateEvent) { }
}
