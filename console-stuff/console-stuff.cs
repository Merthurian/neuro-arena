using System;
using Things;

namespace consolestuff
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			PhysicsDeets pd = new PhysicsDeets ();
			pd.x = 0;
			pd.y = 0;
			pd.a = 0;
			pd.v = 0;
			pd.m = 1;
			pd.d = 1;

			Dude dude = new Dude ("Jesus", pd);
			dude.r = 0.5;

			pd.x = 100;
			Dude dude2 = new Dude ("Fucker", pd);
			dude2.g = 0.5;

			for (int i = 0; i < 100; i++) {
				dude.pd.a += 0.2;

				if (dude.pd.a > Math.PI)
					dude.pd.a -= 2*Math.PI;
				if (dude.pd.a < -Math.PI)
					dude.pd.a += 2*Math.PI;
				dude.checkEyes ();
			}




			Console.WriteLine (Thing.things[0].name);
		}
	}
}
