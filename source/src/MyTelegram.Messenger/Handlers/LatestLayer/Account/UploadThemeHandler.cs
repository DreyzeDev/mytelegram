namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class UploadThemeHandler
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestUploadTheme, MyTelegram.Schema.IDocument>
{
    protected override Task<MyTelegram.Schema.IDocument> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestUploadTheme obj)
    {
        RpcErrors.RpcErrors400.ThemeFileInvalid.ThrowRpcError();
        return default!;
    }
}
