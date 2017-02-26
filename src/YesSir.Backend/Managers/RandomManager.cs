using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YesSir.Backend.Entities;

using static System.Math;

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

		public static Point GenerateKingdomPoint() {
			int chunk_seed = 0,
				x = Instance.Next(int.MinValue, int.MaxValue),
			    y = Instance.Next(int.MinValue, int.MaxValue);

			int chunk_x = (chunk_seed % 5) * ((chunk_seed / 25) % 2 == 0 ? -1 : 1),
				chunk_y = ((chunk_seed / 5) % 5) * ((chunk_seed / 250) % 2 == 0 ? -1 : 1);

			int MAX_R = 1000;
			double k = (double)MAX_R / int.MaxValue;
			float near_k = 1.5f;

			x = (int)(Pow(((Abs(x) * k) / MAX_R), near_k) * MAX_R) * Sign(x);
			y = (int)(Pow(((Abs(y) * k) / MAX_R), near_k) * MAX_R) * Sign(y);

			x += chunk_x * MAX_R * 2;
			y += chunk_y * MAX_R * 2;

			return new Point(x, y);
		}
	}
}
