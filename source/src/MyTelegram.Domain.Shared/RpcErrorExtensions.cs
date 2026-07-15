namespace MyTelegram;

public static class RpcErrorExtensions
{
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static void ThrowRpcError(this RpcError rpcError, long reqMsgId = 0)
    {
        throw new RpcException(rpcError, reqMsgId);
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static void ThrowRpcError(this RpcError rpcError, int xToReplace, long reqMsgId = 0)
    {
        throw new RpcException(rpcError with { Message = string.Format(rpcError.Message, xToReplace) });
    }
}