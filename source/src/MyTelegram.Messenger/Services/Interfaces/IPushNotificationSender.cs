namespace MyTelegram.Messenger.Services.Interfaces;

public interface IPushNotificationSender
{
    Task SendAsync(long userId, string title, string body, PushNotificationCustomData? custom = null);
    Task SendToTokenAsync(string fcmToken, string title, string body, PushNotificationCustomData? custom = null);
}
