namespace MyTelegram.Messenger.Services.Impl;

public class FcmOptions
{
    public string ServerKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

public class FcmPushNotificationSender(
    IQueryProcessor queryProcessor,
    IOptions<FcmOptions> options,
    ILogger<FcmPushNotificationSender> logger,
    IHttpClientFactory httpClientFactory)
    : IPushNotificationSender, ITransientDependency
{
    private const string FcmEndpoint = "https://fcm.googleapis.com/fcm/send";
    private readonly FcmOptions _options = options.Value;

    public async Task SendAsync(long userId, string title, string body, PushNotificationCustomData? custom = null)
    {
        if (!_options.Enabled) return;
        var devices = await queryProcessor.ProcessAsync(new GetPushDevicesQuery(userId));
        foreach (var device in devices)
        {
            if (device.TokenType == 2) // FCM
                await SendToTokenAsync(device.Token, title, body, custom);
        }
    }

    public async Task SendToTokenAsync(string fcmToken, string title, string body, PushNotificationCustomData? custom = null)
    {
        if (!_options.Enabled || string.IsNullOrEmpty(_options.ServerKey)) return;

        try
        {
            var payload = new
            {
                to = fcmToken,
                notification = new { title, body, sound = "default" },
                data = custom
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var client = httpClientFactory.CreateClient("fcm");
            using var request = new HttpRequestMessage(HttpMethod.Post, FcmEndpoint);
            request.Headers.TryAddWithoutValidation("Authorization", $"key={_options.ServerKey}");
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                logger.LogWarning("FCM send failed: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FCM send error for token {Token}", fcmToken[..Math.Min(10, fcmToken.Length)]);
        }
    }
}
