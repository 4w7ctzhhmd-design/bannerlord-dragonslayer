using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace DragonslayerGutsPowers
{
    internal sealed class DragonslayerConfig
    {
        internal static DragonslayerConfig Current { get; private set; } = new DragonslayerConfig { EnableGutsPowers = false };
        public bool EnableGutsPowers = true, PlayerOnly = false, DebugLogging = true;
        public float StrengthMultiplier = 1.25f, SwingSpeedBonus = .45f, HandlingBonus = .35f,
            ShieldDamageMultiplier = 2.5f, StaggerPowerMultiplier = 1.5f,
            PoiseMultiplier = 1.6f, AttackPoiseMultiplier = 2.2f, PoiseMaxIncomingDamage = 35f,
            KnockbackResistanceBonus = .15f;

        internal static void Load()
        {
            try
            {
                var folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../.."));
                using var reader = XmlReader.Create(Path.Combine(folder, "ModuleData/guts_config.xml"),
                    new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
                var doc = new XmlDocument { XmlResolver = null }; doc.Load(reader);
                Current = Parse(doc.DocumentElement);
                DebugLog.Write("Configuration loaded; enabled=" + Current.EnableGutsPowers + ", PlayerOnly=" + Current.PlayerOnly, true);
            }
            catch (Exception ex)
            {
                Current = new DragonslayerConfig { EnableGutsPowers = false };
                DebugLog.Write("Invalid config; powers disabled. " + ex.Message, true);
            }
        }

        internal static DragonslayerConfig Parse(XmlElement root)
        {
            if (root == null || root.Name != "DragonslayerGutsConfig") throw new FormatException("Expected DragonslayerGutsConfig.");
            var result = new DragonslayerConfig();
            foreach (XmlAttribute attribute in root.Attributes)
                if (typeof(DragonslayerConfig).GetField(attribute.Name) == null) throw new FormatException("Unknown option: " + attribute.Name);
            foreach (var field in typeof(DragonslayerConfig).GetFields())
            {
                var text = root.GetAttribute(field.Name);
                if (field.FieldType == typeof(bool))
                {
                    if (!bool.TryParse(text, out var b)) throw new FormatException(field.Name + " must be true/false.");
                    field.SetValue(result, b); continue;
                }
                if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) || float.IsNaN(v) || float.IsInfinity(v))
                    throw new FormatException("Invalid number: " + field.Name);
                float min = 1f, max = 4f;
                if (field.Name.EndsWith("Bonus")) { min = 0f; max = 1f; }
                if (field.Name == "PoiseMaxIncomingDamage") { min = 1f; max = 100f; }
                if (v < min || v > max) throw new FormatException(field.Name + " outside " + min + ".." + max);
                field.SetValue(result, v);
            }
            return result;
        }
    }
}
