namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class UpdateThemeHandler(ICommandBus commandBus, IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestUpdateTheme, MyTelegram.Schema.ITheme>
{
    protected override async Task<MyTelegram.Schema.ITheme> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestUpdateTheme obj)
    {
        IThemeReadModel? theme = obj.Theme switch
        {
            TInputTheme t => await queryProcessor.ProcessAsync(new GetThemeByIdQuery(t.Id)),
            TInputThemeSlug s => await queryProcessor.ProcessAsync(new GetThemeBySlugQuery(s.Slug)),
            _ => null
        };

        if (theme == null)
            RpcErrors.RpcErrors400.ThemeInvalid.ThrowRpcError();

        var settings = obj.Settings?
            .OfType<TInputThemeSettings>()
            .Select(s => new ThemeSettings(
                s.MessageColorsAnimated,
                (long)s.BaseTheme.ConstructorId,
                s.AccentColor,
                s.OutboxAccentColor,
                s.MessageColors?.ToList(),
                null))
            .ToList();

        var command = new UpdateThemeCommand(
            ThemeId.Create(theme!.CreatorUserId, theme.Theme.Slug),
            input.ToRequestInfo(),
            obj.Slug,
            obj.Title,
            null,
            settings,
            null);

        await commandBus.PublishAsync(command);

        var newSlug = obj.Slug ?? theme.Theme.Slug;
        var newTitle = obj.Title ?? theme.Theme.Title;

        return new TTheme
        {
            Id = theme.ThemeId,
            AccessHash = 0,
            Slug = newSlug,
            Title = newTitle,
            Creator = theme.CreatorUserId == input.UserId,
            Emoticon = theme.Emoticon,
            InstallsCount = 0,
        };
    }
}
