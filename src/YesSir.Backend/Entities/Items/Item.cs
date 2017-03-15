using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Entities.Items {
	[MoonSharp.Interpreter.MoonSharpUserData]
	public class Item {
		public string Name;
		public float Quality;

		public int Count = 1;

		public Item(int count, string name, float quality) {
			Name = name;
			Quality = quality;
			Count = count;
		}
	}
}