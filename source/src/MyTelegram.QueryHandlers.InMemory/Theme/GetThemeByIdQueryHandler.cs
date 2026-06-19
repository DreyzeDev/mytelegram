namespace MyTelegram.QueryHandlers.InMemory.Theme;

public class GetThemeByIdQueryHandler(IQueryOnlyReadModelStore<ThemeReadModel> store)
    : IQueryHandler<GetThemeByIdQuery, IThemeReadModel?>
{
    public async Task<IThemeReadModel?> ExecuteQueryAsync(GetThemeByIdQuery query, CancellationToken cancellationToken)
        => await store.FirstOrDefaultAsync(p => p.ThemeId == query.Id, cancellationToken);
}
