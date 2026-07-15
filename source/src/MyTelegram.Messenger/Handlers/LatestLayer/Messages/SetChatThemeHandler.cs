using MyTelegram.Domain.Aggregates.PeerSetting;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Change the chat theme of a certain chat, see <a href="https://corefork.telegram.org/api/themes#chat-themes">here »</a> for more info.
/// Possible errors
/// Code Type Description
/// 400 EMOJI_INVALID The specified theme emoji is valid.
/// 400 EMOJI_NOT_MODIFIED The theme wasn't changed.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.setChatTheme"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SetChatThemeHandler(IPeerHelper peerHelper, ICommandBus commandBus) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSetChatTheme, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestSetChatTheme obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);

        string? themeEmoji = null;
        string? themeGiftSlug = null;
        switch (obj.Theme)
        {
            case TInputChatThemeEmpty:
                break;
            case TInputChatTheme inputChatTheme:
                themeEmoji = inputChatTheme.Emoticon;
                break;
            case TInputChatThemeUniqueGift inputChatThemeUniqueGift:
                themeGiftSlug = inputChatThemeUniqueGift.Slug;
                break;
        }

        var command = new SetChatThemeCommand(PeerSettingsId.Create(input.UserId, peer.PeerId), input.ToRequestInfo(), peer.PeerId, themeEmoji, themeGiftSlug);
        await commandBus.PublishAsync(command);

        return null!;
    }
}