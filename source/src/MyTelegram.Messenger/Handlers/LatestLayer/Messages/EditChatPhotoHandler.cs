namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Changes chat photo and sends a service message on it
/// Possible errors
/// Code Type Description
/// 400 CHAT_ID_INVALID The provided chat id is invalid.
/// 400 CHAT_NOT_MODIFIED No changes were made to chat information because the new information you passed is identical to the current information.
/// 400 IMAGE_PROCESS_FAILED Failure while processing image.
/// 400 PEER_ID_INVALID The provided peer id is invalid.
/// 400 PHOTO_CROP_SIZE_SMALL Photo is too small.
/// 400 PHOTO_EXT_INVALID The extension of the photo is invalid.
/// 400 PHOTO_INVALID Photo invalid.
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.editChatPhoto"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✔] [Anonymous ✖]
/// </remarks>
internal sealed class EditChatPhotoHandler(IMediaHelper mediaHelper, ICommandBus commandBus, IRandomHelper randomHelper, IChannelAdminRightsChecker channelAdminRightsChecker) : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestEditChatPhoto, MyTelegram.Schema.IUpdates>
{
    protected override async Task<IUpdates> HandleCoreAsync(IRequestInput input, RequestEditChatPhoto obj)
    {
        var channelId = obj.ChatId;
        await channelAdminRightsChecker.CheckAdminRightAsync(channelId, input.UserId, adminRights => adminRights.ChangeInfo);

        long fileId = 0;
        var parts = 0;
        var md5 = string.Empty;
        var name = string.Empty;
        var hasVideo = false;
        double? videoStartTs = 0;
        IVideoSize? videoSize = null;
        switch (obj.Photo)
        {
            case Schema.TInputChatUploadedPhoto inputChatUploadedPhoto1:
            {
                var file = inputChatUploadedPhoto1.File ?? inputChatUploadedPhoto1.Video;
                if (file is TInputFile tInputFile)
                {
                    fileId = tInputFile!.Id;
                    parts = tInputFile.Parts;
                    name = tInputFile.Name;
                    hasVideo = inputChatUploadedPhoto1.Video != null;
                    videoStartTs = inputChatUploadedPhoto1.VideoStartTs;
                    switch (file)
                    {
                        case TInputFile inputFile:
                            md5 = inputFile.Md5Checksum;
                            break;
                        case TInputFileBig:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(file));
                    }
                }

                videoSize = inputChatUploadedPhoto1.VideoEmojiMarkup;
            }

                break;
            case TInputChatPhoto inputChatPhoto:
                switch (inputChatPhoto.Id)
                {
                    case TInputPhoto inputPhoto:
                        fileId = inputPhoto.Id;
                        break;
                    case TInputPhotoEmpty:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inputChatPhoto.Id));
                }

                break;
            case TInputChatPhotoEmpty:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        long? photoId;
        var r = await mediaHelper.SavePhotoAsync(input.ReqMsgId, input.UserId, fileId, hasVideo, videoStartTs, parts, name, md5, videoSize);
        photoId = r.PhotoId;
        var photo = r.Photo;
        var command = new EditChannelPhotoCommand(ChannelId.Create(channelId), input.ToRequestInfo(), photoId, new TMessageActionChatEditPhoto { Photo = photo }, randomHelper.NextInt64());
        await commandBus.PublishAsync(command);
        return null !;
    }
}
