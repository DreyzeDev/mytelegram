using MyTelegram.Schema.Payments;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Payments;

internal sealed class GetSavedStarGiftsHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<RequestGetSavedStarGifts, ISavedStarGifts>
{
    protected override async Task<ISavedStarGifts> HandleCoreAsync(IRequestInput input, RequestGetSavedStarGifts obj)
    {
        var ownerPeerId = GetOwnerPeerId(obj.Peer, input.UserId);

        bool? excludeUnsaved = obj.ExcludeUnsaved ? true : obj.ExcludeSaved ? false : null;
        var offset = int.TryParse(obj.Offset, out var o) ? o : 0;

        var userGifts = await queryProcessor.ProcessAsync(
            new GetUserStarGiftsQuery(ownerPeerId, excludeUnsaved, offset, obj.Limit));

        var savedGifts = new TVector<MyTelegram.Schema.ISavedStarGift>();

        foreach (var ug in userGifts)
        {
            var giftReadModel = await queryProcessor.ProcessAsync(new GetStarGiftByIdQuery(ug.GiftId));
            if (giftReadModel == null) continue;

            var doc = await queryProcessor.ProcessAsync(new GetDocumentByIdQuery(giftReadModel.StickerDocumentId));
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
                : new TDocumentEmpty { Id = giftReadModel.StickerDocumentId };

            var tlGift = new TStarGift
            {
                Id = giftReadModel.GiftId,
                Sticker = sticker,
                Stars = giftReadModel.Stars,
                ConvertStars = giftReadModel.ConvertStars,
                Limited = giftReadModel.Limited,
                SoldOut = giftReadModel.SoldOut
            };

            if (giftReadModel.Limited)
            {
                tlGift.AvailabilityRemains = giftReadModel.AvailabilityRemains;
                tlGift.AvailabilityTotal = giftReadModel.AvailabilityTotal;
            }

            var savedGift = new TSavedStarGift
            {
                Gift = tlGift,
                Date = ug.Date,
                MsgId = ug.MsgId,
                Unsaved = ug.Unsaved,
                NameHidden = ug.NameHidden,
                ConvertStars = ug.ConvertStars > 0 ? ug.ConvertStars : null,
                FromId = ug.SenderPeerId != 0 && !ug.NameHidden
                    ? new TPeerUser { UserId = ug.SenderPeerId }
                    : null
            };

            savedGifts.Add(savedGift);
        }

        return new TSavedStarGifts
        {
            Count = savedGifts.Count,
            Gifts = savedGifts,
            Chats = [],
            Users = []
        };
    }

    private static long GetOwnerPeerId(IInputPeer peer, long selfUserId) => peer switch
    {
        TInputPeerSelf => selfUserId,
        TInputPeerUser u => u.UserId,
        TInputPeerChannel c => c.ChannelId,
        TInputPeerChat ch => ch.ChatId,
        _ => selfUserId
    };
}
