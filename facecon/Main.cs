using System;
using Gtk;

namespace FaceCon.FaceCon
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			Gst.Application.Init();	
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
			Gst.Application.Deinit();
		}
	}
}
