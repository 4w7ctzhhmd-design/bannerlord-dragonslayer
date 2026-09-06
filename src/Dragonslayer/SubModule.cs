using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Dragonslayer
{
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Diagnostics.Write("Loaded Dragonslayer 0.1.0; target v1.4.8. Commands: dragonslayer.give, dragonslayer.status");
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (!(game.GameType is Campaign)) return;
            try
            {
                var item = MBObjectManager.Instance.GetObject<ItemObject>("dragonslayer");
                if (item == null) throw new InvalidOperationException("Item not registered. Check module XMLs and load order.");
                var stats = Stats.Load();
                foreach (var w in item.Weapons)
                {
                    var frame = w.Frame;
                    // Installed v1.4.8 public API. Preserve geometry, inertia, flags and all other weapon data.
                    w.Init(w.WeaponDescriptionId, w.PhysicsMaterial, w.ItemUsage,
                        w.ThrustDamageType, w.SwingDamageType, w.BodyArmor, w.WeaponLength,
                        w.WeaponBalance, w.TotalInertia, w.CenterOfMass, stats.Handling,
                        w.SwingDamageFactor, w.ThrustDamageFactor, w.MaxDataValue, w.PassbySoundCode,
                        w.Accuracy, w.MissileSpeed, w.StickingFrame, w.AmmoClass, w.SweetSpotReach,
                        stats.SwingSpeed, stats.SwingDamage, stats.ThrustSpeed, stats.ThrustDamage,
                        w.RotationSpeed, w.WeaponTier, w.ReloadPhaseCount);
                    w.SetFrame(frame); // Init resets Frame; retain the crafted orientation.
                }
                Diagnostics.Write(Commands.Describe(item));
            }
            catch (Exception ex)
            {
                Diagnostics.Write("Configuration failed; generated crafting stats remain. " + ex);
                InformationManager.DisplayMessage(new InformationMessage("Dragonslayer stats failed to apply. Check Dragonslayer.log."));
            }
        }
    }

    public static class Commands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("give", "dragonslayer")]
        public static string Give(List<string> args)
        {
            if (args.Count != 0) return "Usage: dragonslayer.give (adds one sword)";
            if (Campaign.Current == null || MobileParty.MainParty == null)
                return "Load a single-player campaign and return to the campaign map first.";
            try
            {
                var item = MBObjectManager.Instance.GetObject<ItemObject>("dragonslayer");
                if (item == null) return "Dragonslayer item is missing. Check module registration and XML logs.";
                MobileParty.MainParty.ItemRoster.AddToCounts(item, 1);
                Diagnostics.Write("Added one sword to main party. " + Describe(item));
                return "Added one Dragonslayer. Open inventory (I).";
            }
            catch (Exception ex)
            {
                Diagnostics.Write("give failed: " + ex);
                return "Could not add sword. See Dragonslayer.log: " + ex.Message;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("status", "dragonslayer")]
        public static string Status(List<string> args)
        {
            if (Campaign.Current == null) return "No single-player campaign loaded.";
            var item = MBObjectManager.Instance.GetObject<ItemObject>("dragonslayer");
            return item == null ? "Dragonslayer item not registered." : Describe(item);
        }

        internal static string Describe(ItemObject item)
        {
            var w = item.PrimaryWeapon;
            return string.Format(CultureInfo.InvariantCulture,
                "{0}: weight={1:0.00} kg, reach={2} cm, swing={3} {4}, thrust={5} {6}, speeds={7}/{8}, handling={9}, usage={10}",
                item.Name, item.Weight, w.WeaponLength, w.SwingDamage, w.SwingDamageType,
                w.ThrustDamage, w.ThrustDamageType, w.SwingSpeed, w.ThrustSpeed, w.Handling, w.ItemUsage);
        }
    }

    internal sealed class Stats
    {
        internal int SwingDamage, ThrustDamage, SwingSpeed, ThrustSpeed, Handling;
        internal static Stats Load()
        {
            var module = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../.."));
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            var doc = new XmlDocument { XmlResolver = null };
            using (var reader = XmlReader.Create(Path.Combine(module, "ModuleData", "weapon_stats.xml"), settings)) doc.Load(reader);
            var root = doc.DocumentElement;
            if (root == null || root.Name != "DragonslayerStats") throw new FormatException("Expected DragonslayerStats root.");
            return new Stats {
                SwingDamage = Read(root, "swingDamage", 1, 500), ThrustDamage = Read(root, "thrustDamage", 1, 500),
                SwingSpeed = Read(root, "swingSpeed", 1, 200), ThrustSpeed = Read(root, "thrustSpeed", 1, 200),
                Handling = Read(root, "handling", 1, 200)
            };
        }
        private static int Read(XmlElement root, string name, int min, int max)
        {
            if (!int.TryParse(root.GetAttribute(name), NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < min || value > max)
                throw new FormatException(name + " must be an integer in " + min + ".." + max);
            return value;
        }
    }

    internal static class Diagnostics
    {
        private static readonly object Sync = new object();
        internal static void Write(string text)
        {
            try
            {
                lock (Sync)
                {
                    var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dragonslayer", "Logs");
                    Directory.CreateDirectory(directory);
                    var path = Path.Combine(directory, "Dragonslayer.log");
                    if (File.Exists(path) && new FileInfo(path).Length > 1024 * 1024)
                    {
                        File.Copy(path, path + ".previous", true);
                        File.WriteAllText(path, "");
                    }
                    File.AppendAllText(path, DateTime.UtcNow.ToString("O") + " " + text + Environment.NewLine);
                }
            }
            catch (IOException) { /* Logging must not break the game. */ }
            catch (UnauthorizedAccessException) { }
        }
    }
}
