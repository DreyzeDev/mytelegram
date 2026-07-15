namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Mark one or more items of a <a href="https://corefork.telegram.org/api/todo">todo list »</a> as completed or not completed.
/// Possible errors
/// Code Type Description
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.toggleTodoCompleted"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class ToggleTodoCompletedHandler : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestToggleTodoCompleted, MyTelegram.Schema.IUpdates>
{
    protected override Task<MyTelegram.Schema.IUpdates> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestToggleTodoCompleted obj)
    {
        // Todo lists (https://corefork.telegram.org/api/todo) only store their items via TMessageMediaToDo on the
        // message (see AppendTodoListHandler), there is no per-item "completed" state tracked anywhere in
        // MyTelegram.Domain/the read models. Rather than silently no-op or fake success (which would desync the
        // client), throw the one error this RPC documents (PEER_ID_INVALID) until completion tracking is built.
        RpcErrors.RpcErrors400.PeerIdInvalid.ThrowRpcError();
        return null!;
    }
}