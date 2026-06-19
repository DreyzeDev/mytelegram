namespace MyTelegram.Messenger.Handlers.LatestLayer.Account;

internal sealed class GetThemeHandler(IQueryProcessor queryProcessor)
    : RpcResultObjectHandler<MyTelegram.Schema.Account.RequestGetTheme, MyTelegram.Schema.ITheme>
{
    protected override async Task<MyTelegram.Schema.ITheme> HandleCoreAsync(IRequestInput input,
        MyTelegram.Schema.Account.RequestGetTheme obj)
    {
        IThemeReadModel? theme = obj.Theme switch
        {
            TInputTheme t => await queryProcessor.ProcessAsync(new GetThemeByIdQuery(t.Id)),
            TInputThemeSlug s => await queryProcessor.ProcessAsync(new GetThemeBySlugQuery(s.Slug)),
            _ => null
        };

        if (theme == null)
            RpcErrors.RpcErrors400.ThemeInvalid.ThrowRpcError();

        return new TTheme
        {
            Id = theme!.ThemeId,
            AccessHash = 0,
            Slug = theme.Theme.Slug,
            Title = theme.Theme.Title,
            Creator = theme.CreatorUserId == input.UserId,
            Emoticon = theme.Emoticon,
            InstallsCount = 0,
        };
    }
}
