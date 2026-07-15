using MyTelegram.Domain.Aggregates.PeerSetting;
using WallPaperSettings = MyTelegram.WallPaperSettings;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Set a custom <a href="https://corefork.telegram.org/api/wallpapers">wallpaper »</a> in a specific private chat with another user.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 WALLPAPER_INVALID The specified wallpaper is invalid.
/// 400 WALLPAPER_NOT_FOUND The specified wallpaper could not be found.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.setChatWallPaper"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class SetChatWallPaperHandler(IPeerHelper peerHelper, ICommandBus commandBus) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestSetChatWallPaper, MyTelegram.Schema.IUpdates>
{
    protected override async Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestSetChatWallPaper obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);

        if (obj.Revert)
        {
            var revertCommand = new SetChatWallPaperCommand(PeerSettingsId.Create(input.UserId, peer.PeerId), input.ToRequestInfo(), peer.PeerId, null, null, null, null, false, true);
            await commandBus.PublishAsync(revertCommand);
            return null!;
        }

        long? wallPaperId = null;
        long? wallPaperAccessHash = null;
        string? wallPaperSlug = null;

        switch (obj.Wallpaper)
        {
            case TInputWallPaper inputWallPaper:
                wallPaperId = inputWallPaper.Id;
                wallPaperAccessHash = inputWallPaper.AccessHash;
                break;
            case TInputWallPaperNoFile inputWallPaperNoFile:
                wallPaperId = inputWallPaperNoFile.Id;
                break;
            case TInputWallPaperSlug inputWallPaperSlug:
                wallPaperSlug = inputWallPaperSlug.Slug;
                break;
        }

        var settings = obj.Settings is { } s
            ? new WallPaperSettings(
                s.Blur,
                s.Motion,
                s.BackgroundColor,
                s.SecondBackgroundColor,
                s.ThirdBackgroundColor,
                s.FourthBackgroundColor,
                s.Intensity,
                s.Rotation,
                s.Emoticon)
            : null;

        var command = new SetChatWallPaperCommand(PeerSettingsId.Create(input.UserId, peer.PeerId), input.ToRequestInfo(), peer.PeerId, wallPaperId, wallPaperAccessHash, wallPaperSlug, settings, obj.ForBoth, false);
        await commandBus.PublishAsync(command);

        return null!;
    }
}