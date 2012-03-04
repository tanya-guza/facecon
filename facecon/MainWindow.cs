using System;

using System.Runtime.InteropServices;
using System.Collections;
using System.Xml;
using System.Text;
using System.ServiceModel;
using System.Xml.Serialization;
using System.IO;

using Vte;
using Gtk;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceCon.FaceCon
{
/// <summary>
/// Main window class. Hosts connection settings
/// and viewfinder for webcamera
/// </summary>
	public class MainWindow: Window
	{
	#region Gtreamer components
		/// <summary>
		/// The pipeline.
		/// </summary>
		private Gst.Pipeline pipeline;
		/// <summary>
		/// GStreamer Bin for interacting with web-camera
		/// </summary>
		private Gst.Element camerabin;
		/// <summary>
		/// Gstreamer element for displaying web-camera image in GtkDrawingArea
		/// </summary>
		private Gst.Element drawSink;
		/// <summary>
		/// Helper adapter
		/// </summary>
		private Gst.Interfaces.XOverlayAdapter overlayAdapter;
	#endregion
	
	
	#region User interface
		/// <summary>
		/// Виджет для отображения вывода камеры.
		/// </summary>
		private  DrawingArea drawingArea;
	
		/// <summary>
		/// "Host" label
		/// </summary>
		private Label labelHost;
		/// <summary>
		/// "Port" label
		/// </summary>
		private Label labelPort;
	
		/// <summary>
		/// Host entry
		/// </summary>
		private Entry entryHost;
	
		/// <summary>
		/// Port entry
		/// </summary>
		private Entry entryPort;
	
	
		/// <summary>
		/// "Authenticate" button
		/// </summary>
		private Button authButton;
	#endregion
	
		private CommandClient client;
	
		public MainWindow (): base (WindowType.Toplevel)
		{
			BuildInterface ();
		
			pipeline = new Gst.Pipeline ();
			drawSink = Gst.ElementFactory.Make ("xvimagesink");
			camerabin = Gst.ElementFactory.Make ("camerabin");
			camerabin.Connect ("image-done", new Gst.SignalHandler (OnImageDone));
			pipeline.SetState (Gst.State.Null);
	
			overlayAdapter = new Gst.Interfaces.XOverlayAdapter (drawSink.Handle);
			overlayAdapter.XwindowId = gdk_x11_drawable_get_xid (drawingArea.GdkWindow.Handle);
			pipeline.Add (camerabin);
		
			if (camerabin.HasProperty ("viewfinder-sink")) {
				camerabin ["viewfinder-sink"] = drawSink;
			}
		
			if (camerabin.HasProperty ("filename")) {
				camerabin ["filename"] = "snapshot.png";
			}
	
			pipeline.SetState (Gst.State.Playing);
			this.ShowAll ();
		
		}
	
		/// <summary>
		/// Method for initialization of User Interface
		/// </summary>
		private void BuildInterface ()
		{
			this.DeleteEvent += new DeleteEventHandler (OnDelete);
		
			authButton = new Button ();
			authButton.Label = "Authenticate";
			authButton.Clicked += new EventHandler (OnAuthButtonClick);
		
			drawingArea = new DrawingArea ();
			drawingArea.WidthRequest = 320;
			drawingArea.HeightRequest = 240;
		
			labelHost = new Label ("Host");
			labelPort = new Label ("Port");
		
			entryHost = new Entry ("localhost");
			entryPort = new Entry ("8080");
		
			VBox vbox = new VBox ();
			Table table = new Table (3, 2, false);
			table.Attach (labelHost, 0, 1, 0, 1);
			table.Attach (labelPort, 1, 2, 0, 1);
			table.Attach (entryHost, 0, 1, 1, 2);
			table.Attach (entryPort, 1, 2, 1, 2);
			table.Attach (authButton, 0, 2, 2, 3);
			vbox.PackStart (table);
			vbox.PackStart (drawingArea);
			this.Add (vbox);
			this.ShowAll ();
		}
	
		private void OnDelete (object o, DeleteEventArgs args)
		{
			pipeline.SetState (Gst.State.Null);
		}
	
		/// <summary>
		/// Authentication button click handler
		/// </summary>
		private void OnAuthButtonClick (object o, EventArgs args)
		{
			camerabin.Emit ("capture-start", new object[]{});
			camerabin.Emit ("capture-stop", new object[]{});
		
		}
	
		private void OnImageDone (object o, Gst.GLib.SignalArgs args)
		{
			Emgu.CV.Image <Bgr, byte> sourceImage = 
			new Emgu.CV.Image<Bgr, byte> ("snapshot.png");
		
			// Image conversion
			ImageProcessor processor = new ImageProcessor (sourceImage);
		
			// Face detection
			var detector = new FaceDetector ("haarcascade_frontalface_alt2.xml",
		    processor.NormalizedImage);
		
			Image<Gray, byte> drawedFace = processor.GrayscaleImage;
		
			if (detector.processImage (ref drawedFace)) {
				Title = "Лицо найдено. Данные отправляются на сервер";
				var binding = new BasicHttpBinding ();
				var address = new EndpointAddress ("http://" + entryHost.Text + ":" + entryPort.Text);
				client = new CommandClient (binding, address);
				Console.WriteLine (client.authenticate (serializeImage (processor.NormalizedImage)));
				Console.WriteLine (client.executeCommand ("dmesg", ""));
			
			} else {
				Title = "В видоискателе нет лица";
			}
			/*	
		using (Image<Bgr, byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0))) {
			MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
			Emgu.CV.CvInvoke.cvNamedWindow("w1");
			CvInvoke.cvShowImage("w1", img.Ptr);
			CvInvoke.cvWaitKey (0);
			//Destory the window
			CvInvoke.cvDestroyWindow("w1");
		} */
		}

		private string serializeImage (Image<Gray, byte> image)
		{
			var sb = new System.Text.StringBuilder ();
			(new XmlSerializer (typeof(Image<Gray, Byte>))).Serialize (new StringWriter (sb), image);
		
			return sb.ToString ();
		}

		[DllImport ("libgdk-x11-2.0.so.0") ]
		static extern uint gdk_x11_drawable_get_xid (IntPtr handle);
	}
}