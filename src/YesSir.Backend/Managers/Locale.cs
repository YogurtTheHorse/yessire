using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YesSir.Backend.Helpers;

namespace YesSir.Backend.Managers {
	public static class Locale {
		private static Dictionary<string, Dictionary<string, object>> LocalsCache = new Dictionary<string, Dictionary<string, object>>();
		public static string LocalsFolderPath = "locals";

		public static Dictionary<string, object> GetLocale(string language = "ru") {
			if (LocalsCache.ContainsKey(language)) {
				return LocalsCache[language];
			} else {
				using (var stream = new FileStream(LocalsFolderPath + "/" + language + ".yml", FileMode.Open)) {
					using (StreamReader sr = new StreamReader(stream)) {
						Deserializer desirializer = new Deserializer();
						Dictionary<string, object> locale = desirializer.Deserialize<Dictionary<string, object>>(sr);

						LocalsCache[language] = locale;
						return locale;
					}
				}
			}
		}

		public static string[] GetLocales() {
			return new string[] {
				"ru", "en"
			};
		}

		public static bool TryGet<T>(string key, string language, out T obj) {
			List<string> pth = new List<string>(key.Split('.'));
			IDictionary<string, object> locals = GetLocale(language);
			obj = default(T);

			while (pth.Count > 0) {
				string crnt = pth[0];
				if (locals.ContainsKey(crnt)) {
					if (pth.Count == 1) {
						obj = (T)locals[crnt];
						return true;
					} else if (locals[crnt].IsDictionary()) {
						locals = locals[crnt].ToDictionary();
						pth.RemoveAt(0);
					} else {
						return false;
					}
				} else {
					return false;
				}
			}
			
			return false;
		}

		public static string[] GetArray(string key, string language="full") {
			List<string> res = new List<string>();
			List<object> s;
			string[] locals = language == "full" ? GetLocales() : new string[] { language };
			
			foreach (string l in locals) {
				if (TryGet(key, l, out s)) {
					foreach (object ss in s) {
						res.Add(ss.ToString());
					}
				}
			}

			return res.ToArray();
		}

		public static string Get(string key, string language) {
			string s;
			if (TryGet(key, language, out s)) {
				return s;
			} else {
				return string.Format("Unable to find `{0}` in `{1}` locale.", key, language);
			}
		}
	}
}
