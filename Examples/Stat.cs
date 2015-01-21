using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

namespace Stat
{
	class MainClass
	{
		[DllImport("__Internal")]
		unsafe private static extern passwd* getpwuid(uint uid);

		unsafe struct passwd {
			public sbyte* pw_name;       // username
			public sbyte* pw_passwd;     // user password
			public uint pw_uid;          // user ID
			public uint pw_gid;          // group ID
			public sbyte* pw_gecos;      // user information
			public sbyte* pw_dir;        // home directory
			public sbyte* pw_shell;      // shell program
		};

		unsafe public static string getuser(uint uid)
		{
			return new string(getpwuid(1000)->pw_name);
		}

		public static void Main(string[] args)
		{
			Loop.Default.Run(async () => {
				foreach (var file in args) {
					var stat = await UVFileAsync.Stat(file);
					Console.WriteLine("  File: ‘{0}’", file);
					Console.WriteLine("  Size: {0, -15} Blocks: {1, -10} IO Block: {2, -6}", stat.Size, stat.Blocks, stat.BlockSize);
					Console.WriteLine("Device: {0, -15} Inode: {1, -11} Links: {2}", string.Format("{0}h/{1}d", stat.Device.ToString("X"), stat.Device), stat.ino, stat.Link);
					Console.WriteLine("Access: {0, -18} Uid: {1, -18} Gid: {2, -18}", stat.Mode, 
						string.Format("( {0}/ {1})", stat.UID, getuser((uint)stat.UID)),
						string.Format("( {0}/ {1})", stat.GID, getuser((uint)stat.GID)));
					string format = "yyyy-MM-dd H:mm:ss.fffffff zzz";
					Console.WriteLine("Access: {0}", stat.Access.ToString(format));
					Console.WriteLine("Modify: {0}", stat.Modify.ToString(format));
					Console.WriteLine("Change: {0}", stat.Change.ToString(format));
					Console.WriteLine(" Birth: {0}", stat.Birth.ToString(format));
				}
			});

		}
	}
}
