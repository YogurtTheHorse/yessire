using System.Collections.Generic;

namespace YesSir.Backend.Helpers {
	public static class Utils {
		public static U Get<T, U>(this Dictionary<T, U> dict, T key, U def=default(U)) {
			U val = def;
			dict.TryGetValue(key, out val);
			return val;
		}
	}
}
