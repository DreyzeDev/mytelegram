using MyTelegram.Domain.Aggregates.PhoneCall;

namespace MyTelegram.ReadModel.Impl;

public class PhoneCallReadModel : ReadModelBase, IPhoneCallReadModel,
    IAmReadModelFor<PhoneCallAggregate, PhoneCallId, PhoneCallCreatedEvent>,
    IAmReadModelFor<PhoneCallAggregate, PhoneCallId, PhoneCallAcceptedEvent>,
    IAmReadModelFor<PhoneCallAggregate, PhoneCallId, PhoneCallConfirmedEvent>,
    IAmReadModelFor<PhoneCallAggregate, PhoneCallId, PhoneCallDiscardedEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
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

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<PhoneCallAggregate, PhoneCallId, PhoneCallCreatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        var e = domainEvent.AggregateEvent;
        CallId = e.CallId;
        AccessHash = e.AccessHash;
        CallerId = e.CallerId;
        CalleeId = e.CalleeId;
        GaHash = e.GaHash;
        Protocol = e.Protocol;
        IsVideo = e.IsVideo;
        Date = e.Date;
        CallState = "created";
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<PhoneCallAggregate, PhoneCallId, PhoneCallAcceptedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Gb = e.Gb;
        Protocol = e.Protocol;
        CallState = "accepted";
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<PhoneCallAggregate, PhoneCallId, PhoneCallConfirmedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        Ga = e.Ga;
        KeyFingerprint = e.KeyFingerprint;
        Protocol = e.Protocol;
        CallState = "confirmed";
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<PhoneCallAggregate, PhoneCallId, PhoneCallDiscardedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        CallState = "discarded";
        return Task.CompletedTask;
    }
}
