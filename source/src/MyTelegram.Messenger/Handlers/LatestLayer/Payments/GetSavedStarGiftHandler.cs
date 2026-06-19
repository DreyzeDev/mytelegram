using MyTelegram.Schema.Payments;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Payments;

internal sealed class GetSavedStarGiftHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<RequestGetSavedStarGift, ISavedStarGifts>
{
    protected override async Task<ISavedStarGifts> HandleCoreAsync(IRequestInput input, RequestGetSavedStarGift obj)
    {
        var savedGifts = new TVector<MyTelegram.Schema.ISavedStarGift>();

        foreach (var giftInput in obj.Stargift)
        {
            IUserStarGiftReadModel? ug = null;
            switch (giftInput)
            {
                case TInputSavedStarGiftUser userGift:
                    ug = await queryProcessor.ProcessAsync(
                        new GetUserStarGiftByMsgIdQuery(input.UserId, userGift.MsgId));
                    break;
                case TInputSavedStarGiftChat chatGift:
                    if (chatGift.SavedId != 0)
                        ug = await queryProcessor.ProcessAsync(
                            new GetUserStarGiftByMsgIdQuery(chatGift.Peer is TInputPeerChannel c ? c.ChannelId :
                                chatGift.Peer is TInputPeerUser u ? u.UserId : input.UserId,
                                (int)chatGift.SavedId));
                    break;
            }

            if (ug == null) continue;

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

            savedGifts.Add(new TSavedStarGift
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
            });
        }

        return new TSavedStarGifts { Count = savedGifts.Count, Gifts = savedGifts, Chats = [], Users = [] };
    }
}
