namespace MyTelegram.Domain.Aggregates.PhoneCall;

public class PhoneCallId(string value) : Identity<PhoneCallId>(value)
{
    public static PhoneCallId Create(long callId) =>
        NewDeterministic(GuidFactories.Deterministic.Namespaces.Commands, $"phonecall-{callId}");
}
