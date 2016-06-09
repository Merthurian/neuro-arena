using System;
using NeuralNetwork;
using System.Collections.Generic;
using System.Windows;

namespace Things
{
	public struct PhysicsDeets
	{
		public double x, y, a, v, m, d;
	}

	public class Thing
	{
		static int nextID = 0;
		public int id;

		public static List<Thing> things = new List<Thing> ();

		public PhysicsDeets pd;
		public double r, g, b;

		public string name;

		public Thing(string _name)
		{
			id = nextID++;
			name = _name;
			things.Add (this);
		}
	}

	public class Dude : Thing
	{
		NN nn;

		double[] leftEyeSees = new double[3];
		double[] rightEyeSees = new double[3];

		Vector leftEyePos = new Vector ();
		Vector rightEyePos = new Vector ();

		public Dude (string _name, PhysicsDeets _pd)
			: base (_name)
		{
			pd = _pd;
		}

		public void checkEyes()
		{
			leftEyeSees [0] = 0.0;   
			leftEyeSees [1] = 0.0;
			leftEyeSees [2] = 0.0;
			rightEyeSees [0] = 0.0;
			rightEyeSees [1] = 0.0;
			rightEyeSees [2] = 0.0;

			double leftAngle = pd.a - 0.32;
			double rightAngle = pd.a + 0.32;

			double range = 0.74;

			foreach (var thingy in Thing.things) {
				if (thingy.id == this.id) 
					continue;

				Vector v1 = new Vector (pd.x, pd.y);
				Vector v2 = new Vector (thingy.pd.x, thingy.pd.y);

				double ab = Math.Atan2 (thingy.pd.x - pd.x, thingy.pd.y - pd.y);

				double dist = (v1 - v2).Length;

				double ld = DudeMath.a_diff (leftAngle, ab);
				double rd = DudeMath.a_diff (rightAngle, ab);

				if ((ld > -range) && (ld < range))
				{       
					leftEyeSees[0] += 16*(thingy.r/dist);
					leftEyeSees[1] += 16*(thingy.g/dist);
					leftEyeSees[2] += 16*(thingy.b/dist);  
				}
				else if ((rd > -range) && (rd < range))
				{ 
					rightEyeSees[0] += 16*(thingy.r/dist);
					rightEyeSees[1] += 16*(thingy.g/dist);
					rightEyeSees[2] += 16*(thingy.b/dist);    
				}

				leftEyeSees[0] = Math.Tanh(leftEyeSees[0]);   
				leftEyeSees[1] = Math.Tanh(leftEyeSees[1]);
				leftEyeSees[2] = Math.Tanh(leftEyeSees[2]);
				rightEyeSees[0] = Math.Tanh(rightEyeSees[0]);
				rightEyeSees[1] = Math.Tanh(rightEyeSees[1]);
				rightEyeSees[2] = Math.Tanh(rightEyeSees[2]);

				leftEyePos.X = pd.x + Math.Sin (leftAngle) * 4;
				leftEyePos.Y = pd.y + Math.Cos (leftAngle) * 4;
				rightEyePos.X = pd.x + Math.Sin (leftAngle) * 4;
				rightEyePos.Y = pd.y + Math.Cos (leftAngle) * 4;
			}
		}

		void update()
		{
			double[] ins = {leftEyeSees[1],
				leftEyeSees[1],
				leftEyeSees[2],
				rightEyeSees[0],
				rightEyeSees[1],
				rightEyeSees[2]};

			double[] outs = nn.FeedForward (ins);
		}
	}
}

