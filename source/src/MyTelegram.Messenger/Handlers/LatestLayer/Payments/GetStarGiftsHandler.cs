using MyTelegram.Schema.Payments;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Payments;

internal sealed class GetStarGiftsHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<RequestGetStarGifts, IStarGifts>
{
    protected override async Task<IStarGifts> HandleCoreAsync(IRequestInput input, RequestGetStarGifts obj)
    {
        var gifts = await queryProcessor.ProcessAsync(new GetAllStarGiftsQuery());
        var starGifts = new TVector<MyTelegram.Schema.IStarGift>();

        foreach (var g in gifts)
        {
            var doc = await queryProcessor.ProcessAsync(new GetDocumentByIdQuery(g.StickerDocumentId));
            IDocument sticker = doc != null
                ? new TDocument
                {
                    Id = doc.DocumentId,
                    AccessHash = doc.AccessHash,
                    FileReference = doc.FileReference.ToArray(),
                    Date = doc.Date,
                    MimeType = doc.MimeType,
                    Size = doc.Size,
                    DcId = doc.DcId,
                    Attributes = doc.Attributes2 != null
                        ? [.. doc.Attributes2]
                        : doc.Attributes.ToTObject<TVector<IDocumentAttribute>>()
                }
                : new TDocumentEmpty { Id = g.StickerDocumentId };

            var gift = new TStarGift
            {
                Id = g.GiftId,
                Sticker = sticker,
                Stars = g.Stars,
                ConvertStars = g.ConvertStars,
                Limited = g.Limited,
                SoldOut = g.SoldOut
            };

            if (g.Limited)
            {
                gift.AvailabilityRemains = g.AvailabilityRemains;
                gift.AvailabilityTotal = g.AvailabilityTotal;
            }

            starGifts.Add(gift);
        }

        return new TStarGifts { Gifts = starGifts, Chats = [], Users = [] };
    }
}
