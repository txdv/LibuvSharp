using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibuvSharp
{
    internal static class PlatformApis
    {
#if NETSTANDARD1_3
        static PlatformApis()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsDarwin = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            IsUnix = IsDarwin || IsLinux;
            //https://github.com/dotnet/coreclr/issues/4555
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DefaultEncoding = Encoding.GetEncoding(0);
        }
#else
        static PlatformApis()
        {
            IsDarwin = System.Environment.OSVersion.Platform == System.PlatformID.MacOSX;
            IsLinux = System.Environment.OSVersion.Platform == System.PlatformID.Unix;
            IsWindows = !IsDarwin && !IsLinux;
            IsUnix = IsDarwin || IsLinux;
            DefaultEncoding = Encoding.Default;
        }
#endif
        public const string LIBUV = "libuv";

        public static Encoding DefaultEncoding { get; }

        public static bool IsWindows { get; }

        public static bool IsUnix { get; }

        public static bool IsDarwin { get; }

        public static bool IsLinux { get; }

        public static string PtrToStringAuto(IntPtr ptr)
        {
            return IsWindows ? Marshal.PtrToStringUni(ptr) : Marshal.PtrToStringAnsi(ptr);
        }

        public static string PtrToStringAuto(IntPtr ptr, int len)
        {
            return IsWindows ? Marshal.PtrToStringUni(ptr, len) : Marshal.PtrToStringAnsi(ptr, len);
        }

        public static unsafe string NewString(sbyte* value)
        {
#if NETSTANDARD1_3
            var length = *value / sizeof(sbyte*);
            var chars = new char[length];
            for (var i = 0; i < length; ++i)
                chars[i] = (char)value[i];
            //maybe return many \0, we can drop that.
            return new string(chars).Split('\0')[0];
#else
            return new string(value);
#endif

        }
    }
}
