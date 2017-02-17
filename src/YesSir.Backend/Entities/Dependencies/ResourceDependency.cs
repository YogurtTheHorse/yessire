using MoonSharp.Interpreter;
using System;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Dependencies {
	[MoonSharpUserData]
	public class ResourceDependency : IDependency {
		private readonly string Resorce;
		private readonly int Count;

		public ResourceDependency(string resourceName, int count) {
			this.Resorce = resourceName;
			this.Count = count;
		}

		public Tuple<bool, MessageCallback> CheckKingdom(Kingdom kingdom) {
			if (kingdom.GetResourcesCount(Resorce) >= Count) {
				return new Tuple<bool, MessageCallback>(true, new MessageCallback());
			} else {
				MessageCallback cb = new MessageCallback() {
					Text = Locale.Get("resources." + Resorce + ".miss", kingdom.Language),
					From = ECharacter.Knight
				};
				return new Tuple<bool, MessageCallback>(false, cb);
			}
		}

		public IUsable Use(Kingdom kingdom) {
			kingdom.Resources[Resorce].RemoveRange(0, Count);
			if (kingdom.Resources[Resorce].Count == 0) {
				kingdom.Resources.Remove(Resorce);
			}
			return null;
		}
	}
}
