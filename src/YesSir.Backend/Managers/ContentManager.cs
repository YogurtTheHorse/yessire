using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Entities;
using System.Reflection;

namespace YesSir.Backend.Managers {
	[MoonSharpUserData]
	public class ContentManager {
		private static List<BuildingDescription> Buildings = new List<BuildingDescription>();
		private static List<JobDescription> Jobs = new List<JobDescription>();
		private static List<string> Skills = new List<string>();
		private static List<ResourceDescription> StandartResources = new List<ResourceDescription>();

		public static void Init() {
			UserData.RegisterAssembly(typeof(ContentManager).GetTypeInfo().Assembly);
			Script sc = new Script();
			
			sc.Globals["building_dep"] = (Func<string, bool, BuildingDependency>)((s, b) => new BuildingDependency(s, b));
			sc.Globals["resource_dep"] = (Func<string, int, ResourceDependency>)((s, r) => new ResourceDependency(s, r));

			sc.Globals["contentmanager"] = new ContentManager();
			sc.DoFile("Scripts/content.lua");

			foreach (ETask e in Enum.GetValues(typeof(ETask))) {
				Skills.Add(e.ToString().ToLower());
			}

			Skills.AddRange(new string[] {
				"farming",
				"mining",
				"chopping"
			});

			//RegisterResource("corn", 1, "farming", new IDependency[] { new BuildingDependency("field", true) });
			RegisterResource("flour", 2, "milling", null, new IDependency[] { new BuildingDependency("mill", true), new ResourceDependency("corn", 2) });
			RegisterResource("bread", 1.5f, "bakinkg", null, new IDependency[] { new BuildingDependency("bakery", true), new ResourceDependency("flour", 2) });

			RegisterResource("wood", 1, "chopping", new IDependency[] { });

			RegisterResource("rock", 1, "mining", new IDependency[] { });
			RegisterResource("iron", 1.5f, "mining", new IDependency[] { });
			RegisterResource("gold", 2f, "mining", new IDependency[] { });
		}

		public static string GetJobBySkill(string skill, string language) {
			return Jobs.Find(j => j.SkillName == skill).GetName(language);
		}

		public static ResourceDescription[] GetResources() {
			return StandartResources.ToArray();
		}

		public static string[] GetSkills() {
			return Skills.ToArray();
		}

		public static BuildingDescription[] GetBuildings() {
			return Buildings.ToArray();
		}

		public static JobDescription[] GetJobs() {
			return Jobs.ToArray();
		}

		public static void RegisterResource(string name, float difficulty, string skill = "mining", IDependency[] deps = null, IDependency[] cdeps = null) {
			StandartResources.Add(new ResourceDescription() {
				Name = name,
				Difficulty = difficulty,
				Skill = skill,
				Extractable = deps != null,
				ExtractionDependencies = deps ?? new IDependency[] { },
				Creatable = cdeps != null,
				CreationDependencies = cdeps ?? new IDependency[] { }
			});
		}

		public static void RegisterJob(string name, string skillname, int money, string building = "", IDependency[] addition = null) {
			List<IDependency> deps = new List<IDependency>();
			//deps.Add(new HumanDependency("peasant"));

			if (building != "") {
				deps.Add(new BuildingDependency(building, true));
			}

			if (addition != null) {
				foreach (IDependency d in addition) {
					deps.Add(d);
				}
			}

			List<IDependency> FullDeps = new List<IDependency>(deps);
			FullDeps.Add(new ResourceDependency("money", money));

			Jobs.Add(new JobDescription() {
				Name = name,
				SkillName = skillname,
				HireDepence = FullDeps.ToArray(),
				WorkDependence = deps.ToArray(),
				TrainDepence = new IDependency[] { }
			});
		}

		public static void RegisterBuilding(string name, Dictionary<string, int> resources, IDependency[] addition = null) {
			List<IDependency> deps = new List<IDependency>();
			deps.Add(new HumanDependency());

			foreach (KeyValuePair<string, int> p in resources) {
				deps.Add(new ResourceDependency(p.Key, p.Value));
			}

			if (addition != null) {
				foreach (IDependency d in addition) {
					deps.Add(d);
				}
			}

			Buildings.Add(new BuildingDescription() {
				Name = name,
				Dependencies = deps.ToArray()
			});
		}
	}
}
