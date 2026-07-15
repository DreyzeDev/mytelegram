namespace MyTelegram.Messenger.Handlers.LatestLayer.Phone;

internal static class PhoneCallBuilderHelper
{
    internal static TPhoneCallProtocol ToSchema(PhoneCallProtocol protocol) => new()
    {
        UdpP2p = protocol.UdpP2P,
        UdpReflector = protocol.UdpReflector,
        MinLayer = protocol.MinLayer,
        MaxLayer = protocol.MaxLayer,
        LibraryVersions = new TVector<string>(protocol.LibraryVersions)
    };

    internal static PhoneCallProtocol FromSchema(IPhoneCallProtocol proto)
    {
        if (proto is TPhoneCallProtocol p)
            return new PhoneCallProtocol(p.UdpP2p, p.UdpReflector, p.MinLayer, p.MaxLayer, p.LibraryVersions.ToList());
        return new PhoneCallProtocol(true, true, 86, 92, []);
    }
}
