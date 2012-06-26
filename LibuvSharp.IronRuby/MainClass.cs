using System;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronRuby.Builtins;

namespace LibuvSharp.Ruby
{
	public class MainClass
	{
		static private ScriptEngine engine;

		public static void Main(string[] args)
		{
			Assembly.LoadFile("IronRuby.dll");
			Assembly.LoadFile("IronRuby.Libraries.dll");

			engine = IronRuby.Ruby.CreateEngine();
			var paths = engine.GetSearchPaths().ToList();
			paths.Add(Path.Combine("ruby", "Lib", "ironruby"));
			paths.Add(Path.Combine("ruby", "Lib", "ruby", "1.9.1"));
			engine.SetSearchPaths(paths);

			engine.Runtime.LoadAssembly(Assembly.LoadFile(Assembly.GetExecutingAssembly().Location));
			engine.Runtime.LoadAssembly(Assembly.LoadFile("LibuvSharp.dll"));


			if (System.IO.Directory.Exists("ruby")) {
				foreach (string file in System.IO.Directory.GetFiles("ruby", "*.rb")) {
					Load(file);
				}
			}
		}

		static void Load(string filename)
		{
			ScriptSource script = engine.CreateScriptSourceFromFile(filename);
			ScriptScope scope = engine.CreateScope();
			script.Execute(scope);
		}
	}
}

