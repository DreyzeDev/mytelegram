namespace MyTelegram.Domain.Aggregates.Story;

public class StoryAlbumId(string value) : Identity<StoryAlbumId>(value)
{
    public static StoryAlbumId Create(long ownerPeerId, int albumId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"storyalbum-{ownerPeerId}-{albumId}");
}
