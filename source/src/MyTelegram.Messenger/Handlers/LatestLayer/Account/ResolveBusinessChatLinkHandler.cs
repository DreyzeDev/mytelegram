namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class ResolveBusinessChatLinkHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestResolveBusinessChatLink, MyTelegram.Schema.Account.IResolvedBusinessChatLinks>
{
    protected override async Task<MyTelegram.Schema.Account.IResolvedBusinessChatLinks> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestResolveBusinessChatLink obj)
    {
        if (string.IsNullOrEmpty(obj.Slug))
        {
            RpcErrors.RpcErrors400.InviteSlugEmpty.ThrowRpcError();
            return default!;
        }

        var link = await queryProcessor.ProcessAsync(new GetBusinessChatLinkBySlugQuery(obj.Slug));
        if (link == null)
        {
            RpcErrors.RpcErrors400.InviteSlugExpired.ThrowRpcError();
            return default!;
        }

        return new TResolvedBusinessChatLinks
        {
            Peer = new TPeerUser { UserId = link.UserId },
            Message = link.Message,
            Chats = [],
            Users = [],
        };
    }
}
