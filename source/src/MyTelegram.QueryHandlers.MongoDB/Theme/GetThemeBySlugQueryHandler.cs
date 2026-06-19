namespace MyTelegram.QueryHandlers.MongoDB.Theme;

public class GetThemeBySlugQueryHandler(IQueryOnlyReadModelStore<ThemeReadModel> store)
    : IQueryHandler<GetThemeBySlugQuery, IThemeReadModel?>
{
    public async Task<IThemeReadModel?> ExecuteQueryAsync(GetThemeBySlugQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.Slug == query.Slug, cancellationToken);
}
