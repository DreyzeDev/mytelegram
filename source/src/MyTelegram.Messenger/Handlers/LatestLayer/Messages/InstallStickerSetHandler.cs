using MyTelegram.Domain.Aggregates.InstalledStickerSet;

namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Install a stickerset
/// Possible errors
/// Code Type Description
/// 406 STICKERSET_INVALID The provided sticker set is invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.installStickerSet"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class InstallStickerSetHandler(
    IQueryProcessor queryProcessor,
    ICommandBus commandBus)
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestInstallStickerSet, MyTelegram.Schema.Messages.IStickerSetInstallResult>
{
    protected override async Task<MyTelegram.Schema.Messages.IStickerSetInstallResult> HandleCoreAsync(IRequestInput input, MyTelegram.Schema.Messages.RequestInstallStickerSet obj)
    {
        IStickerSetReadModel? stickerSet = obj.Stickerset switch
        {
            TInputStickerSetID byId => await queryProcessor.ProcessAsync(new GetStickerSetByIdQuery(byId.Id)),
            TInputStickerSetShortName byName => await queryProcessor.ProcessAsync(new GetStickerSetByNameQuery(byName.ShortName)),
            _ => null
        };

        if (stickerSet == null)
        {
            RpcErrors.RpcErrors406.StickersetInvalid.ThrowRpcError();
        }

        var aggregateId = InstalledStickerSetId.Create(input.UserId, stickerSet!.StickerSetId);
        var date = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await commandBus.PublishAsync(new InstallCommand(aggregateId, input.UserId, stickerSet.StickerSetId, stickerSet.StickerSetType, date));

        return new MyTelegram.Schema.Messages.TStickerSetInstallResultSuccess();
    }
}