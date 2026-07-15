namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class EditBusinessChatLinkHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestEditBusinessChatLink, MyTelegram.Schema.IBusinessChatLink>
{
    protected override async Task<MyTelegram.Schema.IBusinessChatLink> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestEditBusinessChatLink obj)
    {
        if (string.IsNullOrEmpty(obj.Slug))
        {
            RpcErrors.RpcErrors400.InviteSlugEmpty.ThrowRpcError();
            return default!;
        }

        var existing = await queryProcessor.ProcessAsync(new GetBusinessChatLinkBySlugQuery(obj.Slug));
        if (existing == null || existing.UserId != input.UserId)
        {
            RpcErrors.RpcErrors400.InviteSlugExpired.ThrowRpcError();
            return default!;
        }

        if (obj.Link is not TInputBusinessChatLink link)
        {
            RpcErrors.RpcErrors400.InviteSlugEmpty.ThrowRpcError();
            return default!;
        }

        var entitiesJson = link.Entities?.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(link.Entities)
            : null;

        var command = new EditLinkCommand(
            BusinessChatLinkId.Create(input.UserId, obj.Slug),
            link.Message ?? existing.Message,
            entitiesJson,
            link.Title);

        await commandBus.PublishAsync(command);

        return new TBusinessChatLink
        {
            Link = $"https://t.me/+{obj.Slug}",
            Message = link.Message ?? existing.Message,
            Title = link.Title,
            Views = existing.Views,
        };
    }
}
