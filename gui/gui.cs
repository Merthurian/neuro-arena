using Gtk;
using DudeWorld;
using Things;
using Gdk;
using Cairo;
using System;
using System.Windows;
using System.Threading;

class SharpApp : Gtk.Window {

	World world = new World();

	public SharpApp() : base("Center")
	{
		//Backend Dude stuff
		Thing.init(world);

		Dude.SpawnDudes (2);

		//Gtk and Cairo things
		SetDefaultSize(400, 400);
		SetPosition(WindowPosition.Center);
		DeleteEvent += delegate { Application.Quit(); };

		DrawingArea darea = new DrawingArea ();
		darea.ExposeEvent += OnExpose;

		Add (darea);

		ShowAll();
	}

	void OnExpose(object sender, ExposeEventArgs args)
	{
		DrawingArea darea = (DrawingArea)sender;
		Cairo.Context cairoContext = Gdk.CairoHelper.Create(darea.GdkWindow);

		int width, height;
		width = Allocation.Width;
		height = Allocation.Height;

		cairoContext.Translate(width/2, height/2);

		Circle (cairoContext, 0, 0, world.radius, 0,0,0);

		foreach (var dude in Dude.allTheDudes) {
			dude.update ();
			Circle (cairoContext, dude.spacials.pos.X, dude.spacials.pos.Y, dude.spacials.radius, dude.red,dude.green,dude.blue);
			Arc(cairoContext, 
				dude.spacials.pos.X,
				dude.spacials.pos.Y,
				dude.spacials.radius*1.2,
				-dude.eyeAngle - dude.focus/2,
				-dude.eyeAngle + dude.focus/2,
				dude.leftEyeSense[0],
				dude.leftEyeSense[1],
				dude.leftEyeSense[2]);

			Arc(cairoContext, 
				dude.spacials.pos.X,
				dude.spacials.pos.Y,
				dude.spacials.radius*1.2,
				dude.spacials.angle + dude.eyeAngle - dude.focus/2,
				dude.spacials.angle + dude.eyeAngle + dude.focus/2,
				dude.rightEyeSense[0],
				dude.rightEyeSense[1],
				dude.rightEyeSense[2]);
			
		}


		#region cleanup
		((IDisposable) cairoContext.Target).Dispose();                                      
		((IDisposable) cairoContext).Dispose();
		#endregion
	}

	void Circle (Cairo.Context cairoContext, double x, double y, double radius, double r, double g, double b)
	{		
		cairoContext.SetSourceRGB(r, g, b);
		cairoContext.Arc (x, y, radius, 0, Math.PI*2);
		cairoContext.Stroke ();
	}
	void Arc (Cairo.Context cairoContext, double x, double y, double radius, double start, double stop,double r, double g, double b)
	{		
		cairoContext.SetSourceRGB(r, g, b);
		cairoContext.Arc (x, y, radius, DudeMath.wrapPi(start), DudeMath.wrapPi(stop));
		cairoContext.Stroke ();
	}

	public static void Main()
	{
		Application.Init();
		new SharpApp();        
		Application.Run();
	}
}
