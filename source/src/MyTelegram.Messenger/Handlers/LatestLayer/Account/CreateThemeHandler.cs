namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class CreateThemeHandler(ICommandBus commandBus, IIdGenerator idGenerator)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestCreateTheme, MyTelegram.Schema.ITheme>
{
    protected override async Task<MyTelegram.Schema.ITheme> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestCreateTheme obj)
    {
        if (string.IsNullOrWhiteSpace(obj.Title))
            RpcErrors.RpcErrors400.ThemeTitleInvalid.ThrowRpcError();

        var slug = string.IsNullOrWhiteSpace(obj.Slug)
            ? Guid.NewGuid().ToString("N")[..16]
            : obj.Slug;

        var themeId = await idGenerator.NextLongIdAsync(IdType.GlobalMessageId, input.UserId);

        var settings = obj.Settings?
            .OfType<TInputThemeSettings>()
            .Select(s => new ThemeSettings(
                s.MessageColorsAnimated,
                (long)s.BaseTheme.ConstructorId,
                s.AccentColor,
                s.OutboxAccentColor,
                s.MessageColors?.ToList(),
                null))
            .ToList() ?? [];

        var command = new CreateThemeCommand(
            ThemeId.Create(input.UserId, slug),
            input.ToRequestInfo(),
            input.UserId,
            themeId,
            slug,
            obj.Title,
            null,
            settings,
            null,
            "tdesktop");

        await commandBus.PublishAsync(command);

        return new TTheme
        {
            Id = themeId,
            AccessHash = 0,
            Slug = slug,
            Title = obj.Title,
            Creator = true,
            InstallsCount = 0,
        };
    }
}
