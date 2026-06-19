namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class GetWallPaperHandler
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestGetWallPaper, MyTelegram.Schema.IWallPaper>
{
    protected override Task<MyTelegram.Schema.IWallPaper> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestGetWallPaper obj)
    {
        RpcErrors.RpcErrors400.WallpaperInvalid.ThrowRpcError();
        return default!;
    }
}
