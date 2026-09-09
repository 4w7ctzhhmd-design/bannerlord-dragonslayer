namespace DragonslayerInstantKill
{
    internal static class HitPolicy
    {
        internal static bool ShouldKill(string itemId, bool human, bool active, bool validAttacker,
            bool strikesAgent, bool shield, bool alternative, bool missile, bool fall, bool charge)
        {
            return itemId == "dragonslayer" && human && active && validAttacker && strikesAgent
                && !shield && !alternative && !missile && !fall && !charge;
        }
    }
}
