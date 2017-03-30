using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Reflection;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Entities.Items;
using YesSir.Backend.Entities.Kingdoms;

namespace YesSir.Backend.Managers {
	public static class ScriptManager {
		public static Script Script;	

		public static void Init() {
			Script = new Script();

			UserData.RegisterAssembly(typeof(ContentManager).GetTypeInfo().Assembly);
			UserData.RegisterType<Kingdom>();

			Script.Globals["building_dep"] = (Func<string, bool, BuildingDependency>)((s, b) => new BuildingDependency(s, b));
			Script.Globals["item_dep"] = (Func<string, int, ItemDependency>)((s, r) => new ItemDependency(s, r)); ;
			Script.Globals["human_dep"] = (Func<HumanDependency>)(() => new HumanDependency());
			
			Script.Globals["gen_items"] = (Func<int, string, float, Item>)((cnt, name, quality) => new Item(cnt, name, quality));

			Script.Globals["new_kingdom"] = (Func<DynValue>)(() => UserData.Create(new Kingdom()));

			Script.Globals["contentmanager"] = typeof(ContentManager);
			Script.Globals["rand"] = typeof(RandomManager);
			//Script.Globals["usersmanager"] = typeof(UsersManager);
			Script.Globals["locale"] = typeof(Locale);
		}

		public static DynValue DoFile(string v, params object[] objs) {
			DynValue foo = DoFile(v);
			return foo.Function.Call(objs);
		}

		public static DynValue DoFile(string v) {
			return Script.DoFile(v);
		}
	}
}
