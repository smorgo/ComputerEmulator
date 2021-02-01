using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FilePersistence
{
    public static class CrossPlatformPathExtensions
    {
        public static string Combine(params string[] parts)
        {
            var current = string.Empty;

            foreach(var part in parts)
            {
                var newPart = ResolveCrossPlatformPart(part);
                current = Path.Combine(current, newPart);
            }

            return current;
        }

        private static string ResolveCrossPlatformPart(string part)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return part;
            }

            if(part == "~" || part == "~/")
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar;
            }

            if(part.StartsWith("~/"))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + part.Substring(2);
            }

            if(part.StartsWith("~"))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + part.Substring(1);
            }

            return part;
        }
    }
}