using System;
using System.Collections.Generic;
using System.Linq;
using YesSir.Backend.Commands.Dependencies;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Entities;

namespace YesSir.Backend.Managers {
	public static class ContentManager {
		private static List<BuildingDescription> Buildings = new List<BuildingDescription>();
		private static List<JobDescription> Jobs = new List<JobDescription>();
		private static List<string> Skills = new List<string>();
		private static List<ResourceDescription> StandartResources = new List<ResourceDescription>();

		static ContentManager() {
			RegisterJob("peasant", "farming", 10);
			RegisterJob("builder", "building", 100);
			RegisterJob("miner", "mining", 100);
			RegisterBuilding("forge", new Dictionary<string, int>() {
				{ "rock", 5 },
				{ "iron", 5 }
			});

			foreach (ETask e in Enum.GetValues(typeof(ETask))) {
				Skills.Add(e.ToString().ToLower());
			}

			Skills.AddRange(new string[] {
				"farming",
				"mining",
				"chopping"
			});

			RegisterResource("wood", 1, false);

			RegisterResource("rock", 1, true);
			RegisterResource("iron", 1.5f, true);
			RegisterResource("gold", 2f, true);
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

		public static void RegisterResource(string name, float difficulty, bool isOre) {
			StandartResources.Add(new ResourceDescription() {
				Name = name,
				Difficulty = difficulty,
				IsOre = isOre
			});
		}

		public static void RegisterJob(string name, string skillname, int money, string builing = "", IDependency[] addition = null) {
			List<IDependency> deps = new List<IDependency>();
			//deps.Add(new HumanDependency("peasant"));

			if (builing != "") {
				deps.Add(new BuildingDependency(builing));
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
