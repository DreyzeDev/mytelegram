using MyTelegram.Domain.Aggregates.QrCode;

namespace MyTelegram.ReadModel.Impl;

public class QrCodeReadModel : ReadModelBase, IQrCodeReadModel,
    IAmReadModelFor<QrCodeAggregate, QrCodeId, QrCodeLoginTokenExportedEvent>,
    IAmReadModelFor<QrCodeAggregate, QrCodeId, LoginTokenAcceptedEvent>,
    IAmReadModelFor<QrCodeAggregate, QrCodeId, QrCodeLoginSuccessEvent>
{
    public string Id { get; private set; } = null!;
    public long? Version { get; set; }
    
    public long TempAuthKeyId { get; private set; }
    public long PermAuthKeyId { get; private set; }
    public byte[] Token { get; private set; } = null!;
    public int ExpireDate { get; private set; }
    public bool IsAccepted { get; private set; }
    public long UserId { get; private set; }

    public Task ApplyAsync(IReadModelContext context, IDomainEvent<QrCodeAggregate, QrCodeId, QrCodeLoginTokenExportedEvent> domainEvent, CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        TempAuthKeyId = domainEvent.AggregateEvent.TempAuthKeyId;
        PermAuthKeyId = domainEvent.AggregateEvent.PermAuthKeyId;
        Token = domainEvent.AggregateEvent.Token;
        ExpireDate = domainEvent.AggregateEvent.ExpireDate;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context, IDomainEvent<QrCodeAggregate, QrCodeId, LoginTokenAcceptedEvent> domainEvent, CancellationToken cancellationToken)
    {
        IsAccepted = true;
        UserId = domainEvent.AggregateEvent.UserId;
        return Task.CompletedTask;
    }

    public Task ApplyAsync(IReadModelContext context, IDomainEvent<QrCodeAggregate, QrCodeId, QrCodeLoginSuccessEvent> domainEvent, CancellationToken cancellationToken)
    {
        // Maybe MarkForDeletion here? MTProto requires QR codes to be deleted after use, but let's keep it simple.
        return Task.CompletedTask;
    }
}
