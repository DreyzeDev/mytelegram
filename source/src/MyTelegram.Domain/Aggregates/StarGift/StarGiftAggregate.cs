namespace MyTelegram.Domain.Aggregates.StarGift;

[EnableAutoGeneration]
public class StarGiftAggregate : AggregateRoot<StarGiftAggregate, StarGiftId>
{
    private readonly StarGiftState _state = new();

    public StarGiftAggregate(StarGiftId id) : base(id)
    {
        Register(_state);
    }

    public void CreateStarGift(long giftId, long stars, long convertStars, int availabilityTotal, long stickerDocumentId)
    {
        if (IsNew)
            Emit(new StarGiftCreatedEvent(giftId, stars, convertStars, availabilityTotal, stickerDocumentId));
    }

    public void UpdateAvailability(int availabilityRemains)
    {
        Specs.AggregateIsCreated.ThrowDomainErrorIfNotSatisfied(this);
        Emit(new StarGiftAvailabilityUpdatedEvent(availabilityRemains));
    }
}
