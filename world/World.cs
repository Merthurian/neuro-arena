using System;
using System.Windows;

namespace DudeWorld
{
	public struct Spacials
	{
		public double angle, mass, radius;
		public Vector pos;
	}
	public class World
	{
		public double friction = 0.1;
		public double energy = 0.01;
		public double charge = 0.1;
		public double radius = 100;
		public Spacials dudeSpacials;
		public bool ready = false;
		public World ()
		{
			dudeSpacials.mass = 1;
			dudeSpacials.radius = 5;

			ready = true;
		}
	}
}

