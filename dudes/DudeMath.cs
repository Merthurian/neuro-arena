using System;

namespace Things
{
	public static class DudeMath
	{
		public static double a_diff(double a1, double a2)
		{  
			double d = a2 - a1;

			while (d < -Math.PI)
				d += 2 * Math.PI;

			while (d > Math.PI)
				d -= 2*Math.PI;

			return d;  
		}
	}
}

