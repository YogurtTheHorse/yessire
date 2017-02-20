using System;
using System.Linq;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Entities;
using YesSir.Backend.Entities.Items;

namespace YesSir.Backend.Managers {
	[MoonSharpUserData]
	public class ContentManager {
		private static List<BuildingDescription> Buildings = new List<BuildingDescription>();
		private static List<JobDescription> Jobs = new List<JobDescription>();
		private static List<string> Skills = new List<string>();
		private static List<ItemDescription> StandartResources = new List<ItemDescription>();
		
		public static void Init() {
			ScriptManager.DoFile("Scripts/content.lua");

			foreach (ETask e in Enum.GetValues(typeof(ETask))) {
				Skills.Add(e.ToString().ToLower());
			}

			Skills.AddRange(new string[] {
				"farming",
				"mining",
				"chopping",
				"milling"
			});
		}

		public static string GetJobBySkill(string skill, string language) {
			return Jobs.Find(j => j.SkillName == skill).GetName(language);
		}

		public static ItemDescription[] GetResources() {
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

		public static void RegisterGrain(string grain, params string[] args) {
			foreach (ItemDescription id in StandartResources) {
				if (args.Contains(id.Name)) {
					id.Culture = grain;
				}
			}
		}

		public static void RegisterResource(string name, float difficulty, string skill = "mining", IDependency[] deps = null, IDependency[] cdeps = null) {
			StandartResources.Add(new ItemDescription() {
				Name = name,
				Difficulty = difficulty,
				Skill = skill,
				Extractable = deps != null,
				ExtractionDependencies = deps ?? new IDependency[] { },
				Creatable = cdeps != null,
				CreationDependencies = cdeps ?? new IDependency[] { }
			});
		}

		public static void RegisterFood(string name, float difficulty, string skill = "mining", IDependency[] deps = null, IDependency[] cdeps = null) {
			RegisterResource(name, difficulty, skill, deps, cdeps);
			StandartResources.Last().IsFood = true;
		}

		public static string[] GetFood() {
			return StandartResources
				.FindAll(r => r.IsFood)
				.Select(r => r.Name)
				.ToArray();
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
			FullDeps.Add(new ItemDependency("money", money));

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
				deps.Add(new ItemDependency(p.Key, p.Value));
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

		public static string GetBuildingName(string building, string language) {
			var buildingd = Buildings.Find((b) => b.Name == building);
			return buildingd != null ? buildingd.GetName(language) : "";
		}
	}
}
