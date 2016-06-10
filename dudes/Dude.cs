using System;
using NeuralNetwork;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using DudeWorld;

namespace Things
{
	public struct Spacials
	{
		public double x, y, angle, velocity, mass, d;
	}

	public class Thing
	{
		static int nextID = 0;
		public int id;

		public static World world = new World();

		public static List<Thing> things = new List<Thing> ();

		public Spacials spacials;
		public double red, green, blue;

		public string name;

		public Thing(string _name)
		{
			id = nextID++;
			name = _name;
			things.Add (this);
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
		energy,
		charge,
		feedback0,
		feedback1,
		feedback2
	}


	public enum nnOutputs
	{
		colorR,
		colorG,
		colorB,
		thrust,
		turn,
		act,
		eyeAngle,
		focus,
		feedback0,
		feedback1,
		feedback2
	}


	public class Dude : Thing
	{
		NN nn = new NN (Enum.GetNames (typeof(nnInputs)).Length, Enum.GetNames (typeof(nnOutputs)).Length, 3, 3);

		double[] leftEyeSense = new double[3];
		double[] rightEyeSense = new double[3];

		double[] ins = new double[Enum.GetNames (typeof(nnInputs)).Length];
		double[] outs = new double[Enum.GetNames (typeof(nnOutputs)).Length];

		double eyeAngle = 2; 	//nn will controll these both
		double focus = 0.1;			//between mapping to 0 - 2

		double turn;

		double pressure = 0;

		double charge = 0;
		double energy = 0;

		//Vector leftEyePos = new Vector ();
		//Vector rightEyePos = new Vector ();

		public Dude (string _name, Spacials sp)
			: base (_name)
		{
			spacials = sp;
		}

		public void ponder()
		{
			ins [(int)nnInputs.leftEyeR] = leftEyeSense [0];
			ins [(int)nnInputs.leftEyeG] = leftEyeSense [1];
			ins [(int)nnInputs.leftEyeB] = leftEyeSense [2];
			ins [(int)nnInputs.rightEyeR] = rightEyeSense [0];
			ins [(int)nnInputs.rightEyeG] = rightEyeSense [1];
			ins [(int)nnInputs.rightEyeB] = rightEyeSense [2];
			ins [(int)nnInputs.pressrue] = pressure;
			ins [(int)nnInputs.feedback0] = outs[(int)nnOutputs.feedback0];
			ins [(int)nnInputs.feedback1] = outs[(int)nnOutputs.feedback1];
			ins [(int)nnInputs.feedback2] = outs[(int)nnOutputs.feedback2];

			outs = nn.FeedForward (ins);
		}

		public void act()
		{
			energy = DudeMath.constrain (energy + world.energy,0,1);
			if (outs [(int)nnOutputs.act] < 0) {
				charge = DudeMath.constrain (charge + world.charge,0,1);
				return;
			}

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
				turn -= 0.2;
			if (turn < -0.1)
				turn += 0.2;

			spacials.angle = DudeMath.wrapPi (spacials.angle + turn);
			spacials.velocity = DudeMath.constrain (spacials.velocity + (outs [(int)nnOutputs.thrust] * 0.05),-1,1);
			spacials.velocity *= 1 - world.friction;

			spacials.x += Math.Sin (spacials.angle) * spacials.velocity * 5;
			spacials.y += Math.Cos (spacials.angle) * spacials.velocity * 5;
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

				Vector v1 = new Vector (spacials.x, spacials.y);
				Vector v2 = new Vector (thingy.spacials.x, thingy.spacials.y);

				double angleBetweenDudes = Math.Atan2 (thingy.spacials.x - spacials.x, thingy.spacials.y - spacials.y);

				double dist = (v1 - v2).Length;
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

		void update()
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

			Dude spinDude = new Dude ("Spin", pd);
			spinDude.red = 0;
			spinDude.green = 0;
			spinDude.blue = 0;

			//

//			Dude redDude = new Dude ("Red", pd);
//			redDude.red = 0;
//			redDude.green = 0;
//			redDude.blue = 0;

			//farDude.pd.x = 100;
//			redDude.spacials.x = 10;



			Dude greenDude = new Dude ("Green", pd);
			greenDude.red = 1;
			greenDude.green = 1;
			greenDude.blue = 1;

			//farDude.pd.x = 100;
			greenDude.spacials.x = -10;

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

			spacials.x = 0;
			spacials.y = 0;
			spacials.angle = 0;
			spacials.velocity = 0;
			spacials.mass = 1;
			spacials.d = 1;

			return spacials;
		}
	}

}

