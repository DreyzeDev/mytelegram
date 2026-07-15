namespace MyTelegram.ReadModel.Interfaces;

public interface IDocumentReadModel : IReadModel
{
    long AccessHash { get; }
    byte[]? Attributes { get; }
    long? CreatorId { get; }
    int Date { get; }
    int DcId { get; }
    long DocumentId { get; }
    ReadOnlyMemory<byte> FileReference { get; }
    int? Fingerprint { get; }
    string? Md5CheckSum { get; }
    string? Name { get; }
    string MimeType { get; }
    long Size { get; }

    /// <summary>
    /// SHA256 hash of the document's file content, used by messages.getDocumentByHash.
    /// No existing hash field was found on this read model (Md5CheckSum is a different, weaker
    /// legacy checksum used elsewhere), so this field was added specifically for that lookup.
    /// </summary>
    byte[]? Sha256Hash { get; }

    //byte[]? Stickers { get; }
    long? ThumbId { get; }
    List<PhotoSize>? Thumbs { get; }
    long? VideoThumbId { get; }
    List<VideoSize>? VideoThumbs { get; }
    List<IDocumentAttribute>? Attributes2 { get; }
}