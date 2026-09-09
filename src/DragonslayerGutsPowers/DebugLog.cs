using System;
using System.IO;
namespace DragonslayerGutsPowers
{
    internal static class DebugLog
    {
        private static readonly object Sync = new object();
        internal static void Write(string text, bool always = false)
        {
            if (!always && !DragonslayerConfig.Current.DebugLogging) return;
            try { lock (Sync) {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dragonslayer", "Logs");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "GutsPowers.log");
                if (File.Exists(path) && new FileInfo(path).Length > 1048576) { File.Copy(path, path + ".previous", true); File.WriteAllText(path, ""); }
                File.AppendAllText(path, DateTime.UtcNow.ToString("O") + " " + text + Environment.NewLine);
            }} catch (IOException) {} catch (UnauthorizedAccessException) {}
        }
    }
}
