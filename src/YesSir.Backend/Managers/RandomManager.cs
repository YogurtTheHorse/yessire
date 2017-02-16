using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YesSir.Backend.Managers {
	[MoonSharp.Interpreter.MoonSharpUserData]
	public static class RandomManager {
		public static Random Instance = new Random();

		public static int Next(int a, int b) {
			return Instance.Next(a, b);
		}

		public static float NextGoodSkill() {
			return 0.45f + (float)Instance.NextDouble() / 10f;
		}

		public static T Select<T>() where T : struct, IConvertible {
			if (!typeof(T).GetTypeInfo().IsEnum) {
				throw new ArgumentException("T must be an enum");
			}
			T[] t = (T[])Enum.GetValues(typeof(T));
			return t[Next(0, t.Length)];
		}

		public static float NextFloat() {
			return (float)Instance.NextDouble();
		}

		public static float NextDefaultSkill() {
			return 0.1f + (float)Instance.NextDouble() / 5f;
		}

		public static float QuanticFloat(float max=1) {
			return (float)Math.Pow(Instance.NextDouble() * max, 10);
		}

		public static T RandomChoice<T>(this IEnumerable<T> source) {
			var arr = source.ToArray();
			return arr[Instance.Next(arr.Length)];
		}
	}
}
