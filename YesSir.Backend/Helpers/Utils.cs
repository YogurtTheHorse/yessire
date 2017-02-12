using System.Collections.Generic;

namespace YesSir.Backend.Helpers {
	public static class Utils {
		public static U Get<T, U>(this Dictionary<T, U> dict, T key) {
			U val = default(U);
			dict.TryGetValue(key, out val);
			return val;
		}
	}
}
