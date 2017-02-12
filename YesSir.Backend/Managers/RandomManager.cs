using System;

namespace YesSir.Backend.Managers {
	public static class RandomManager {
		public static Random Instance = new Random();

		public static int Next(int a, int b) {
			return Instance.Next(a, b);
		}

		public static float NextGoodSkill() {
			return 0.45f + (float)Instance.NextDouble() / 10f;
		}

		public static T Select<T>() where T : struct, IConvertible {
			if (!typeof(T).IsEnum) {
				throw new ArgumentException("T must be an enum");
			}
			T[] t = (T[])Enum.GetValues(typeof(T));
			return t[Next(0, t.Length)];
		}

		public static float NextDefaultSkill() {
			return 0.1f + (float)Instance.NextDouble() / 5f;
		}
	}
}
