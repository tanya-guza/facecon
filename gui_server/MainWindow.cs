using System;
using Gtk;
using System.ServiceModel;
using FaceCon.CommandService;
public partial class MainWindow: Gtk.Window
{	
	private ServiceHost serviceHost;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		serviceHost = new ServiceHost(typeof(CommandService));
		var binding = new BasicHttpBinding();
		var address = new Uri("http://localhost:8080");
		serviceHost.AddServiceEndpoint(typeof(ICommandService),
		                               binding, address);
		serviceHost.Open();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		serviceHost.Close();
		Application.Quit ();
		a.RetVal = true;
	}
}
