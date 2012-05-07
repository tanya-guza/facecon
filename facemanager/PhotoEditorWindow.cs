using System;
using System.Runtime.InteropServices;
using System.Drawing;
using Gdk;

using Gtk;

using Gst;
using Gst.Interfaces;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using FaceCon.CommandService;

namespace FaceCon.FaceManager
{
	public class PhotoEditorWindow : Gtk.Dialog
	{
		public Image<Gray, byte> Photo { get; set; }
		#region UI controls
		private DrawingArea drawingArea;
		private Button captureButton;
		private Button previewButton;
		private Button saveButton;
		private Button cancelButton;
		#endregion
		
		#region Gtreamer components
		/// <summary>
		/// The pipeline.
		/// </summary>
		private Pipeline pipeline;
		/// <summary>
		/// GStreamer Bin for interacting with web-camera
		/// </summary>
		private Element camerabin;
		/// <summary>
		/// Gstreamer element for displaying web-camera image in GtkDrawingArea
		/// </summary>
		private Element drawSink;
		/// <summary>
		/// Helper adapter
		/// </summary>
		private XOverlayAdapter overlayAdapter;
		#endregion
		
		private bool needReInit;
		
		public PhotoEditorWindow (Gtk.Window Parent) : base("Photo Editor", Parent,
		                                                    DialogFlags.Modal)
		{
			BuildInterface ();
			ReInitPipeline ();
			
		}
		
		private void ReInitPipeline ()
		{
			if (pipeline != null) {
				pipeline.SetState (Gst.State.Null);
				pipeline = null;
			}
			
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
			needReInit = false;
		}
		
		private void BuildInterface ()
		{
			VBox vbox = this.Child as VBox;
			HButtonBox buttonBox = new HButtonBox ();
			
			drawingArea = new DrawingArea ();
			drawingArea.SetSizeRequest (640, 480);
			
			captureButton = new Button ("Capture");
			previewButton = new Button ("Preview");
			previewButton.Sensitive = false;
			saveButton = new Button ("Save");
			saveButton.Sensitive = false;
			cancelButton = new Button ("Cancel");
			buttonBox.PackStart (captureButton);
			buttonBox.PackStart (previewButton);
			buttonBox.PackStart (saveButton);
			buttonBox.PackStart (cancelButton);
			buttonBox.LayoutStyle = ButtonBoxStyle.Center;
			
			vbox.PackStart (drawingArea);
			vbox.PackStart (buttonBox, false, true, 8);
				
			this.ShowAll ();
			
			this.DeleteEvent += new DeleteEventHandler(OnDelete);
			captureButton.Clicked += new EventHandler (CaptureButtonClicked);
			previewButton.Clicked += new EventHandler(PreviewButtonClicked);
			saveButton.Clicked += new EventHandler (SaveButtonClicked);
			cancelButton.Clicked += new EventHandler (CancelButtonClicked);
		}
		
		private void CaptureButtonClicked (object sender, EventArgs args)
		{
			if (!needReInit) {
				needReInit = true;
				camerabin.Emit ("capture-start", new object[]{});
				camerabin.Emit ("capture-stop", new object[]{});
				captureButton.Label = "Resume";
				saveButton.Sensitive = true;
				previewButton.Sensitive = true;
			} else {
				captureButton.Label = "Capture";
				ReInitPipeline ();
				saveButton.Sensitive = false;
				previewButton.Sensitive = false;
				CvInvoke.cvDestroyWindow ("Face Preview");
			}
		}
		
		private void PreviewButtonClicked (object sender, EventArgs args)
		{
			PreviewResult();
		}
		
		private void SaveButtonClicked (object sender, EventArgs args)
		{
			pipeline.SetState (Gst.State.Null);
			CvInvoke.cvDestroyWindow ("Face Preview");
			this.Hide();
		}
		
		private void CancelButtonClicked (object sender, EventArgs args)
		{
			pipeline.SetState (Gst.State.Null);
			Photo = null;
			CvInvoke.cvDestroyWindow ("Face Preview");
			this.Hide();
		}
		
		private void OnImageDone (object o, Gst.GLib.SignalArgs args)
		{
			pipeline.SetState (Gst.State.Null);

			Emgu.CV.Image <Bgr, byte> sourceImage = 
			new Emgu.CV.Image<Bgr, byte> ("snapshot.png");
		
			// Image conversion
			ImageProcessor processor = new ImageProcessor (sourceImage);
		
			// Face detection
			var detector = new FaceDetector ("/usr/local/share/OpenCV/haarcascades/haarcascade_frontalface_alt2.xml",
		    processor.NormalizedImage);
		
			Image<Gray, byte> grayFace = processor.GrayscaleImage;
		
			System.Drawing.Rectangle rect = new System.Drawing.Rectangle ();
			if (detector.processImage (grayFace, out rect)) {
				Title = "Face found";
				Photo = grayFace.GetSubRect(rect);
				//PreviewResult();
			} else {
				Title = "Face not found";
			}
		}
		
		private void OnDelete(object sender, EventArgs args)
		{
			CvInvoke.cvDestroyWindow ("Face Preview");
		}
		
		void PreviewResult()
		{
			if (Photo != null)
			{
				Emgu.CV.CvInvoke.cvNamedWindow ("Face Preview");
				CvInvoke.cvShowImage ("Face Preview", Photo.Clone());
				CvInvoke.cvWaitKey (0);
				//Destory the window
				Console.WriteLine("Here");
				CvInvoke.cvDestroyWindow ("Face Preview");	
			}
		}
		
		[DllImport ("libgdk-x11-2.0.so.0") ]
		static extern uint gdk_x11_drawable_get_xid (IntPtr handle);
	}
}

