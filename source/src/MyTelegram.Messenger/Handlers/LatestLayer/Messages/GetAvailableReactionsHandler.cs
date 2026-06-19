namespace MyTelegram.Messenger.Handlers.LatestLayer.Messages;
/// <summary>
/// Obtain available <a href="https://corefork.telegram.org/api/reactions">message reactions »</a>
/// <para><c>See <a href="https://corefork.telegram.org/method/messages.getAvailableReactions"/> </c></para>
/// </summary>
/// <remarks>
/// Access: [User ✔] [Bot ✖] [Anonymous ✖]
/// </remarks>
internal sealed class GetAvailableReactionsHandler
    : RpcResultObjectHandler<MyTelegram.Schema.Messages.RequestGetAvailableReactions, MyTelegram.Schema.Messages.IAvailableReactions>
{
    private static readonly (string Emoji, string Title)[] _reactions =
    [
        ("👍", "Thumbs Up"),
        ("👎", "Thumbs Down"),
        ("❤️", "Heart"),
        ("🔥", "Fire"),
        ("🥰", "Hearts"),
        ("👏", "Applause"),
        ("😁", "Grinning"),
        ("🤔", "Thinking"),
        ("🤯", "Exploding Head"),
        ("😱", "Screaming"),
        ("🤬", "Rage"),
        ("😢", "Crying"),
        ("🎉", "Party"),
        ("🤩", "Star Eyes"),
        ("🤮", "Vomiting"),
        ("💩", "Poop"),
        ("🙏", "Folded Hands"),
        ("👌", "OK Hand"),
        ("🕊️", "Dove"),
        ("🤡", "Clown"),
        ("🥱", "Yawning"),
        ("🥴", "Woozy"),
        ("😍", "Smiling with Hearts"),
        ("🐳", "Whale"),
        ("❤️‍🔥", "Heart on Fire"),
        ("🌚", "New Moon Face"),
        ("🌭", "Hotdog"),
        ("💯", "100"),
        ("🤣", "ROFL"),
        ("⚡", "Lightning"),
        ("🍌", "Banana"),
        ("🏆", "Trophy"),
        ("💔", "Broken Heart"),
        ("🤨", "Raised Eyebrow"),
        ("😐", "Neutral Face"),
        ("🍓", "Strawberry"),
        ("🍾", "Champagne"),
        ("💋", "Kiss"),
        ("🖕", "Middle Finger"),
        ("😈", "Smiling Devil"),
        ("😴", "Sleeping"),
        ("😭", "Loudly Crying"),
        ("🤓", "Nerd"),
        ("👻", "Ghost"),
        ("👨‍💻", "Technologist"),
        ("👀", "Eyes"),
        ("🎃", "Jack-O-Lantern"),
        ("🙈", "See-No-Evil"),
        ("😡", "Angry Face")
    ];

    protected override Task<MyTelegram.Schema.Messages.IAvailableReactions> HandleCoreAsync(
        IRequestInput input,
        MyTelegram.Schema.Messages.RequestGetAvailableReactions obj)
    {
        var reactionList = new TVector<IAvailableReaction>();

        foreach (var (emoji, title) in _reactions)
        {
            var empty = new TDocumentEmpty { Id = 0 };
            reactionList.Add(new TAvailableReaction
            {
                Reaction = emoji,
                Title = title,
                Inactive = false,
                Premium = false,
                StaticIcon = empty,
                AppearAnimation = empty,
                SelectAnimation = empty,
                ActivateAnimation = empty,
                EffectAnimation = empty,
            });
        }

        return Task.FromResult<IAvailableReactions>(new TAvailableReactions
        {
            Hash = 0,
            Reactions = reactionList
        });
    }
}
