using MyTelegram.Messenger.Services.Caching;
using MyTelegram.Schema.Messages;
using MyTelegram.Schema.Updates;
using MyTelegram.Schema.Extensions;
using MyTelegram.Domain.Aggregates.EncryptedChat;
namespace MyTelegram.Messenger.QueryServer.DomainEventHandlers;

public class EncryptedChatDomainEventHandler(
    IObjectMessageSender objectMessageSender,
    ICommandBus commandBus,
    IIdGenerator idGenerator,
    IAckCacheService ackCacheService,
    IQueryProcessor queryProcessor)
    : DomainEventHandlerBase(objectMessageSender, commandBus, idGenerator, ackCacheService),
        ISubscribeSynchronousTo<EncryptedChatAggregate, EncryptedChatId, EncryptedChatRequestedEvent>,
        ISubscribeSynchronousTo<EncryptedChatAggregate, EncryptedChatId, EncryptedChatAcceptedEvent>,
        ISubscribeSynchronousTo<EncryptedChatAggregate, EncryptedChatId, EncryptedChatDiscardedEvent>,
        ISubscribeSynchronousTo<EncryptedMessageAggregate, EncryptedMessageId, EncryptedMessageSentEvent>
{
    public async Task HandleAsync(IDomainEvent<EncryptedChatAggregate, EncryptedChatId, EncryptedChatRequestedEvent> domainEvent, CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        var chatRequested = new TEncryptedChatRequested
        {
            Id = (int)e.ChatId,
            AccessHash = e.AccessHash,
            Date = e.Date,
            AdminId = e.AdminId,
            ParticipantId = e.ParticipantId,
            GA = e.Ga
        };

        var update = new TUpdateEncryption { Chat = chatRequested, Date = e.Date };
        var updates = new TUpdateShort { Date = e.Date, Update = update };

        await PushUpdatesToPeerAsync(
            new Peer(PeerType.User, e.ParticipantId),
            updates,
            pts: 0,
            updatesType: UpdatesType.Updates,
            skipSaveUpdates: false);
    }

    public async Task HandleAsync(IDomainEvent<EncryptedChatAggregate, EncryptedChatId, EncryptedChatAcceptedEvent> domainEvent, CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        var chatId = int.Parse(domainEvent.AggregateIdentity.Value.Replace("encryptedchat-", ""));
        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(chatId), cancellationToken);
        if (chat == null) return;

        var activeChat = new TEncryptedChat
        {
            Id = (int)chat.ChatId,
            AccessHash = chat.AccessHash,
            Date = chat.Date,
            AdminId = chat.AdminId,
            ParticipantId = chat.ParticipantId,
            GAOrB = e.Gb,
            KeyFingerprint = e.KeyFingerprint
        };

        var update = new TUpdateEncryption { Chat = activeChat, Date = chat.Date };
        var updates = new TUpdateShort { Date = chat.Date, Update = update };

        await PushUpdatesToPeerAsync(
            new Peer(PeerType.User, chat.AdminId),
            updates,
            pts: 0,
            updatesType: UpdatesType.Updates,
            skipSaveUpdates: false);
    }

    public async Task HandleAsync(IDomainEvent<EncryptedChatAggregate, EncryptedChatId, EncryptedChatDiscardedEvent> domainEvent, CancellationToken cancellationToken)
    {
        var chatId = int.Parse(domainEvent.AggregateIdentity.Value.Replace("encryptedchat-", ""));
        var chat = await queryProcessor.ProcessAsync(new GetEncryptedChatByIdQuery(chatId), cancellationToken);
        if (chat == null) return;

        var discardedChat = new TEncryptedChatDiscarded
        {
            Id = (int)chat.ChatId
        };

        var update = new TUpdateEncryption { Chat = discardedChat, Date = CurrentDate };
        var updates = new TUpdateShort { Date = CurrentDate, Update = update };

        // Send to both admin and participant
        await PushUpdatesToPeerAsync(new Peer(PeerType.User, chat.AdminId), updates, pts: 0, updatesType: UpdatesType.Updates, skipSaveUpdates: false);
        await PushUpdatesToPeerAsync(new Peer(PeerType.User, chat.ParticipantId), updates, pts: 0, updatesType: UpdatesType.Updates, skipSaveUpdates: false);
    }

    public async Task HandleAsync(IDomainEvent<EncryptedMessageAggregate, EncryptedMessageId, EncryptedMessageSentEvent> domainEvent, CancellationToken cancellationToken)
    {
        var e = domainEvent.AggregateEvent;
        
        IEncryptedMessage encryptedMsg;
        if (e.MessageType == SendMessageType.MessageService)
        {
            encryptedMsg = new TEncryptedMessageService
            {
                RandomId = e.RandomId,
                ChatId = (int)e.ChatId,
                Date = e.Date,
                Bytes = e.Data
            };
        }
        else
        {
            var encryptedFile = e.File?.ToTObject<IEncryptedFile>() ?? new TEncryptedFileEmpty();
            encryptedMsg = new TEncryptedMessage
            {
                RandomId = e.RandomId,
                ChatId = (int)e.ChatId,
                Date = e.Date,
                Bytes = e.Data,
                File = encryptedFile
            };
        }

        var update = new TUpdateNewEncryptedMessage { Message = encryptedMsg, Qts = e.Qts };
        var updates = new TUpdateShort { Date = e.Date, Update = update };

        await PushUpdatesToPeerAsync(
            new Peer(PeerType.User, e.UserId),
            updates,
            pts: 0,
            updatesType: UpdatesType.Updates,
            skipSaveUpdates: true);
    }

    private int CurrentDate => (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
}
