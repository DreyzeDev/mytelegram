namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Returns chat basic info on their IDs.
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getChats"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class GetChatsHandler(IQueryProcessor queryProcessor, IPhotoAppService photoAppService, IChatConverterService chatConverterService) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetChats, MyTelegram.Schema.Messages.IChats>
{
    protected override async Task<MyTelegram.Schema.Messages.IChats> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestGetChats obj)
    {
        var channelIds = obj.Id.Distinct().ToList();
        var channelReadModels = await queryProcessor.ProcessAsync(new GetChannelByChannelIdListQuery(channelIds));
        var photoReadModels = await photoAppService.GetPhotosAsync(channelReadModels);
        var channelMemberReadModels = await queryProcessor.ProcessAsync(new GetChannelMemberListByChannelIdListQuery(input.UserId, [..channelReadModels.Select(p => p.ChannelId)]));
        var channels = chatConverterService.ToChannelList(input, channelReadModels, photoReadModels, channelMemberReadModels, layer: input.Layer);
        return new TChats
        {
            Chats = [..channels]
        };
    }
}
