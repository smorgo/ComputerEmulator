using System;
using System.IO;
using System.Runtime.InteropServices;
using HardwareCore;

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

        public static string ResolveCrossPlatformPart(string part)
        {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if(part.StartsWith("~/"))
                {
                    part = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + part.After("~/");
                }

                if(part.StartsWith("~"))
                {
                    part = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + part.After("~");
                }
            }

            return part;
        }
    }
}