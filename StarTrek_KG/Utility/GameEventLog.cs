using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StarTrek_KG.Utility
{
    public static class GameEventLog
    {
        private static readonly object FileLock = new object();

        public static string GetDefaultPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "latest-game-log.txt");
        }

        public static void Reset(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            lock (FileLock)
            {
                EnsureDirectory(path);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.WriteAllText(path, string.Empty, Encoding.UTF8);
            }
        }

        public static void Append(string path, string message)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lock (FileLock)
            {
                EnsureDirectory(path);
                var line = $"[{DateTime.UtcNow:O}] {message}";
                File.AppendAllLines(path, new[] { line }, Encoding.UTF8);
            }
        }

        public static List<string> ReadAll(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new List<string>();
            }

            lock (FileLock)
            {
                if (!File.Exists(path))
                {
                    return new List<string>();
                }

                return File.ReadAllLines(path).ToList();
            }
        }

        private static void EnsureDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
