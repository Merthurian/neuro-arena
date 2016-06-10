using System;

namespace Things
{
	public static class DudeMath
	{
		public static double aDiff(double a1, double a2)
		{  
			double d = a2 - a1;

			wrapPi (d);

			return d;  
		}

		public static double wrapPi(double a)
		{
			while (a < -Math.PI)
				a += 2 * Math.PI;

			while (a > Math.PI)
				a -= 2*Math.PI;

			return a;
		}

		public static double constrain(double a, double min, double max)
		{
			if (a > max)
				a = max;
			if (a < min)
				a = min;

			return a;
		} 
		static public double map(double value, 
			double istart, 
			double istop, 
			double ostart, 
			double ostop) 
		{
			return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
		}
	}

}

