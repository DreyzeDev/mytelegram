namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Get stickers by emoji
/// Possible errors
/// Code Type Description
/// 400 EMOTICON_EMPTY The emoji is empty.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getStickers"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetStickersHandler(
    IQueryProcessor queryProcessor,
    ILayeredService<IDocumentConverter> documentService)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetStickers, MyTelegram.Schema.Messages.IStickers>
{
    protected override async Task<IStickers> HandleCoreAsync(IRequestInput input, RequestGetStickers obj)
    {
        if (string.IsNullOrEmpty(obj.Emoticon))
        {
            RpcErrors.RpcErrors400.EmoticonEmpty.ThrowRpcError();
        }

        var allSets = await queryProcessor.ProcessAsync(new GetAllStickerSetsQuery());
        var matchingDocumentIds = allSets
            .SelectMany(s => s.Packs)
            .Where(p => p.Emoticon == obj.Emoticon)
            .SelectMany(p => p.Documents)
            .Distinct()
            .ToList();

        if (matchingDocumentIds.Count == 0)
        {
            return new TStickers { Hash = obj.Hash, Stickers = [] };
        }

        var docReadModels = await queryProcessor.ProcessAsync(new GetDocumentsByIdListQuery(matchingDocumentIds));
        var docConverter = documentService.GetConverter(input.Layer);
        var stickers = docReadModels.Select(d => (Schema.IDocument)docConverter.ToDocument(d)).ToList();

        return new TStickers { Hash = obj.Hash, Stickers = new TVector<Schema.IDocument>(stickers) };
    }
}