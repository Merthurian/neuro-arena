using System;
using NeuralNetwork;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using DudeWorld;
using System.Security.Cryptography;

namespace Things
{
	public class Thing
	{
		static int nextID = 0;
		public int id;

		public static Random random = new Random ();

		public static World world;

		public static List<Thing> things = new List<Thing> ();

		public Spacials spacials;
		public double red, green, blue;

		public Thing()
		{
			id = nextID++;
			things.Add (this);
			spacials = world.dudeSpacials;
		}

		public static void init (World _world){
			world = _world;
		}

	}

	public enum nnInputs
	{
		leftEyeR,
		leftEyeG,
		leftEyeB,
		rightEyeR,
		rightEyeG,
		rightEyeB,
		pressrue,
		distToCentre,
		energy,
		charge,
		feedback0,
		feedback1,
		feedback2,
		feedback3,
		thrusterA0,
		thrusterA1,
		thrusterA2,
		thrusterA3
	}
		
	public enum nnOutputs
	{
		colorR,
		colorG,
		colorB,
		turn,
		eyeAngle,
		focus,
		feedback0,
		feedback1,
		feedback2,
		feedback3,
		thrusterA0,
		thrusterA1,
		thrusterA2,
		thrusterA3,
		thrusterT0,
		thrusterT1,
		thrusterT2,
		thrusterT3
	}

	public class Dude : Thing
	{
		NN nn = new NN (Enum.GetNames (typeof(nnInputs)).Length, Enum.GetNames (typeof(nnOutputs)).Length, 3, 3);

		public static List<Dude> allTheDudes = new List<Dude> ();

		public double[] leftEyeSense = new double[3];
		public double[] rightEyeSense = new double[3];

		double[] ins = new double[Enum.GetNames (typeof(nnInputs)).Length];
		double[] outs = new double[Enum.GetNames (typeof(nnOutputs)).Length];

		double[,] thrusters = new double[4,2];//angle

		public double eyeAngle = 1; 	//nn will controll these both
		public double focus = 1;		//between mapping to 0 - 2

		double turn;
		double distToCentre = 0;

		double pressure = 0;

		double charge = 0;
		double energy = 0;

		//Vector leftEyePos = new Vector ();
		//Vector rightEyePos = new Vector ();

		static public void SpawnDudes(int howManyDudes)
		{
			for (int i = 0; i < howManyDudes; i++) {
				Dude dude = new Dude ();
				allTheDudes.Add (dude);
			}
		}

		public Dude()
		{
			double r = world.radius;

			Vector point = new Vector (DudeMath.map (random.NextDouble (), 0, 1, -r, r),
				DudeMath.map (random.NextDouble (), 0, 1, -r, r));

			while((point - spacials.pos).Length > world.radius)
				point = new Vector (DudeMath.map (random.NextDouble (), 0, 1, -r, r),
					DudeMath.map (random.NextDouble (), 0, 1, -r, r));

			spacials.angle = DudeMath.map (random.NextDouble (), 0, 1, -Math.PI, Math.PI);

			teleport (point);
		}

		public Dude (Spacials sp)
		{
			spacials = sp;
		}

		public void teleport (Vector pos)
		{
			spacials.pos = pos;
		}

		public void ponder()
		{
			distToCentre = (new Vector(0,0) - spacials.pos).Length;
	
			ins [(int)nnInputs.leftEyeR] = leftEyeSense [0];
			ins [(int)nnInputs.leftEyeG] = leftEyeSense [1];
			ins [(int)nnInputs.leftEyeB] = leftEyeSense [2];
			ins [(int)nnInputs.rightEyeR] = rightEyeSense [0];
			ins [(int)nnInputs.rightEyeG] = rightEyeSense [1];
			ins [(int)nnInputs.rightEyeB] = rightEyeSense [2];
			ins [(int)nnInputs.pressrue] = pressure;
			ins [(int)nnInputs.distToCentre] = distToCentre;

			ins [(int)nnInputs.feedback0] = outs[(int)nnOutputs.feedback0];
			ins [(int)nnInputs.feedback1] = outs[(int)nnOutputs.feedback1];
			ins [(int)nnInputs.feedback2] = outs[(int)nnOutputs.feedback2];
			ins [(int)nnInputs.feedback3] = outs[(int)nnOutputs.feedback3];
			ins [(int)nnInputs.thrusterA0] = thrusters [0,0];
			ins [(int)nnInputs.thrusterA1] = thrusters [1,0];
			ins [(int)nnInputs.thrusterA2] = thrusters [2,0];
			ins [(int)nnInputs.thrusterA3] = thrusters [3,0];

			outs = nn.FeedForward (ins);
		}

		public void act()
		{
			energy = DudeMath.constrain (energy + world.energy,0,1);
			charge = DudeMath.constrain (charge + world.charge,0,1);
			eyeAngle = DudeMath.constrain(eyeAngle + (outs[(int)nnOutputs.eyeAngle] * 0.01), 0, 2);
			focus = DudeMath.map (outs [(int)nnOutputs.eyeAngle], -1, 1, 0, 2);
			red = DudeMath.map(outs [(int)nnOutputs.colorR],-1,1,0,1);
			green = DudeMath.map(outs [(int)nnOutputs.colorG],-1,1,0,1);
			blue = DudeMath.map(outs [(int)nnOutputs.colorB],-1,1,0,1);
		}

		public void manouver()
		{
			turn += outs [(int)nnOutputs.turn] * 0.03;

			if (turn > 0.1)
				turn = 0.1;
			if (turn < -0.1)
				turn = 0.1;

			spacials.angle = DudeMath.wrapPi (spacials.angle + turn);

			doThrusters ();
		}
		public void doThrusters()
		{
			thrusters[0,0] = DudeMath.wrapPi (thrusters[0,0] + (outs[(int)nnOutputs.thrusterA0]*0.1));
			thrusters[1,0] = DudeMath.wrapPi (thrusters[1,0] + (outs[(int)nnOutputs.thrusterA1]*0.1));
			thrusters[2,0] = DudeMath.wrapPi (thrusters[2,0] + (outs[(int)nnOutputs.thrusterA2]*0.1));
			thrusters[3,0] = DudeMath.wrapPi (thrusters[3,0] + (outs[(int)nnOutputs.thrusterA3]*0.1));


			thrusters[0,1] = DudeMath.map (outs [(int)nnOutputs.thrusterT0], -1, 1, 0, 1);
			thrusters[1,1] = DudeMath.map (outs [(int)nnOutputs.thrusterT1], -1, 1, 0, 1);
			thrusters[2,1] = DudeMath.map (outs [(int)nnOutputs.thrusterT2], -1, 1, 0, 1);
			thrusters[3,1] = DudeMath.map (outs [(int)nnOutputs.thrusterT3], -1, 1, 0, 1);


			for (int i = 0; i < 4; i++) {
				double angle = DudeMath.wrapPi (spacials.angle + thrusters [i,0]);

				spacials.pos.X += Math.Sin (angle) * thrusters[i,1] * 5;
				spacials.pos.Y += Math.Cos (angle) * thrusters[i,1] * 5;
			}
		}

		public void checkSenses()
		{
			leftEyeSense [0] = 0.0;   
			leftEyeSense [1] = 0.0;
			leftEyeSense [2] = 0.0;
			rightEyeSense [0] = 0.0;
			rightEyeSense [1] = 0.0;
			rightEyeSense [2] = 0.0;
			pressure = 0;

			double leftAngle = DudeMath.wrapPi(spacials.angle - eyeAngle);
			double rightAngle = DudeMath.wrapPi (spacials.angle + eyeAngle);

			foreach (var thingy in Thing.things) {
				if (thingy.id == this.id) 
					continue;

				double angleBetweenDudes = Math.Atan2 (thingy.spacials.pos.X - spacials.pos.X, thingy.spacials.pos.Y - spacials.pos.Y);

				double dist = (spacials.pos - thingy.spacials.pos).Length;
				pressure += dist*thingy.spacials.mass; //See tanh later, just taking advantage of this expensive number

				double leftEyeDifference = DudeMath.aDiff (leftAngle, angleBetweenDudes);
				double rightEyeDifference = DudeMath.aDiff (rightAngle, angleBetweenDudes);

				if ((leftEyeDifference > -focus) && (leftEyeDifference < focus))
				{       
					leftEyeSense[0] += 16*(thingy.red/dist);
					leftEyeSense[1] += 16*(thingy.green/dist);
					leftEyeSense[2] += 16*(thingy.blue/dist);  
				}
				if ((rightEyeDifference > -focus) && (rightEyeDifference < focus))
				{ 
					rightEyeSense[0] += 16*(thingy.red/dist);
					rightEyeSense[1] += 16*(thingy.green/dist);
					rightEyeSense[2] += 16*(thingy.blue/dist);    
				}

				leftEyeSense[0] = Math.Tanh(leftEyeSense[0]);   
				leftEyeSense[1] = Math.Tanh(leftEyeSense[1]);
				leftEyeSense[2] = Math.Tanh(leftEyeSense[2]);
				rightEyeSense[0] = Math.Tanh(rightEyeSense[0]);
				rightEyeSense[1] = Math.Tanh(rightEyeSense[1]);
				rightEyeSense[2] = Math.Tanh(rightEyeSense[2]);

//				leftEyePos.X = pd.x + Math.Sin (leftAngle) * 4;
//				leftEyePos.Y = pd.y + Math.Cos (leftAngle) * 4;
//				rightEyePos.X = pd.x + Math.Sin (leftAngle) * 4;
//				rightEyePos.Y = pd.y + Math.Cos (leftAngle) * 4;
			}

			pressure = Math.Tanh (pressure);//Squish it down
		}

		public void update()
		{
			checkSenses ();
			ponder ();
			act ();
			manouver ();
		}

		public static void EyeTest()
		{
			//Two dudes. One at 0,0 the other at 100, 0
			//the first one just spins and we get

			Spacials pd = defaultSpacials ();

			//Dude spinDude = new Dude ("Spin", pd);
			Dude spinDude = new Dude ();
			spinDude.red = 0;
			spinDude.green = 0;
			spinDude.blue = 0;

			spinDude.update ();

			//

//			Dude redDude = new Dude ("Red", pd);
//			redDude.red = 0;
//			redDude.green = 0;
//			redDude.blue = 0;

			//farDude.pd.x = 100;
//			redDude.spacials.x = 10;



			//Dude greenDude = new Dude ("Green", pd);
			Dude greenDude = new Dude ();
			greenDude.red = 1;
			greenDude.green = 1;
			greenDude.blue = 1;

			//farDude.pd.x = 100;
			greenDude.spacials.pos.X = -10;

//
//			Dude blueDude = new Dude ("Blue", pd);
//			blueDude.red = 0;
//			blueDude.green = 0;
//			blueDude.blue = 0;
//
//			blueDude.spacials.y = 10;

			//

			string output = "";			

			for (double a = -Math.PI; a < Math.PI; a += 0.1) {
				spinDude.spacials.angle = a;

				spinDude.checkSenses ();

				output += spinDude.leftEyeSense[0].ToString() + ",";
				output += spinDude.leftEyeSense[1].ToString() + ",";
				output += spinDude.leftEyeSense[2].ToString() + ", ,";
				output += spinDude.rightEyeSense[0].ToString() + ",";
				output += spinDude.rightEyeSense[1].ToString() + ",";
				output += spinDude.rightEyeSense [2].ToString () + ",";
				output += spinDude.spacials.angle.ToString("F2") + "\r\n";
			}

			using (StreamWriter sw = new StreamWriter("EyeTest.csv"))
			{
				sw.Write(output);
			}				
		}

		public static Spacials defaultSpacials()
		{
			Spacials spacials = new Spacials ();

			spacials.pos = new Vector();
			spacials.angle = 0;
			spacials.mass = 1;
			spacials.radius = 1;

			return spacials;
		}
	}

}

