using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Entities.Items {
	[MoonSharp.Interpreter.MoonSharpUserData]
	public class Item {
		public string Name;
		public float Quality;

		public static List<Item> GenerateItems(int count, string name, float quality, float diff = 0.05f) {
			var items = new Item[count];
			for (int i = 0; i < count; i++) {
				items[i] = new Item() {
					Name = name,
					Quality = RandomManager.NextFloat() * (diff * 2) + quality - diff
				};
			}
			return new List<Item>(items);
		}
	}
}