using System;
using Gtk;
using Gdk;
using System.Drawing;
using System.Drawing.Imaging;
using FaceCon.CommandService;
using System.Runtime.Serialization;
namespace FaceCon.FaceManager
{
	public class MainWindow : Gtk.Window
	{
		#region User Interface Controls
		/// <summary>
		/// The user management buttons. Allow add, edit and delete
		/// new users that can authenticate using face recognition
		/// </summary>
		/*
		private ButtonBox userManagementButtons;
		private Button addUserButton;
		private Button editUserButton;
		private Button deleteUserButton;
		*/
		/// <summary>
		/// The photo management buttons. Allow add and delete photos
		/// from user profile.
		/// </summary>
		private HButtonBox photoManagementButtons;
		private Button addPhotoButton;
		private Button deletePhotoButton;
		/// <summary>
		/// Tree view to display users
		/// </summary>
		private TreeView usersView;
		
		/// <summary>
		/// Tree view to display user photos
		/// </summary>
		private TreeView photosView;
		
		/// <summary>
		/// "Users" label
		/// </summary>
		private Label usersLabel;
		
		/// <summary>
		/// "Photos" label
		/// </summary>
		private Label photosLabel;
		
		/// <summary>
		/// The control buttons.
		/// </summary>
		private ButtonBox controlButtons;
		private Button saveButton;
		private Button cancelButton;
		
		#endregion
		
		UserInfoManager userInfoManager;
		
		public MainWindow (): base (Gtk.WindowType.Toplevel)
		{
			userInfoManager = new UserInfoManager ();
			if (userInfoManager.havePermissions () && userInfoManager.connect ()) {
				BuildInterface ();
				RenderUserList ();
				
			} else {
				Console.WriteLine ("No required permissions!");
				Application.Quit ();
			}
		}
		
		
		#region Event Handlers
		private void AddPhotoButtonClicked (object sender,
		                             EventArgs args)
		{
			PhotoEditorWindow photoEditor = new PhotoEditorWindow (this);
			photoEditor.Run ();
			
			if (photoEditor.Photo != null) {
				Console.WriteLine ("Decided to save!");
				Photo photo = new Photo();
				photo.UserId = GetSelectedUser().Id;
				photo.ImageData = UserInfoManager.SerializeImage(photoEditor.Photo.Clone());
				
				userInfoManager.SavePhoto(photo);
			} else {
				Console.WriteLine ("Cancel!");
			}
		}
		
		private void UsersViewCursorChanged (
			object sender, EventArgs args)
		{
			TreeSelection selection = usersView.Selection;
			Gtk.TreeIter iter;
			Gtk.TreeModel model;
			selection.GetSelected (out model, out iter);
            
			if (NothingIsSelected (iter)) {
				addPhotoButton.Sensitive = false;
			} else {
				RenderPhotoList(GetSelectedUser());
				addPhotoButton.Sensitive = true;
				
			}
			
		}

		private void OnDelete (object sender, DeleteEventArgs args)
		{
			Application.Quit ();
		}
		
		#endregion
		
		#region Renderer helpers
		private void RenderUserId (
            Gtk.TreeViewColumn column,
            Gtk.CellRenderer cell,
            Gtk.TreeModel model,
            Gtk.TreeIter iter)
		{
			User user = (User)model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = user.Id.ToString ();
		}
		
		private void RenderUserUid (
            Gtk.TreeViewColumn column,
            Gtk.CellRenderer cell,
            Gtk.TreeModel model,
            Gtk.TreeIter iter)
		{
			User user = (User)model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = user.Uid.ToString ();
		}
		
		private void RenderUserName (
            Gtk.TreeViewColumn column,
            Gtk.CellRenderer cell,
            Gtk.TreeModel model,
            Gtk.TreeIter iter)
		{
			User user = (User)model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = user.Name;
		}
		
		private void RenderPhotoId (
            Gtk.TreeViewColumn column,
            Gtk.CellRenderer cell,
            Gtk.TreeModel model,
            Gtk.TreeIter iter)
		{
			Photo photo = (Photo)model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = photo.Id.ToString();
		}
		
		private void RenderPhotoImage (
            Gtk.TreeViewColumn column,
            Gtk.CellRenderer cell,
            Gtk.TreeModel model,
            Gtk.TreeIter iter)
		{
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			
			Photo photo = (Photo)model.GetValue (iter, 0);
		
			photo.Image.Save("hello.png");
		
			Console.WriteLine(stream.Capacity);
			Gdk.Pixbuf p = new Gdk.Pixbuf("hello.png");
			
			(cell as Gtk.CellRendererPixbuf).Pixbuf = p;
		}
		
		
		#endregion
		
		#region Utils
		protected User GetSelectedUser()
		{
			TreeSelection selection = usersView.Selection;
			Gtk.TreeIter iter;
			Gtk.TreeModel model;
			selection.GetSelected (out model, out iter);
            
			if (NothingIsSelected (iter)) {
				return null;
			} else {
				return (User)model.GetValue (iter, 0);        
			}
		}
		
		protected bool NothingIsSelected (Gtk.TreeIter iter)
		{
			return iter.Stamp == 0;
		}

		private void RenderUserList ()
		{
			var store = new ListStore (typeof(User));
			
			foreach (var user in userInfoManager.ListUsers()) {
				store.AppendValues (user);
			}
			
			usersView.Model = store;
			
		}
		
		private void RenderPhotoList (User user)
		{
			var store = new ListStore (typeof(Photo));
			
			foreach (var photo in (userInfoManager.ListUserPhotoData(user))) {
				store.AppendValues (photo);
			}
			
			photosView.Model = store;
		}
		
		private void BuildInterface ()
		{
			VBox usersPane = new VBox ();
			VBox photosPane = new VBox ();
			HBox panes = new HBox ();
			
			#region Users pane
			usersLabel = new Label ("Users");
			usersView = new TreeView ();
			usersView.SetSizeRequest (320, 240);
			
			TreeViewColumn userIdColumn = new TreeViewColumn ();
			TreeViewColumn userUidColumn = new TreeViewColumn ();
			TreeViewColumn userNameColumn = new TreeViewColumn ();
			
			userIdColumn.Title = "ID";
			userUidColumn.Title = "UID";
			userNameColumn.Title = "Name";
			
			CellRendererText idRenderer = new CellRendererText ();
			CellRendererText uidRenderer = new CellRendererText ();
			CellRendererText nameRenderer = new CellRendererText ();
			
			userIdColumn.PackStart (idRenderer, true);
			userUidColumn.PackStart (uidRenderer, true);
			userNameColumn.PackStart (nameRenderer, true);
			
			userIdColumn.SetCellDataFunc (idRenderer, new TreeCellDataFunc (RenderUserId));
			userUidColumn.SetCellDataFunc (uidRenderer, new TreeCellDataFunc (RenderUserUid));
			userNameColumn.SetCellDataFunc (nameRenderer, new TreeCellDataFunc (RenderUserName));
			
			usersView.AppendColumn (userIdColumn);
			usersView.AppendColumn (userUidColumn);
			usersView.AppendColumn (userNameColumn);
			
			usersPane.PackStart (usersLabel, false, false, 8);
			usersPane.PackStart (usersView);
			#endregion
			
			#region Photos pane
			photosLabel = new Label ("Photos");
			addPhotoButton = new Button ("Add photo");
			deletePhotoButton = new Button ("Delete photo");
			
			addPhotoButton.Sensitive = false;
			deletePhotoButton.Sensitive = false;
			
			photoManagementButtons = new HButtonBox ();
			photoManagementButtons.LayoutStyle = ButtonBoxStyle.Start;
			photoManagementButtons.PackStart (addPhotoButton);
			photoManagementButtons.PackStart (deletePhotoButton);
			
			photosView = new TreeView ();
			photosView.SetSizeRequest (320, 240);
			TreeViewColumn photoIdColumn = new TreeViewColumn ();
			photoIdColumn.Title = "ID";
			TreeViewColumn photoImageColumn = new TreeViewColumn();
			photoImageColumn.Title = "Image";
			
			CellRendererText photoIdRenderer = new CellRendererText();
			CellRendererPixbuf imageRenderer = new CellRendererPixbuf();
			
			photoIdColumn.PackStart(photoIdRenderer, true);
			photoImageColumn.PackStart(imageRenderer, true);
			
			photoIdColumn.SetCellDataFunc(photoIdRenderer, new TreeCellDataFunc(RenderPhotoId));
			photoImageColumn.SetCellDataFunc(imageRenderer, new TreeCellDataFunc(RenderPhotoImage));
			
			photosView.AppendColumn (photoIdColumn);
			photosView.AppendColumn (photoImageColumn);
			
			ListStore photosStore = new ListStore (typeof(Photo));
			photosView.Model = photosStore;
			
			photosPane.PackStart (photosLabel, false, false, 8);
			photosPane.PackStart (photosView);
			photosPane.PackStart (photoManagementButtons);
			#endregion
			
			panes.PackStart (usersPane);
			panes.PackStart (photosPane);
			
			#region Control buttons
			controlButtons = new HButtonBox ();
			saveButton = new Button ("Save");
			cancelButton = new Button ("Cancel");
			controlButtons.PackStart (saveButton, false, false, 8);
			controlButtons.PackStart (cancelButton, false, false, 8);
			controlButtons.LayoutStyle = ButtonBoxStyle.End;
			#endregion
			
			VBox rows = new VBox ();
			rows.PackStart (panes);
			rows.PackStart (controlButtons);
			this.Add (rows);
			this.ShowAll ();
			
			#region Event Handlers Setup
			this.DeleteEvent += new DeleteEventHandler (OnDelete);
			usersView.CursorChanged += new EventHandler (UsersViewCursorChanged);
			addPhotoButton.Clicked += new EventHandler (AddPhotoButtonClicked);
			#endregion
		}
		
		#endregion
		
	}
}

