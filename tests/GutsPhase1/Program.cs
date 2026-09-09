using System;
using System.Xml;
using DragonslayerGutsPowers;

static void Check(string name, bool result) { if (!result) throw new Exception(name); }
static bool Eligible(string id = "dragonslayer", bool playerOnly = false, bool player = false,
    bool active = true, bool human = true, bool enabled = true, bool network = false) =>
    WielderPolicy.Eligible(enabled, human, active, playerOnly, player, network, id);
Check("NPC actively wields", Eligible());
Check("Inventory only/sheath", !Eligible(id: null));
Check("Switched weapon", !Eligible(id: "native_sword"));
Check("Dead agent", !Eligible(active: false));
Check("Mount excluded", !Eligible(human: false));
Check("Master off", !Eligible(enabled: false));
Check("Network excluded", !Eligible(network: true));
Check("NPC excluded by setting", !Eligible(playerOnly: true));
Check("Player allowed", Eligible(playerOnly: true, player: true));
var doc = new XmlDocument { XmlResolver = null }; doc.Load(args[0]);
var defaults = DragonslayerConfig.Parse(doc.DocumentElement);
Check("Defaults", defaults.StrengthMultiplier == 1.25f && defaults.PoiseMaxIncomingDamage == 35);
foreach (var value in new[] { "NaN", "Infinity", "5", "0", "not-a-number" }) {
    var copy = (XmlDocument)doc.CloneNode(true); copy.DocumentElement.SetAttribute("StrengthMultiplier", value);
    bool rejected = false;
    try { DragonslayerConfig.Parse(copy.DocumentElement); } catch (FormatException) { rejected = true; }
    Check("Reject bad strength: " + value, rejected);
}
foreach (string option in new[] { "PlayerOnly", "EnableGutsPowers" }) {
    var copy = (XmlDocument)doc.CloneNode(true); copy.DocumentElement.RemoveAttribute(option);
    bool rejected = false;
    try { DragonslayerConfig.Parse(copy.DocumentElement); } catch (FormatException) { rejected = true; }
    Check("Reject missing " + option, rejected);
}
var unknown = (XmlDocument)doc.CloneNode(true); unknown.DocumentElement.SetAttribute("SwingSpeeedBonus", "0.5");
bool invalid = false;
try { DragonslayerConfig.Parse(unknown.DocumentElement); } catch (FormatException) { invalid = true; }
Check("Reject typo", invalid);
Console.WriteLine("PASS: 18 configuration/wielder cases; no game engine executed.");
