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

		Dude.SpawnDudes (100);

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

		cairoContext.SetSourceRGB(0, 0, 0);
		Circle (cairoContext, 0, 0, world.radius);

		foreach (var dude in Dude.allTheDudes) {
			dude.update ();
			Circle (cairoContext, dude.spacials.pos.X, dude.spacials.pos.Y, dude.spacials.radius);
		}

		#region cleanup
		((IDisposable) cairoContext.Target).Dispose();                                      
		((IDisposable) cairoContext).Dispose();
		#endregion
	}

	void Circle (Cairo.Context cairoContext, double x, double y, double radius)
	{		
		cairoContext.Arc (x, y, radius, 0, Math.PI*2);
		cairoContext.Stroke ();
	}

	public static void Main()
	{
		Application.Init();
		new SharpApp();        
		Application.Run();
	}
}
