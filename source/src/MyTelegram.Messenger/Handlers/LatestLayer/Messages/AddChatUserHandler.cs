namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Adds a user to a chat and sends a service message on it.
/// Possible errors
/// Code Type Description
/// 400 BOT_GROUPS_BLOCKED This bot can't be added to groups.
/// 403 CHAT_ADMIN_REQUIRED You must be an admin in this chat to do this.
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 CHAT_INVALID Invalid chat.
/// 400 CHAT_MEMBER_ADD_FAILED Could not add participants.
/// 403 CHAT_WRITE_FORBIDDEN You can't write in this chat.
/// 400 INPUT_USER_DEACTIVATED The specified user was deleted.
/// 400 MSG_ID_INVALID Invalid message ID provided.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 USERS_TOO_MUCH The maximum number of users has been exceeded (to create a chat, for example).
/// 400 USER_ALREADY_PARTICIPANT The user is already in the group.
/// 400 USER_ID_INVALID The provided user ID is invalid.
/// 400 USER_IS_BLOCKED You were blocked by this user.
/// 403 USER_NOT_MUTUAL_CONTACT The provided user is not a mutual contact.
/// 403 USER_PRIVACY_RESTRICTED The user's privacy settings do not allow you to do this.
/// 400 YOU_BLOCKED_USER You blocked this user.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.addChatUser"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class AddChatUserHandler : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestAddChatUser, MyTelegram.Schema.Messages.IInvitedUsers>
{
    private readonly ICommandBus _commandBus;
    private readonly IPeerHelper _peerHelper;
    private readonly IRandomHelper _randomHelper;
    private readonly IAccessHashHelper _accessHashHelper;
    private readonly IPrivacyAppService _privacyAppService;
    private readonly IChannelAppService _channelAppService;
    private readonly IChannelAdminRightsChecker _channelAdminRightsChecker;

    public AddChatUserHandler(ICommandBus commandBus, IPeerHelper peerHelper, IRandomHelper randomHelper, IAccessHashHelper accessHashHelper, IPrivacyAppService privacyAppService, IChannelAppService channelAppService, IChannelAdminRightsChecker channelAdminRightsChecker)
    {
        _commandBus = commandBus;
        _peerHelper = peerHelper;
        _randomHelper = randomHelper;
        _accessHashHelper = accessHashHelper;
        _privacyAppService = privacyAppService;
        _channelAppService = channelAppService;
        _channelAdminRightsChecker = channelAdminRightsChecker;
    }

    protected override async Task<MyTelegram.Schema.Messages.IInvitedUsers> HandleCoreAsync(IRequestInput input, RequestAddChatUser obj)
    {
        await _channelAdminRightsChecker.CheckAdminRightAsync(obj.ChatId, input.UserId, adminRights => adminRights.InviteUsers);
        await _accessHashHelper.CheckAccessHashAsync(input, obj.UserId);
        var channelReadModel = await _channelAppService.GetAsync(obj.ChatId);
        channelReadModel.ThrowExceptionIfChannelDeleted();

        var userIds = new List<long>();
        var botUserIds = new List<long>();
        if (obj.UserId is TInputUser inputUser)
        {
            userIds.Add(inputUser.UserId);
        }

        var inviterUserId = input.UserId;
        if (channelReadModel!.Broadcast || channelReadModel.HasLink)
        {
            inviterUserId = MyTelegramConsts.GroupAnonymousBotUserId;
        }

        var command = new StartInviteToChannelCommand(TempId.New, input.ToRequestInfo(), obj.ChatId, channelReadModel.Broadcast, channelReadModel.HasLink, inviterUserId, channelReadModel.TopMessageId, channelReadModel.TopMessageId, userIds, botUserIds, ChatJoinType.InvitedByAdmin);
        await _commandBus.PublishAsync(command);
        return null !;
    }
}
