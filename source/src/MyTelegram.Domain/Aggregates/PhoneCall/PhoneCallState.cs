namespace MyTelegram.Domain.Aggregates.PhoneCall;

public class PhoneCallState : AggregateState<PhoneCallAggregate, PhoneCallId, PhoneCallState>,
    IApply<PhoneCallCreatedEvent>,
    IApply<PhoneCallAcceptedEvent>,
    IApply<PhoneCallConfirmedEvent>,
    IApply<PhoneCallDiscardedEvent>
{
    public long CallId { get; private set; }
    public long AccessHash { get; private set; }
    public long CallerId { get; private set; }
    public long CalleeId { get; private set; }
    public byte[] GaHash { get; private set; } = [];
    public PhoneCallProtocol Protocol { get; private set; } = new(true, true, 86, 92, []);
    public bool IsVideo { get; private set; }
    public int Date { get; private set; }
    public byte[]? Gb { get; private set; }
    public byte[]? Ga { get; private set; }
    public long KeyFingerprint { get; private set; }
    public string CallState { get; private set; } = "created";

    public void Apply(PhoneCallCreatedEvent e)
    {
        CallId = e.CallId;
        AccessHash = e.AccessHash;
        CallerId = e.CallerId;
        CalleeId = e.CalleeId;
        GaHash = e.GaHash;
        Protocol = e.Protocol;
        IsVideo = e.IsVideo;
        Date = e.Date;
        CallState = "created";
    }

    public void Apply(PhoneCallAcceptedEvent e)
    {
        Gb = e.Gb;
        Protocol = e.Protocol;
        CallState = "accepted";
    }

    public void Apply(PhoneCallConfirmedEvent e)
    {
        Ga = e.Ga;
        KeyFingerprint = e.KeyFingerprint;
        Protocol = e.Protocol;
        CallState = "confirmed";
    }

    public void Apply(PhoneCallDiscardedEvent e)
    {
        CallState = "discarded";
    }
}
