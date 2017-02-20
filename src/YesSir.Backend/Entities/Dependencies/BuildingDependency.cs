using MoonSharp.Interpreter;
using System;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Dependencies {
	[MoonSharpUserData]
	public class BuildingDependency : IDependency {
		public string BuildingName;
		public bool CheckBusy;

		public BuildingDependency(string builing, bool busy = false) {
			this.BuildingName = builing;
			this.CheckBusy = busy;
		}

		public Tuple<bool, MessageCallback> CheckKingdom(Kingdom kingdom) {
			foreach (Building b in kingdom.Buildings) {
				if (b.Name == BuildingName && (!CheckBusy || !b.IsBusy)) { return new Tuple<bool, MessageCallback>(true, new MessageCallback()); }
			}

			return new Tuple<bool, MessageCallback>(
				false,
				new MessageCallback(Locale.Get("buildings." + BuildingName + ".miss", kingdom.Language), ECharacter.Knight)
			);
		}

		public IUsable Use(Kingdom kingdom) {
			if (CheckBusy) {
				foreach (Building b in kingdom.Buildings) {
					if (b.Name == BuildingName && !b.IsBusy) {
						b.IsBusy = true;
						return b;
					}
				}
			}
			return null;
		}
	}
}