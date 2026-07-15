namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Get a document by its SHA256 hash, mainly used for gifs
/// Possible errors
/// Code Type Description
/// 400 SHA256_HASH_INVALID The provided SHA256 hash is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getDocumentByHash"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class GetDocumentByHashHandler(
    IQueryProcessor queryProcessor,
    ILayeredService<IDocumentConverter> documentService)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetDocumentByHash, MyTelegram.Schema.IDocument>
{
    protected override async Task<MyTelegram.Schema.IDocument> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetDocumentByHash obj)
    {
        var documentReadModel = await queryProcessor.ProcessAsync(new GetDocumentByHashQuery(obj.Sha256.ToArray(), obj.Size, obj.MimeType));
        if (documentReadModel == null)
        {
            // Per corefork docs, messages.getDocumentByHash returns documentEmpty when no matching document is found.
            return new TDocumentEmpty();
        }

        var docConverter = documentService.GetConverter(input.Layer);
        return docConverter.ToDocument(documentReadModel);
    }
}