namespace MyTelegram.ReadModel.Interfaces;

public interface IStoryAlbumReadModel : IReadModel
{
    long OwnerPeerId { get; }
    int AlbumId { get; }
    string Title { get; }
    List<int> StoryIds { get; }
}
