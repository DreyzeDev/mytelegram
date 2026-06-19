# MyTelegram

[![API Layer](https://img.shields.io/badge/API_Layer-224-blueviolet)](https://corefork.telegram.org/methods)
[![MTProto](https://img.shields.io/badge/MTProto_Protocol-2.0-green)](https://corefork.telegram.org/mtproto/)
[![MyTelegram Channel](https://img.shields.io/badge/Subscribe-_MyTelegram_Channel-0088cc)](https://t.me/+9wMJrMqLTIoyYzM8)
[![MyTelegram Discussion Group](https://img.shields.io/badge/Join_-MyTelegram_Discussion_Group-0088cc)](https://t.me/+S-aNBoRvCRpPyXrR)

MyTelegram is a self-hosted C# implementation of the Telegram server-side API, designed for private deployments and extensibility.

## Supported Features

### Open Source Features
- API Layer: `224`
- MTProto Transports: `Abridged`, `Intermediate`
- Private Chat
- Supergroup Chat
- Channel

### Pro Version Features
- End-to-End Encrypted Chat
- Voice & Video Calls
- Bot Support
- Privacy Settings & 2FA
- Stickers
- Reactions
- Star Gifts
- Forum Topics
- Themes & Wallpapers
- Auto-Delete Messages
- Scheduled Messages
- Chatlist
- Telegram Business
- Stories
- Passkey Login
- Email Login
- Email Sender
- Direct Messages
- Push Server (Firebase)

---

## Running MyTelegram Server

### Run with Docker

1. Download the Docker Compose configuration files:

https://raw.githubusercontent.com/loyldg/mytelegram/dev/docker/compose/docker-compose.yml  
https://raw.githubusercontent.com/loyldg/mytelegram/dev/docker/compose/.env

2. Edit `.env` and replace `192.168.1.100` with your own server IP address.

3. Start the server:

```
mkdir -p ./data/mytelegram
chmod -R a+w ./data/mytelegram
docker compose up
```
4. Default verification code (for testing only): `22222`
5. Default listening ports: `20443`, `20543`, `20643`, `20644`, `30443`, `30444`

## Building Docker Images

### Linux / amd64
`./build-all-amd64.sh`

### Linux / arm64
`./build-all-arm64.sh`

## MyTelegram Clients

| Platform | Repository |
|----------|------------|
| Desktop (TDesktop) | https://github.com/loyldg/mytelegram-tdesktop |
| Android | https://github.com/loyldg/mytelegram-android |
| iOS | https://github.com/loyldg/mytelegram-iOS |
| WebK | https://github.com/loyldg/mytelegram-webk |
| WebA | https://github.com/loyldg/mytelegram-weba |

### Configure Clients
1. Clone the client source code.  
2. Search for `192.168.1.100` in all files and replace it with your own server IP.

---

## Support MyTelegram

If you find MyTelegram helpful, please consider giving the project a ⭐.


## Feedback

- Contact author: https://t.me/mytelegram666  
- MyTelegram Channel: https://t.me/+9wMJrMqLTIoyYzM8  
- Discussion Group: https://t.me/+S-aNBoRvCRpPyXrR

---

# Pro Features: Прогресс реализации

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

## Статус реализации

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

Подробное описание каждой функции см. в [PROGRESS.md](PROGRESS.md).
