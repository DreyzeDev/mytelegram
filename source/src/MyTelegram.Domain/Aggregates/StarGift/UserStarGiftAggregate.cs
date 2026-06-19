namespace MyTelegram.Domain.Aggregates.StarGift;

[EnableAutoGeneration]
public class UserStarGiftAggregate : AggregateRoot<UserStarGiftAggregate, UserStarGiftId>
{
    private readonly UserStarGiftState _state = new();

    public UserStarGiftAggregate(UserStarGiftId id) : base(id)
    {
        Register(_state);
    }

    public void ReceiveGift(long ownerPeerId, int msgId, long giftId, long senderPeerId, int date,
        long convertStars, bool nameHidden)
    {
        if (IsNew)
            Emit(new UserStarGiftReceivedEvent(ownerPeerId, msgId, giftId, senderPeerId, date, convertStars, nameHidden));
    }

    public void SaveGift(bool unsaved)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new UserStarGiftSavedEvent(unsaved));
    }

    public void ConvertGift(long requesterId)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        if (_state.Converted)
            RpcErrors.RpcErrors400.MessageIdInvalid.ThrowRpcError();
        if (_state.OwnerPeerId != requesterId)
            RpcErrors.RpcErrors400.StargiftOwnerInvalid.ThrowRpcError();
        Emit(new UserStarGiftConvertedEvent(requesterId));
    }
}
