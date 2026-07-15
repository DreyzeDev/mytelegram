namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Approve or reject a <a href="https://corefork.telegram.org/api/suggested-posts">suggested post »</a>.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.toggleSuggestedPostApproval"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class ToggleSuggestedPostApprovalHandler : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestToggleSuggestedPostApproval, MyTelegram.Schema.IUpdates>
{
    protected override Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestToggleSuggestedPostApproval obj)
    {
        // Suggested posts (https://corefork.telegram.org/api/suggested-posts) are not modelled anywhere in
        // MyTelegram.Domain yet: there is no aggregate/read-model state tracking a message as a "suggested post"
        // or its approval status, so there is nothing here to approve/reject. Throw the one error this RPC
        // documents (PEER_ID_INVALID) instead of faking success or leaving a NotImplementedException.
        RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        return null!;
    }
}