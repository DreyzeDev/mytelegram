namespace MyTelegram.ReadModel.Impl;

public class UserPasswordReadModel : ReadModelBase, IUserPasswordReadModel,
    IAmReadModelFor<UserAggregate, UserId, UserPasswordUpdatedEvent>
{
    public virtual string Id { get; set; } = null!;
    public virtual long UserId { get; private set; }
    public virtual bool HasPassword { get; private set; }
    public virtual string? Hint { get; private set; }
    public virtual string? Email { get; private set; }
    public virtual string? UnconfirmedEmail { get; private set; }
    public virtual bool IsEmailConfirmed { get; private set; }
    public virtual byte[] PasswordHash { get; private set; } = [];
    public virtual byte[] Salt1 { get; private set; } = [];
    public virtual byte[] Salt2 { get; private set; } = [];
    public virtual int G { get; private set; }
    public virtual byte[] P { get; private set; } = [];
    public virtual SrpData SrpData { get; private set; } = null!;
    public virtual long SrpId { get; private set; }
    public virtual long? Version { get; set; }

    public Task ApplyAsync(IReadModelContext context,
        IDomainEvent<UserAggregate, UserId, UserPasswordUpdatedEvent> domainEvent,
        CancellationToken cancellationToken)
    {
        Id = domainEvent.AggregateIdentity.Value;
        UserId = domainEvent.AggregateEvent.UserId;
        HasPassword = domainEvent.AggregateEvent.HasPassword;

        var ps = domainEvent.AggregateEvent.PasswordSetting;
        if (ps != null)
        {
            Hint = ps.Hint;
            Email = ps.Email;
            PasswordHash = ps.NewPasswordHash;
            Salt1 = ps.NewAlgo.Salt1;
            Salt2 = ps.NewAlgo.Salt2;
            G = ps.NewAlgo.G;
            P = ps.NewAlgo.P;
        }
        else
        {
            Hint = null;
            Email = null;
            PasswordHash = [];
            Salt1 = [];
            Salt2 = [];
            G = 0;
            P = [];
        }

        SrpData = null!;
        SrpId = 0;

        return Task.CompletedTask;
    }
}
