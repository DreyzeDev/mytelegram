namespace MyTelegram.Domain.Aggregates.Story;

public class StoryId(string value) : Identity<StoryId>(value)
{
    public static StoryId Create(long userId, int storyId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"story-{userId}-{storyId}");
}
