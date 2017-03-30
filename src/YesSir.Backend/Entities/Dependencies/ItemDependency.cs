using MoonSharp.Interpreter;
using System;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Dependencies {
	[MoonSharpUserData]
	public class ItemDependency : IDependency {
		public string ItemName;
		public readonly int Count;

		public ItemDependency(string itemName, int count) {
			this.ItemName = itemName;
			this.Count = count;
		}

		public Tuple<bool, MessageCallback> CheckKingdom(Kingdom kingdom) {
			if (kingdom.GetResourcesCount(ItemName) >= Count) {
				return new Tuple<bool, MessageCallback>(true, new MessageCallback());
			} else {
				MessageCallback cb = new MessageCallback() {
					Text = Locale.Get("resources." + ItemName + ".miss", kingdom.Language),
					From = ECharacter.Knight
				};
				return new Tuple<bool, MessageCallback>(false, cb);
			}
		}

		public IUsable Use(Kingdom kingdom) {
			kingdom.TakeResource(ItemName, Count);
			if (ItemName == "water_bucket") { kingdom.AddResource("bucket", Count); }

			return null;
		}
	}
}
