using MoonSharp.Interpreter;
using System;
using System.Reflection;
using YesSir.Backend.Entities.Dependencies;

namespace YesSir.Backend.Managers {
	public static class ScriptManager {
		private static Script Script;	

		public static void Init() {
			Script = new Script();

			UserData.RegisterAssembly(typeof(ContentManager).GetTypeInfo().Assembly);

			Script.Globals["building_dep"] = (Func<string, bool, BuildingDependency>)((s, b) => new BuildingDependency(s, b));
			Script.Globals["resource_dep"] = (Func<string, int, ResourceDependency>)((s, r) => new ResourceDependency(s, r)); ;
			Script.Globals["human_dep"] = (Func<HumanDependency>)(() => new HumanDependency());

			Script.Globals["contentmanager"] = typeof(ContentManager);
			//Script.Globals["usersmanager"] = typeof(UsersManager);
			Script.Globals["locale"] = typeof(Locale);
		}

		public static DynValue DoFile(string v) {
			return Script.DoFile(v);
		}
	}
}
