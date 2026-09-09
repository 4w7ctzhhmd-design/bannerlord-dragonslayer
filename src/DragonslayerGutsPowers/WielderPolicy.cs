namespace DragonslayerGutsPowers
{
    internal static class WielderPolicy
    {
        internal static bool Eligible(bool enabled, bool human, bool active, bool playerOnly,
            bool playerControlled, bool networkSession, string wieldedId) =>
            enabled && human && active && (!playerOnly || playerControlled) && !networkSession
            && wieldedId == "dragonslayer";
    }
}
