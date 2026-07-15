namespace MyTelegram.Messenger.Handlers.LatestLayer.Auth;

/// <summary>
/// Login as a bot
/// Possible errors
/// Code Type Description
/// 400 ACCESS_TOKEN_EXPIRED Access token expired.
/// 400 ACCESS_TOKEN_INVALID Access token invalid.
/// 400 API_ID_INVALID API ID invalid.
/// 400 API_ID_PUBLISHED_FLOOD This API id was published somewhere, you can't use it now.
/// <para><c>See <a href="https://corefork.telegram.org/method/auth.importBotAuthorization"/></c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✔]
/// </remarks>
internal sealed class ImportBotAuthorizationHandler(
    IQueryProcessor queryProcessor,
    IEventBus eventBus,
    IUserAppService userAppService,
    ILayeredService<IAuthorizationConverter> layeredService,
    IUserConverterService userConverterService,
    IPhotoAppService photoAppService,
    ILogger<ImportBotAuthorizationHandler> logger) 
    : RpcResultObjectHandler<MyTelegram.Schema.Auth.RequestImportBotAuthorization, MyTelegram.Schema.Auth.IAuthorization>
{
    protected override async Task<MyTelegram.Schema.Auth.IAuthorization> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Auth.RequestImportBotAuthorization obj)
    {
        var bot = await queryProcessor.ProcessAsync(new GetBotByTokenQuery(obj.BotAuthToken), default);
        
        if (bot == null)
        {
            RpcErrors.RpcErrors400.AccessTokenInvalid.ThrowRpcError();
        }

        var userId = bot!.UserId;
        var userReadModel = await userAppService.GetAsync(userId);
        
        if (userReadModel == null)
        {
            RpcErrors.RpcErrors400.AccessTokenInvalid.ThrowRpcError();
        }

        await eventBus.PublishAsync(new UserSignUpSuccessIntegrationEvent(
            input.AuthKeyId,
            input.PermAuthKeyId,
            userId));

        var photos = await photoAppService.GetPhotosAsync(userReadModel);
        ILayeredUser user = userConverterService.ToUser(input, userReadModel, photos, layer: input.Layer);
        user.Self = true;

        return layeredService.GetConverter(input.Layer).CreateAuthorization(user);
    }
}