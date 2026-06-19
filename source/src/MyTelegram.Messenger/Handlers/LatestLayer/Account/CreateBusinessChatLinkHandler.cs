namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class CreateBusinessChatLinkHandler(ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestCreateBusinessChatLink, MyTelegram.Schema.IBusinessChatLink>
{
    protected override async Task<MyTelegram.Schema.IBusinessChatLink> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestCreateBusinessChatLink obj)
    {
        if (obj.Link is not TInputBusinessChatLink link)
        {
            RpcErrors.RpcErrors400.InviteSlugEmpty.ThrowRpcError();
            return default!;
        }

        var slug = Guid.NewGuid().ToString("N")[..16];
        var entitiesJson = link.Entities?.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(link.Entities)
            : null;

        var command = new CreateLinkCommand(
            BusinessChatLinkId.Create(input.UserId, slug),
            input.ToRequestInfo(),
            input.UserId,
            slug,
            link.Message ?? string.Empty,
            entitiesJson,
            link.Title);

        await commandBus.PublishAsync(command);

        return new TBusinessChatLink
        {
            Link = $"https://t.me/+{slug}",
            Message = link.Message ?? string.Empty,
            Title = link.Title,
            Views = 0,
        };
    }
}
