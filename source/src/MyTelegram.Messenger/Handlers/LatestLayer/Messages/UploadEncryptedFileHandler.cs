namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Upload encrypted file and associate it to a secret chat (without actually sending it to the chat).
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.uploadEncryptedFile"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class UploadEncryptedFileHandler
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestUploadEncryptedFile, MyTelegram.Schema.IEncryptedFile>
{
    protected override Task<MyTelegram.Schema.IEncryptedFile> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestUploadEncryptedFile obj)
    {
        // The file is already uploaded via upload.saveFilePart; here we register it to the secret chat.
        // Return encryptedFile so the client can embed it in sendEncryptedFile.
        IEncryptedFile result = obj.File switch
        {
            TInputEncryptedFileUploaded f => new TEncryptedFile
            {
                Id = f.Id,
                AccessHash = Random.Shared.NextInt64(),
                Size = 0,
                DcId = 1,
                KeyFingerprint = f.KeyFingerprint
            },
            TInputEncryptedFileBigUploaded f => new TEncryptedFile
            {
                Id = f.Id,
                AccessHash = Random.Shared.NextInt64(),
                Size = 0,
                DcId = 1,
                KeyFingerprint = f.KeyFingerprint
            },
            _ => new TEncryptedFileEmpty()
        };
        return Task.FromResult(result);
    }
}