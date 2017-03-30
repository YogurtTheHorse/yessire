using System;
using System.Collections.Generic;

namespace YesSir.Backend.Helpers {
	public static class Utils {
		public static U Get<T, U>(this Dictionary<T, U> dict, T key, U def=default(U)) {
			U val = def;
			dict.TryGetValue(key, out val);
			return val;
		}

		public static bool NearlyEqual(this float a, float b, float epsilon=0.001f) {
			float diff = Math.Abs(a - b);

			return diff <= epsilon;
		}
	}
}
