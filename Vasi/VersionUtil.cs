using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace Vasi
{
    [PublicAPI]
    public static class VersionUtil
    {
        public static string GetVersion<T>()
        {
            Assembly asm = typeof(T).Assembly;
            
            string ver = asm.GetName().Version.ToString();

            using var sha1 = SHA1.Create();
            using FileStream stream = File.OpenRead(asm.Location);

            byte[] hashBytes = sha1.ComputeHash(stream);
            
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return $"{ver}-{hash.Substring(0, 6)}";
        }
    }
}