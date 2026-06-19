namespace MyTelegram.Domain.Aggregates.Privacy;

[EnableAutoGeneration]
public class PrivacyAggregate : AggregateRoot<PrivacyAggregate, PrivacyId>
{
    private readonly PrivacyState _state = new();

    public PrivacyAggregate(PrivacyId id) : base(id)
    {
        Register(_state);
    }

    public void SetPrivacy(long userId, PrivacyType privacyType, List<PrivacyValueData> privacyValueDataList)
    {
        Emit(new PrivacySetEvent(userId, privacyType, privacyValueDataList));
    }
}
