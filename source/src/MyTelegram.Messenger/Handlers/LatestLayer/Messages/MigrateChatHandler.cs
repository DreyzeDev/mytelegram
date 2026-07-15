namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Turn a <a href="https://corefork.telegram.org/api/channel#migration">basic group into a supergroup</a>
/// Possible errors
/// Code Type Description
/// 400 CHANNELS_TOO_MUCH You have joined too many channels/supergroups.
/// 403 CHAT_ADMIN_REQUIRED You must be an admin in this chat to do this.
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 500 CHAT_INVALID Invalid chat.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.migrateChat"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class MigrateChatHandler(IChannelAppService channelAppService, IChannelAdminRightsChecker channelAdminRightsChecker, IChatConverterService chatConverterService, IPhotoAppService photoAppService, IQueryProcessor queryProcessor) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestMigrateChat, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestMigrateChat obj)
    {
        var channelReadModel = await channelAppService.GetAsync(obj.ChatId);
        channelReadModel.ThrowExceptionIfChannelDeleted();
        await channelAdminRightsChecker.CheckAdminRightAsync(obj.ChatId, input.UserId, adminRights => adminRights.ChangeInfo, RpcErrors.RpcErrors403.ChatAdminRequired);

        // Basic chats are already internally represented as (mega-group) channels in this server,
        // so there is no structural migration to perform - we simply hand back the channel as-is.
        var channelMemberReadModels = await queryProcessor.ProcessAsync(new GetChannelMemberListByChannelIdListQuery(input.UserId, [channelReadModel!.ChannelId]));
        var photoReadModels = await photoAppService.GetPhotosAsync([channelReadModel]);
        var channelList = chatConverterService.ToChannelList(input, [channelReadModel], photoReadModels, channelMemberReadModels, layer: input.Layer);

        return new TUpdates
        {
            Updates = [],
            Users = [],
            Chats = [..channelList],
            Date = CurrentDate,
            Seq = 0
        };
    }
}
