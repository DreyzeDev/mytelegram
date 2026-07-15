namespace MyTelegram.ReadModel.Interfaces;

public interface IPhoneCallReadModel : IReadModel
{
    long CallId { get; }
    long AccessHash { get; }
    long CallerId { get; }
    long CalleeId { get; }
    byte[] GaHash { get; }
    PhoneCallProtocol Protocol { get; }
    bool IsVideo { get; }
    int Date { get; }
    byte[]? Gb { get; }
    byte[]? Ga { get; }
    long KeyFingerprint { get; }
    string CallState { get; }
}
