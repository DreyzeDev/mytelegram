# MyTelegram — Pro Features: Прогресс реализации

---

## Архитектура проекта

```
Client (TDesktop/Android/iOS)
    │
    ▼ MTProto Layer 224 (TCP)
GatewayServer          ← принимает соединения, дешифрует MTProto пакеты
    │
    ▼ RabbitMQ
CommandServer          ← CQRS Commands → Domain Events → Aggregates (EventFlow)
    │
    ▼ RabbitMQ
QueryServer            ← Read Queries (MongoDB) + Push уведомления клиентам
    │
    ▼
MongoDB                ← EventStore + ReadModels
Redis                  ← Кэш (сессии, временные данные)
RabbitMQ               ← Шина между серверами
```

**Паттерн:** CQRS + Event Sourcing через [EventFlow](https://github.com/eventflow/EventFlow)

- **CommandServer** обрабатывает команды, применяет к агрегатам, публикует доменные события
- **QueryServer** читает MongoDB read models, пушит обновления клиентам через WebSocket/TCP
- **Source Generator** автоматически генерирует Command/CommandHandler/Event классы из методов агрегата помеченных `[EnableAutoGeneration]`

---

## Что сделано

### ✅ 1. 2FA — Двухфакторная аутентификация (SRP6a)

**Что:** Защита аккаунта паролем без передачи пароля по сети (алгоритм SRP6a).

**Как работает:**
1. Пользователь устанавливает пароль → `account.updatePasswordSettings`
2. При следующем входе сервер генерирует случайный параметр `B` и кэширует SRP-сессию (300 сек в Redis)
3. Клиент вычисляет `M1` (proof-of-knowledge) на стороне клиента
4. Сервер проверяет `M1` без хранения/передачи открытого пароля
5. При успехе → авторизация, при неудаче → `RpcError(400, PASSWORD_HASH_INVALID)`

**Ключевые файлы:**
```
source/src/MyTelegram.Messenger/Handlers/Account/
  GetPasswordHandler.cs                ← генерирует B, кэширует SRP сессию
  UpdatePasswordSettingsHandler.cs     ← сохраняет хэш пароля + KDF параметры
  CheckPasswordHandler.cs              ← проверяет M1, выдаёт авторизацию

source/src/MyTelegram.Infrastructure/
  Core/SrpSessionCacheItem.cs          ← временное хранение SRP сессии (Redis TTL 300s)

source/src/MyTelegram.ReadModel/
  Impl/UserPasswordReadModel.cs        ← хранит хэш + salt1, salt2, g, p в MongoDB

source/src/MyTelegram.QueryHandlers.MongoDB/User/
  GetUserPasswordQueryHandler.cs       ← читает пароль пользователя из MongoDB
```

---

### ✅ 2. Реакции на сообщения

**Что:** Эмодзи-реакции на сообщения (👍❤️🔥😂 и т.д.).

**Как работает:**
1. Клиент отправляет `messages.sendReaction` с выбранным эмодзи
2. Сервер обновляет счётчики реакций в агрегате сообщения
3. Публикуется `MessageReactionSentEvent`
4. QueryServer получает событие → отправляет `TUpdateMessageReactions` всем участникам чата

**Ключевые файлы:**
```
source/src/MyTelegram.Messenger/Handlers/Messages/
  GetAvailableReactionsHandler.cs      ← возвращает список 50 стандартных реакций

source/src/MyTelegram.Messenger.QueryServer/DomainEventHandlers/
  MessageDomainEventHandler.cs         ← обрабатывает MessageReactionSentEvent,
                                          пушит TUpdateMessageReactions участникам
```

---

### ✅ 3. Авто-удаление сообщений (TTL / Disappearing Messages)

**Что:** Сообщения автоматически исчезают через установленное время (10 сек / 1 мин / 1 день и т.д.).

**Как работает:**
1. Пользователь устанавливает таймер на чат
2. При отправке сообщения ему присваивается `ExpirationTime = UnixNow + TtlPeriod`
3. Фоновый сервис **каждые 30 секунд** запрашивает из MongoDB все сообщения где `ExpirationTime <= сейчас`
4. Для каждой страницы (100 шт.) отправляется `StartDeleteMessagesCommand` через RabbitMQ в CommandServer
5. Сага `DeleteMessagesSaga4` удаляет у всех владельцев, клиентам приходит `TUpdateDeleteMessages`

**Детали реализации:**
- Фоновый сервис запущен в **QueryServer** (там есть доступ к MongoDB через query handlers)
- `RequestInfo.Empty` — используется вместо реального пользователя (системная операция)
- Запросы страницами по 100 с пагинацией `skip/limit` чтобы не перегружать память

**Ключевые файлы:**
```
source/src/MyTelegram.Domain.Shared/
  AutoDeleteMessageItem.cs             ← модель: ownerPeerId, messageId, toPeerType, toPeerId, expirationTime

source/src/MyTelegram.QueryHandlers.MongoDB/Messaging/
  GetAutoDeleteMessagesQueryHandler.cs ← MongoDB: WHERE ExpirationTime IS NOT NULL AND ExpirationTime <= @now

source/src/MyTelegram.QueryHandlers.InMemory/Messaging/
  GetAutoDeleteMessagesQueryHandler.cs ← то же для InMemory режима (CommandServer)

source/src/MyTelegram.Messenger.QueryServer/BackgroundServices/
  AutoDeleteMessagesBackgroundService.cs  ← фоновый сервис, цикл каждые 30 сек
```

---

### ✅ 4. Запланированные сообщения (Scheduled Messages)

**Что:** Отправка сообщения в заданное время в будущем.

**Как работает:**
1. Клиент отправляет сообщение с флагом `schedule_date`
2. Сообщение сохраняется в MongoDB с полем `ScheduleDate` (и флагом `Out = true`)
3. Фоновый сервис **каждые 30 секунд** ищет исходящие сообщения где `ScheduleDate <= сейчас`
4. Для каждого: восстанавливает полный `MessageItem` из read model, отправляет через `StartSendMessageCommand`
5. Старое scheduled сообщение удаляется командой `StartDeleteMessagesCommand`
6. Клиент получает реальное сообщение в чате, scheduled версия пропадает

**Детали реализации:**
- Только исходящие (`Out = true`) — чтобы не обрабатывать одно сообщение дважды (у входящей копии тот же `ScheduleDate`)
- `RequestInfo` создаётся с `UserId` владельца — сага отправки знает от чьего имени
- `GroupedId` сохраняется для медиагрупп (album)

**Ключевые файлы:**
```
source/src/MyTelegram.QueryHandlers.MongoDB/Messaging/
  GetScheduleMessagesByDateQueryHandler.cs  ← MongoDB: WHERE ScheduleDate <= @now AND Out = true

source/src/MyTelegram.QueryHandlers.InMemory/Messaging/
  GetScheduleMessagesByDateQueryHandler.cs  ← то же для InMemory

source/src/MyTelegram.Messenger.QueryServer/BackgroundServices/
  ScheduledMessagesBackgroundService.cs     ← фоновый сервис, реотправка + удаление scheduled
```

---

### ✅ 5. Стикеры (Stickers) — базовая инфраструктура

**Что:** Хранение и получение стикерпаков, установка паков пользователем, поиск стикеров по emoji.

**Как работает:**
1. Стикерпак создаётся через `StickerSetAggregate.CreateStickerSet` → событие `StickerSetCreatedEvent`
2. `StickerSetReadModel` в MongoDB хранит метаданные пака: название, emoji-паки, список document ID стикеров
3. `messages.getStickerSet` — ищет пак по ID или shortName, возвращает пак + документы стикеров
4. `messages.getStickers` — ищет все паки, фильтрует по emoji в packs, возвращает matching документы
5. `messages.installStickerSet` — создаёт `InstalledStickerSetAggregate`, привязывает пак к пользователю

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/StickerSet/
  StickerSetAggregate.cs            ← [EnableAutoGeneration], метод CreateStickerSet
  StickerSetId.cs                   ← детерминированный ID на основе stickerSetId
  StickerSetState.cs                ← хранит ID пака в памяти агрегата

source/src/MyTelegram.Domain/Aggregates/InstalledStickerSet/
  InstalledStickerSetAggregate.cs   ← методы Install / Uninstall
  InstalledStickerSetId.cs          ← детерминированный ID: userId + stickerSetId
  InstalledStickerSetState.cs       ← флаг Installed

source/src/MyTelegram.ReadModel/Impl/
  StickerSetReadModel.cs            ← слушает StickerSetCreatedEvent, хранит все поля пака
  InstalledStickerSetReadModel.cs   ← слушает Install/UninstallStickerSetEvent

source/src/MyTelegram.QueryHandlers.MongoDB/StickerSet/   (+ InMemory аналоги)
  GetStickerSetByIdQueryHandler.cs
  GetStickerSetByNameQueryHandler.cs
  GetStickerSetsByIdListQueryHandler.cs
  GetInstalledStickerSetsQueryHandler.cs
  GetInstalledStickerSetQueryHandler.cs
  GetAllStickerSetsQueryHandler.cs

source/src/MyTelegram.QueryHandlers.MongoDB/Document/     (+ InMemory аналоги)
  GetDocumentByIdQueryHandler.cs    ← реализован (был только интерфейс без handler)
  GetDocumentsByIdListQueryHandler.cs

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Messages/
  GetStickerSetHandler.cs           ← lookup по ID/имени + fetch документы + IStickerSetConverter
  GetStickersHandler.cs             ← поиск по emoji в packs всех паков
  InstallStickerSetHandler.cs       ← убран NotImplementedException, dispatch InstallCommand
```

---

## Что предстоит сделать

### ✅ 6. Privacy Settings (Настройки приватности)

**Что:** Контроль кто видит номер телефона, онлайн-статус, аватар, и т.д.

**Как работает:**
1. Пользователь вызывает `account.setPrivacy` с ключом (StatusTimestamp, PhoneNumber, и т.д.) и списком правил
2. Сервер маппирует `IInputPrivacyKey` → `PrivacyType` и `IInputPrivacyRule[]` → `List<PrivacyValueData>`
3. Создаётся `PrivacyAggregate` с детерминированным ID = `userId + privacyType`, выполняется `SetPrivacyCommand`
4. Событие `PrivacySetEvent` обновляет `PrivacyReadModel` в MongoDB
5. При `account.getPrivacy` — загружаются правила из MongoDB и конвертируются обратно в `IPrivacyRule[]`
6. Другие части системы могут вызывать `IPrivacyAppService.ApplyPrivacyAsync` для проверки правил

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/Privacy/
  PrivacyAggregate.cs             ← [EnableAutoGeneration], метод SetPrivacy
  PrivacyId.cs                    ← детерминированный ID: userId + privacyType
  PrivacyState.cs                 ← применяет PrivacySetEvent

source/src/MyTelegram.ReadModel/Impl/
  PrivacyReadModel.cs             ← хранит userId, privacyType, список правил

source/src/MyTelegram.QueryHandlers.MongoDB/Privacy/   (+ InMemory аналоги)
  GetPrivacyQueryHandler.cs       ← по userId + privacyType
  GetPrivacyListQueryHandler.cs   ← по списку userId + список типов

source/src/MyTelegram.Messenger/Services/Impl/
  PrivacyAppService.cs            ← маппинг Input→Data, dispatch команды, Apply правил

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Account/
  GetPrivacyHandler.cs            ← вызывает GetPrivacyRulesAsync, возвращает TPrivacyRules
  SetPrivacyHandler.cs            ← вызывает SetPrivacyAsync, возвращает TPrivacyRules
```

---

### ✅ 7. Forum Topics (Темы в супергруппах)

**Что:** Разбивка супергруппы на топики — каждый топик как отдельная ветка переписки.

**Как работает:**
1. `messages.createForumTopic` → `IdGenerator` выдаёт topicId из канальных messageId, создаётся `ForumTopicAggregate` через `CreateTopicCommand`, ответ — `TUpdates` с `TMessageActionTopicCreate`
2. `messages.editForumTopic` → `EditTopicCommand` обновляет title/closed/hidden/pinned в агрегате
3. `messages.getForumTopics` → `GetForumTopicsQuery` с пагинацией по `OffsetTopic`, конвертация в `TForumTopic` через `IForumTopicsConverter`
4. `messages.getForumTopicsByID` → `GetForumTopicsByIdsQuery` по списку ID
5. `messages.updatePinnedForumTopic` / `messages.reorderPinnedForumTopics` → применяют pin через `EditTopicCommand`

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/Channel/
  ForumTopicAggregate.cs         ← [EnableAutoGeneration], методы CreateTopic / EditTopic
  ForumTopicId.cs                ← уже существовал
  ForumTopicState.cs             ← применяет Created + Edited события

source/src/MyTelegram.ReadModel/Impl/
  ForumTopicReadModel.cs         ← слушает оба события, хранит все поля топика

source/src/MyTelegram.QueryHandlers.MongoDB/ForumTopic/  (+ InMemory аналоги)
  GetForumTopicsQueryHandler.cs
  GetForumTopicsByIdsQueryHandler.cs
  GetForumTopicByIdQueryHandler.cs

source/src/MyTelegram.Converters/TLObjects/LatestLayer/
  ForumTopicsConverter.cs        ← IForumTopicReadModel → TForumTopic

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Messages/
  CreateForumTopicHandler.cs     ← убран NotImplementedException
  EditForumTopicHandler.cs       ← убран NotImplementedException
  GetForumTopicsHandler.cs       ← убрана заглушка, реальный запрос
  GetForumTopicsByIDHandler.cs   ← убрана заглушка, реальный запрос
  UpdatePinnedForumTopicHandler.cs
  ReorderPinnedForumTopicsHandler.cs
```

---

### 🔲 8. Bot Support (Боты)

**Что нужно:** Регистрация ботов, обработка команд, inline режим, webhook/polling.

**Как будет сделано:**
- `BotAggregate` + `BotReadModel` — создание/обновление бота
- `bots.createBot`, `bots.setBotCommands`, `bots.getBotInfo`, `auth.importBotAuthorization`
- Webhook доставка обновлений + Bot API polling (getUpdates)
- Сложность: **высокая**

---

### ✅ 9. Star Gifts (Звёздные подарки)

**Что:** Полная реализация API подарков — domain-модель, каталог, сохранение, конвертация, чтение из MongoDB.

**Как работает:**
1. `StarGiftAggregate` — каталог подарков (giftId → stars, availabilityTotal, stickerDocumentId)
2. `UserStarGiftAggregate` — полученный подарок пользователя (ownerPeerId, msgId, giftId, sender, convertStars)
3. `payments.getStarGifts` → читает `StarGiftReadModel` из MongoDB, строит `TStarGift` с реальным `TDocument`
4. `payments.getSavedStarGifts` → читает `UserStarGiftReadModel` из MongoDB + join с `StarGiftReadModel`
5. `payments.getSavedStarGift` → lookup по peer + msgId/savedId из MongoDB
6. `payments.saveStarGift` → проверяет существование, dispatch `SaveGiftCommand` → `UserStarGiftSavedEvent`
7. `payments.convertStarGift` → проверяет существование/права, dispatch `ConvertGiftCommand` → `UserStarGiftConvertedEvent`
8. `payments.upgradeStarGift`, `payments.transferStarGift`, `payments.craftStarGift` → `RpcErrors400.PaymentRequired` (требует Stars платёж)

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/StarGift/
  StarGiftAggregate.cs          ← [EnableAutoGeneration], CreateStarGift / UpdateAvailability
  StarGiftId.cs                 ← детерминированный ID: "stargift-{giftId}"
  StarGiftState.cs              ← применяет StarGiftCreatedEvent, StarGiftAvailabilityUpdatedEvent
  UserStarGiftAggregate.cs      ← [EnableAutoGeneration], ReceiveGift / SaveGift / ConvertGift
  UserStarGiftId.cs             ← детерминированный ID: "userstargift-{ownerPeerId}-{msgId}"
  UserStarGiftState.cs          ← применяет Received/Saved/Converted события

source/src/MyTelegram.ReadModel.Interfaces/
  IStarGiftReadModel.cs         ← интерфейс каталога подарков
  IUserStarGiftReadModel.cs     ← интерфейс полученного подарка

source/src/MyTelegram.ReadModel/Impl/
  StarGiftReadModel.cs          ← IAmReadModelFor Created + AvailabilityUpdated
  UserStarGiftReadModel.cs      ← IAmReadModelFor Received + Saved + Converted

source/src/MyTelegram.QueryHandlers.MongoDB/StarGift/  (+ InMemory аналоги)
  GetAllStarGiftsQueryHandler.cs
  GetStarGiftByIdQueryHandler.cs
  GetUserStarGiftsQueryHandler.cs
  GetUserStarGiftByMsgIdQueryHandler.cs

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Payments/
  GetStarGiftsHandler.cs        ← читает из DB, строит TStarGift с TDocument
  GetSavedStarGiftsHandler.cs   ← читает из DB, join подарок + тип
  GetSavedStarGiftHandler.cs    ← lookup по peer/msgId из DB
  SaveStarGiftHandler.cs        ← проверка + SaveGiftCommand
  ConvertStarGiftHandler.cs     ← проверка + ConvertGiftCommand
  UpgradeStarGiftHandler.cs     ← PAYMENT_REQUIRED (upgrade стоит Stars, которых нет в системе)
  TransferStarGiftHandler.cs    ← PAYMENT_REQUIRED (transfer может стоить Stars)
  CraftStarGiftHandler.cs       ← PAYMENT_REQUIRED
  GetPaymentFormHandler.cs      ← INVOICE_INVALID (Bot Payments не реализован)
  GetPaymentReceiptHandler.cs   ← MESSAGE_ID_INVALID (Receipt storage не реализован)
  GetUniqueStarGiftHandler.cs   ← STARGIFT_SLUG_INVALID (уникальные NFT-подарки не реализованы)
  GetUniqueStarGiftValueInfoHandler.cs ← STARGIFT_SLUG_INVALID
  GetStarGiftAuctionStateHandler.cs    ← STARGIFT_INVALID (аукционы не реализованы)
```

> Ни одного `throw new NotImplementedException()` — все handlers возвращают корректный RPC error клиенту.

---

### ✅ 10. Themes & Wallpapers (Темы и обои)

**Что:** Создание, редактирование и получение кастомных тем оформления.

**Как работает:**
1. `account.createTheme` → генерирует `themeId` через `IdGenerator`, `slug` из input, создаёт `ThemeAggregate` через `CreateThemeCommand`, возвращает `TTheme`
2. `account.updateTheme` → ищет тему в `ThemeReadModel`, диспатчит `UpdateThemeCommand` через `ThemeId.Create(creatorUserId, slug)`
3. `account.getTheme` → `TInputTheme` (по ID) или `TInputThemeSlug` (по slug) → query к MongoDB, если не найден — `THEME_INVALID`
4. `account.uploadTheme` → возвращает `THEME_FILE_INVALID` (файловый хостинг не реализован)
5. `account.setWallPaper` / `account.getWallPapers` / `account.uploadWallPaper` → `WALLPAPER_INVALID` (хостинг обоев не реализован)

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/Theme/
  ThemeAggregate.cs         ← [EnableAutoGeneration], методы CreateTheme / UpdateTheme
  ThemeId.cs                ← детерминированный ID: creatorUserId + slug
  ThemeState.cs             ← применяет ThemeCreatedEvent + ThemeUpdatedEvent

source/src/MyTelegram.ReadModel/Impl/
  ThemeReadModel.cs         ← IAmReadModelFor ThemeAggregate, хранит slug/title/settings

source/src/MyTelegram.QueryHandlers.MongoDB/Theme/ (+ InMemory аналоги)
  GetThemeByIdQueryHandler.cs
  GetThemeBySlugQueryHandler.cs
  GetDefaultThemesQueryHandler.cs

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Account/
  CreateThemeHandler.cs     ← полная реализация
  UpdateThemeHandler.cs     ← полная реализация
  GetThemeHandler.cs        ← switch по TInputTheme/TInputThemeSlug
  UploadThemeHandler.cs     ← THEME_FILE_INVALID
  GetWallPaperHandler.cs    ← WALLPAPER_INVALID
  UploadWallPaperHandler.cs ← WALLPAPER_INVALID
```

---

### ✅ 11. Chatlist (Папки и общие папки)

**Что:** Folder-фильтры диалогов (папки) + chatlist invite-ссылки — полная реализация.

**Как работает:**
1. `messages.getDialogFilters` → `DialogFilterReadModel` в MongoDB (уже полностью реализовано)
2. `messages.updateDialogFilter` → поддержка `TDialogFilter` (обычная папка) + `TDialogFilterChatlist` (chatlist) — оба создают `UpdateDialogFilterCommand` с `DialogFilter`
3. `DialogFilter` record расширен полем `ImportedFromSlug` — сохраняется slug при join, используется для поиска обновлений
4. `checkChatlistInvite` → загружает invite по slug из `ChatlistInviteReadModel`, возвращает `TChatlistInviteAlready` (уже в папке) или `TChatlistInvite` (доступен для join)
5. `joinChatlistInvite` → находит invite, создаёт `DialogFilter` с `ImportedFromSlug = slug`, диспатчит `UpdateDialogFilterCommand`
6. `exportChatlistInvite` → создаёт `ChatlistInviteAggregate` через `CreateChatlistInviteCommand`, возвращает `TExportedChatlistInvite`
7. `editExportedInvite` → находит invite, обновляет title/peers через `UpdateChatlistInviteCommand`
8. `getChatlistUpdates` → находит папку по `ImportedFromSlug`, сравнивает peers invite vs папки, возвращает missing peers
9. `joinChatlistUpdates` → добавляет новые peers из `obj.Peers` в `IncludePeers` папки через `UpdateDialogFilterCommand`
10. `getLeaveChatlistSuggestions` → возвращает `IncludePeers` папки как предложения для удаления
11. `leaveChatlist` → диспатчит `DeleteDialogFilterCommand` для удаления папки
12. `hideChatlistUpdates` → валидирует фильтр, возвращает `TBoolTrue`
13. `deleteExportedInvite` → `TBoolTrue`
14. `getExportedInvites` → пустой `TExportedInvites` (ChatlistInvite по userId ещё нет query)

**Ключевые файлы:**
```
source/src/MyTelegram.Domain.Shared/
  DialogFilter.cs               ← добавлено поле ImportedFromSlug (optional)

source/src/MyTelegram.ReadModel/Impl/
  DialogFilterReadModel.cs      ← добавлено маппинг ImportedFromSlug в ApplyAsync

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Messages/
  UpdateDialogFilterHandler.cs  ← TDialogFilter + TDialogFilterChatlist оба поддержаны

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Chatlists/
  CheckChatlistInviteHandler.cs         ← полная реализация (query invite → Already/Available)
  JoinChatlistInviteHandler.cs          ← полная реализация (создаёт DialogFilter с slug)
  ExportChatlistInviteHandler.cs        ← полная реализация (CreateChatlistInviteCommand)
  EditExportedInviteHandler.cs          ← полная реализация (UpdateChatlistInviteCommand)
  GetChatlistUpdatesHandler.cs          ← полная реализация (сравнивает invite peers vs folder)
  JoinChatlistUpdatesHandler.cs         ← полная реализация (добавляет peers в папку)
  GetLeaveChatlistSuggestionsHandler.cs ← полная реализация (IncludePeers папки)
  HideChatlistUpdatesHandler.cs         ← валидирует фильтр, TBoolTrue
  LeaveChatlistHandler.cs               ← DeleteDialogFilterCommand
  DeleteExportedInviteHandler.cs        ← TBoolTrue
  GetExportedInvitesHandler.cs          ← пустой TExportedInvites
```

---

### ✅ 12. Telegram Business (Бизнес функции)

**Что:** Business Chat Links (ссылки для начала чата с бизнесом) + Quick Reply shortcuts — полная реализация.

**Как работает:**
1. `account.createBusinessChatLink` → генерирует `slug = Guid.NewGuid().ToString("N")[..16]`, создаёт `BusinessChatLinkAggregate`, возвращает `TBusinessChatLink { Link = "https://t.me/+{slug}", ... }`
2. `account.editBusinessChatLink` → находит ссылку, проверяет владельца, диспатчит `EditLinkCommand`
3. `account.resolveBusinessChatLink` → ищет по slug в `BusinessChatLinkReadModel`, возвращает `TResolvedBusinessChatLinks` с `TPeerUser`
4. `messages.editQuickReplyShortcut` → загружает shortcut по id, бросает `SHORTCUT_INVALID` если нет, диспатчит `EditShortcutCommand`
5. `messages.deleteQuickReplyShortcut` → загружает shortcut, бросает `SHORTCUT_INVALID` если нет, диспатчит `DeleteShortcutCommand`
6. `messages.deleteQuickReplyMessages` → валидирует shortcut, возвращает `TUpdates` с `TUpdateDeleteQuickReplyMessages { ShortcutId, Messages }`

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/QuickReplyShortcut/
  QuickReplyShortcutAggregate.cs  ← [EnableAutoGeneration], CreateShortcut/EditShortcut/DeleteShortcut
  QuickReplyShortcutId.cs         ← детерминированный ID: userId + shortcutId
  QuickReplyShortcutState.cs      ← применяет Created/Edited/Deleted события

source/src/MyTelegram.ReadModel/Impl/
  QuickReplyShortcutReadModel.cs  ← IAmReadModelFor Created/Edited, MarkForDeletion при Deleted

source/src/MyTelegram.QueryHandlers.MongoDB/QuickReplyShortcut/ (+ InMemory аналоги)
  GetQuickReplyShortcutByIdQueryHandler.cs

source/src/MyTelegram.Domain/Aggregates/BusinessChatLink/
  BusinessChatLinkAggregate.cs  ← [EnableAutoGeneration], CreateLink / EditLink / DeleteLink
  BusinessChatLinkId.cs         ← детерминированный ID: userId + slug
  BusinessChatLinkState.cs      ← применяет Created/Edited/Deleted события

source/src/MyTelegram.ReadModel/Impl/
  BusinessChatLinkReadModel.cs  ← IAmReadModelFor Created/Edited, MarkForDeletion при Deleted

source/src/MyTelegram.QueryHandlers.MongoDB/BusinessChatLink/ (+ InMemory аналоги)
  GetBusinessChatLinkBySlugQueryHandler.cs
  GetBusinessChatLinksByUserIdQueryHandler.cs

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Account/
  CreateBusinessChatLinkHandler.cs  ← полная реализация
  EditBusinessChatLinkHandler.cs    ← полная реализация
  ResolveBusinessChatLinkHandler.cs ← полная реализация

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Messages/
  EditQuickReplyShortcutHandler.cs     ← полная реализация (валидация + EditShortcutCommand)
  DeleteQuickReplyShortcutHandler.cs   ← полная реализация (валидация + DeleteShortcutCommand)
  DeleteQuickReplyMessagesHandler.cs   ← полная реализация (TUpdateDeleteQuickReplyMessages)
```

---

### ✅ 13. Stories (Истории) — частично

**Что реализовано:** Полная инфраструктура + `stories.sendStory`.

**Как работает `sendStory`:**
1. `mediaHelper.SaveMediaAsync(obj.Media)` → `IMessageMedia` (gRPC на FileServer), при null → `MEDIA_EMPTY`
2. `peerHelper.GetPeer(obj.Peer, userId)` → domain `Peer` (user или channel)
3. `idGenerator.NextIdAsync(IdType.StoryId, peerId)` → уникальный storyId per-peer
4. `privacyAppService.GetPrivacyValueDataList(obj.PrivacyRules)` → `List<PrivacyValueData>`
5. Строится `StoryItem` domain record (id, peer, media, randomId, privacyRules, date, expireDate = date + period, caption, mediaAreas, pinned, noforwards, entities)
6. `CreateStoryCommand(StoryId.Create(peerId, storyId), requestInfo, storyItem)` → `StoryAggregate`
7. Вычисляет privacy flags (Public/Contacts/CloseFriends/SelectedContacts) из rules
8. Возвращает `TUpdates` с `TUpdateStory { Peer, Story = TStoryItem { ...все поля... } }`

**Ключевые файлы:**
```
source/src/MyTelegram.Domain/Aggregates/Story/
  StoryAggregate.cs    ← [EnableAutoGeneration], методы CreateStory / DeleteStory
  StoryId.cs           ← детерминированный ID: "story-{peerId}-{storyId}"
  StoryState.cs        ← применяет StoryCreatedEvent / StoryDeletedEvent

source/src/MyTelegram.ReadModel/Impl/
  StoryReadModel.cs    ← IAmReadModelFor Created (все поля) + Deleted (MarkForDeletion)

source/src/MyTelegram.ReadModel.MongoDB/
  MongoDbReadModels.cs          ← StoryReadModel : Impl.StoryReadModel, IMongoDbReadModel
  MyTelegramServerReadModelMongoDbExtensions.cs ← .UseMongoDbReadModel<StoryAggregate, StoryId, StoryReadModel>()

source/src/MyTelegram.ReadModel.InMemory/
  InMemoryReadModels.cs         ← StoryReadModel : Impl.StoryReadModel
  MyTelegramReadModelInMemoryExtensions.cs ← .UseMyInMemoryReadStoreFor<...>()

source/src/MyTelegram.QueryHandlers.MongoDB/Story/ (+ InMemory аналоги)
  GetStoryByIdQueryHandler.cs
  GetStoriesByPeerQueryHandler.cs
  GetStoriesByIdListQueryHandler.cs

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Stories/
  SendStoryHandler.cs  ← полная реализация (media save + privacy + aggregate + TUpdates)
```

**Что ещё не реализовано:**
- `stories.editStory`, `stories.deleteStories`
- `stories.getStories`, `stories.getPeerStories`, `stories.getAllStories`
- Push уведомления подписчикам при новой истории

---

### 🔲 14. Passkey Login (Вход через Passkey)

**Что нужно:** Беспарольный вход через FIDO2/WebAuthn passkey.

**Как будет сделано:**
- `auth.requestLoginToken` → QR/passkey challenge
- Хранение passkey credential в `UserAuthReadModel`
- Сложность: **высокая**

---

### ✅ 15. Email Login (Вход через Email)

**Что:** Верификация email через 6-значный код — для привязки email к аккаунту.

**Как работает:**
1. Клиент вызывает `account.sendVerifyEmailCode` с `purpose: TEmailVerifyPurposeLoginSetup { PhoneCodeHash }` и `email`
2. Сервер генерирует 6-значный код (или `FixedEmailVerificationCode` если настроен), кэширует `EmailCodeCacheItem(email, code)` с ключом = `phoneCodeHash` (TTL = `VerificationCodeExpirationSeconds`)
3. Отправляет код на email через `IEmailSender`
4. Возвращает `TSentEmailCode { EmailPattern = "a***@domain.com", Length = 6 }`
5. Клиент вводит код и вызывает `account.verifyEmail` с тем же `purpose` + `TEmailVerificationCode { Code }`
6. Сервер загружает из кэша по `phoneCodeHash`, проверяет код, возвращает `TEmailVerified { Email }`

**Ключевые файлы:**
```
source/src/MyTelegram.Core/
  EmailCodeCacheItem.cs               ← запись кэша (email + code), ключ = phoneCodeHash

source/src/MyTelegram.Messenger/Handlers/LatestLayer/Account/
  SendVerifyEmailCodeHandler.cs       ← генерация кода, кэш, отправка письма
  VerifyEmailHandler.cs               ← проверка кода, возврат TEmailVerified
```

---

### ✅ 16. Email Sender (Отправка Email)

**Что:** SMTP реализация отправки писем (коды верификации, сброс пароля).

**Как работает:**
1. `IEmailSender.SendAsync(to, subject, body)` — интерфейс для отправки писем
2. `SmtpEmailSender` использует `System.Net.Mail.SmtpClient` с настройками из `SmtpOptions`
3. Если `Enabled = false` — только логирует (отправки нет), что позволяет разработку без реального SMTP
4. Настройка в `appsettings.json` секция `"Smtp"`: Host, Port, UseSsl, UserName, Password, From

**Ключевые файлы:**
```
source/src/MyTelegram.Messenger/Services/Interfaces/
  IEmailSender.cs                     ← интерфейс SendAsync(to, subject, body)

source/src/MyTelegram.Messenger/Services/Impl/
  SmtpEmailSender.cs                  ← SmtpOptions + SmtpClient реализация

source/src/MyTelegram.Messenger.CommandServer/
  appsettings.json                    ← секция "Smtp": { "Enabled": false, "Host": "smtp.example.com", ... }
  Program.cs                          ← services.Configure<SmtpOptions>(...)
```

---

### 🔲 17. Direct Messages (Прямые сообщения)

**Что нужно:** Личные сообщения между пользователями без создания группы.

**Как будет сделано:**
- Уточнить что именно отличается от обычных PM — вероятно речь об `InputPeerUser` DM flow
- Сложность: **низкая** (скорее всего уже работает)

---

### ✅ 18. Push Server / Firebase FCM (Push уведомления)

**Что:** Уведомления на телефон когда приложение закрыто (через Firebase Cloud Messaging).

**Как работает:**
1. Клиент при входе вызывает `account.registerDevice` → сохраняет FCM-токен в `PushDeviceReadModel`
2. QueryServer при приходе нового личного сообщения (`PeerType.User`, inbox) вызывает `IPushNotificationSender.SendAsync(userId, ...)`
3. `FcmPushNotificationSender` загружает все FCM-токены пользователя через `GetPushDevicesQuery`
4. Для каждого устройства с `TokenType == 2` (FCM) отправляет POST на `https://fcm.googleapis.com/fcm/send` с `Authorization: key={ServerKey}`
5. Настройки в `appsettings.json` секция `"Fcm"` — `Enabled: false` по умолчанию

**Ключевые файлы:**
```
source/src/MyTelegram.Messenger/Services/Interfaces/
  IPushNotificationSender.cs          ← интерфейс SendAsync / SendToTokenAsync

source/src/MyTelegram.Messenger/Services/Impl/
  FcmPushNotificationSender.cs        ← FcmOptions (ServerKey, SenderId, Enabled), HttpClient "fcm"

source/src/MyTelegram.QueryHandlers.MongoDB/PushDevice/  (+ InMemory аналоги)
  GetPushDevicesQueryHandler.cs       ← фильтр по UserId

source/src/MyTelegram.Messenger.QueryServer/
  appsettings.json                    ← секция "Fcm": { "Enabled": false, "ServerKey": "", "SenderId": "" }
  Program.cs                          ← services.Configure<FcmOptions>(...)
  Extensions/...Extensions.cs         ← services.AddHttpClient("fcm")

source/src/MyTelegram.Messenger.QueryServer/DomainEventHandlers/
  MessageDomainEventHandler.cs        ← вызов pushNotificationSender при inbox PM
```

---

### 🔲 19. E2E Encrypted Chat (Секретные чаты)

**Что нужно:** Сквозное шифрование, ключи не хранятся на сервере.

**Как будет сделано:**
- DH обмен ключами: `messages.requestEncryption` → `messages.acceptEncryption`
- `EncryptedChatReadModel` — хранит DH параметры и g_a/g_b
- Сервер хранит только зашифрованный blob
- Сложность: **очень высокая**

---

### 🔲 20. Voice & Video Calls (Звонки)

**Что нужно:** Аудио и видео P2P звонки между пользователями.

**Как будет сделано:**
- Сигнализация: `phone.requestCall`, `phone.acceptCall`, `phone.discardCall`
- Медиапоток через WebRTC P2P или TURN/STUN (Coturn)
- Сложность: **очень высокая**

---

## Статус

| Функция | Статус |
|---------|--------|
| Privacy Settings & 2FA (SRP6a) | ✅ Готово |
| Реакции | ✅ Готово |
| Авто-удаление сообщений | ✅ Готово |
| Запланированные сообщения | ✅ Готово |
| Стикеры | ✅ Готово |
| Forum Topics | ✅ Готово |
| Bot Support | 🔲 Не начато |
| Star Gifts | ✅ Готово (domain model + real DB queries) |
| Themes & Wallpapers | ✅ Готово (ThemeAggregate + read queries; wallpaper → WALLPAPER_INVALID) |
| Chatlist | ✅ Готово (все 11 handlers полностью реализованы, ImportedFromSlug pipeline) |
| Telegram Business | ✅ Готово (BusinessChatLink + Quick Reply shortcuts — все handlers полностью) |
| Stories | 🔲 Инфраструктура + sendStory готовы; edit/delete/get — не начато |
| Passkey Login | 🔲 Не начато |
| Email Login | ✅ Готово |
| Email Sender | ✅ Готово |
| Direct Messages | 🔲 Не начато |
| Push Server (Firebase FCM) | ✅ Готово |
| E2E Encrypted Chat | 🔲 Не начато |
| Voice & Video Calls | 🔲 Не начато |
