using MyTelegram.Domain.Aggregates.PeerSetting;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Show or hide the <a href="https://corefork.telegram.org/api/translation">real-time chat translation popup</a> for a certain chat
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.togglePeerTranslations"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class TogglePeerTranslationsHandler(IPeerHelper peerHelper, ICommandBus commandBus) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestTogglePeerTranslations, IBool>
{
    protected override async Task<IBool> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestTogglePeerTranslations obj)
    {
        var peer = peerHelper.GetPeer(obj.Peer, input.UserId);
        var command = new TogglePeerTranslationsCommand(PeerSettingsId.Create(input.UserId, peer.PeerId), input.ToRequestInfo(), peer.PeerId, obj.Disabled);
        await commandBus.PublishAsync(command);

        return new TBoolTrue();
    }
}