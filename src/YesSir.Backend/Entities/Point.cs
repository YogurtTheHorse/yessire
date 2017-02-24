using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YesSir.Backend.Entities {
	public class Point {
		public long X { get; set; }
		public long Y { get; set; }

		public Point() : this(0) {  }

		public Point(int x) : this(x, x) { }

		public Point(long x, long y) {
			X = x;
			Y = y;
		}

		public float Distance(Point coordinate) {
			long x = X - coordinate.X,
				 y = Y - coordinate.Y;

			return (float)Math.Sqrt(x * x + y * y);
		}
	}
}
