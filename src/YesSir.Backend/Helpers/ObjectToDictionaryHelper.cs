using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace YesSir.Backend.Helpers {
	public static class ObjectToDictionaryHelper {
		public static IDictionary<string, object> ToDictionary(this object source) {
			return source.ToDictionary<object>();
		}

		public static IDictionary<string, T> ToDictionary<T>(this object source) {
			if (source == null || !source.IsDictionary()) {
				throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is not dictionary.");
			}

			var dictionary = new Dictionary<string, T>();
			foreach (var key in ((IDictionary)source).Keys) {
				dictionary.Add((string)key, (T)((IDictionary)source)[key]);
			}
			return dictionary;
		}

		public static bool IsDictionary(this object source) {
			if (source == null) {
				return false;
			}

			return typeof(IDictionary).GetTypeInfo().IsAssignableFrom(source.GetType());
		}

		private static bool IsOfType<T>(object value) {
			return value is T;
		}
	}
}
