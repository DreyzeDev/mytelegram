namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Get info about a stickerset
/// Possible errors
/// Code Type Description
/// 400 EMOTICON_STICKERPACK_MISSING inputStickerSetDice.emoji cannot be empty.
/// 406 STICKERSET_INVALID The provided sticker set is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getStickerSet"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class GetStickerSetHandler(
    IQueryProcessor queryProcessor,
    ILayeredService<IStickerSetConverter> stickerSetService,
    ILayeredService<IDocumentConverter> documentService)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetStickerSet, MyTelegram.Schema.Messages.IStickerSet>
{
    protected override async Task<MyTelegram.Schema.Messages.IStickerSet> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetStickerSet obj)
    {
        IStickerSetReadModel? stickerSet = obj.Stickerset switch
        {
            TInputStickerSetID byId => await queryProcessor.ProcessAsync(new GetStickerSetByIdQuery(byId.Id)),
            TInputStickerSetShortName byName => await queryProcessor.ProcessAsync(new GetStickerSetByNameQuery(byName.ShortName)),
            _ => null
        };

        if (stickerSet == null)
        {
            RpcErrors.RpcErrors406.StickersetInvalid.ThrowRpcError();
        }

        var documentReadModels = await queryProcessor.ProcessAsync(new GetDocumentsByIdListQuery(stickerSet!.StickerDocumentIds));
        var docConverter = documentService.GetConverter(input.Layer);
        var documents = documentReadModels.Select(d => (Schema.IDocument)docConverter.ToDocument(d)).ToList();

        return stickerSetService.GetConverter(input.Layer).ToMessagesStickerSet(input.UserId, stickerSet, documents);
    }
}