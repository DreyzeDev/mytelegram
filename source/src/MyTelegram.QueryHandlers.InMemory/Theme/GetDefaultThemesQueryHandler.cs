namespace MyTelegram.QueryHandlers.InMemory.Theme;

public class GetDefaultThemesQueryHandler(IQueryOnlyReadModelStore<ThemeReadModel> store)
    : IQueryHandler<GetDefaultThemesQuery, IReadOnlyCollection<IThemeReadModel>>
{
    public async Task<IReadOnlyCollection<IThemeReadModel>> ExecuteQueryAsync(GetDefaultThemesQuery query, CancellationToken cancellationToken)
        => await store.FindAsync(_ => true, cancellationToken: cancellationToken);
}
