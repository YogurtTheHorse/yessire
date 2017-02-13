using System;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Commands.Dependencies {
	internal class BuildingDependency : IDependency {
		private string BuildingName;

		public BuildingDependency(string builing) {
			this.BuildingName = builing;
		}

		public Tuple<bool, MessageCallback> CheckKingdom(Kingdom kingdom) {
			foreach (Building b in kingdom.Buildings) {
				if (b.Name == BuildingName) { return new Tuple<bool, MessageCallback>(true, new MessageCallback()); }
			}

			return new Tuple<bool, MessageCallback>(
				false, 
				new MessageCallback(Locale.Get("buildings." + BuildingName + ".miss", kingdom.Language), ECharacter.Knight)
			);
		}

		public void Use(Kingdom kingdom) { }
	}
}