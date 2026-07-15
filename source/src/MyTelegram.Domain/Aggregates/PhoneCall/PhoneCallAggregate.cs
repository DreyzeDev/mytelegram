namespace MyTelegram.Domain.Aggregates.PhoneCall;

[EnableAutoGeneration]
public class PhoneCallAggregate : AggregateRoot<PhoneCallAggregate, PhoneCallId>
{
    private readonly PhoneCallState _state = new();

    public PhoneCallAggregate(PhoneCallId id) : base(id)
    {
        Register(_state);
    }

    public void CreatePhoneCall(long callId, long accessHash, long callerId, long calleeId,
        byte[] gaHash, PhoneCallProtocol protocol, bool isVideo, int date)
    {
        if (IsNew)
            Emit(new PhoneCallCreatedEvent(callId, accessHash, callerId, calleeId, gaHash, protocol, isVideo, date));
    }

    public void AcceptPhoneCall(byte[] gb, PhoneCallProtocol protocol, int date)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new PhoneCallAcceptedEvent(gb, protocol, date));
    }

    public void ConfirmPhoneCall(byte[] ga, long keyFingerprint, PhoneCallProtocol protocol, int date)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new PhoneCallConfirmedEvent(ga, keyFingerprint, protocol, date));
    }

    public void DiscardPhoneCall(string? reasonType, int duration, bool isVideo)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new PhoneCallDiscardedEvent(reasonType, duration, isVideo));
    }
}
