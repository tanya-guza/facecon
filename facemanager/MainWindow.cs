using System;
using Gtk;
using Gdk;
using FaceCon.CommandService;

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
		private void AddPhotoButtonClicked(object sender,
		                             EventArgs args)
		{
			PhotoEditorWindow photoEditor = new PhotoEditorWindow();
			photoEditor.Show();
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
		
		#endregion
		
		#region Utils
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
			TreeViewColumn photoImageColumn = 
				new TreeViewColumn ("Image", new CellRendererPixbuf (), 0);
			photoIdColumn.Title = "ID";
			photosView.AppendColumn (photoIdColumn);
			photosView.AppendColumn (photoImageColumn);
			ListStore photosStore = new ListStore (typeof(int), typeof(Pixbuf));
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
			addPhotoButton.Clicked += new EventHandler(AddPhotoButtonClicked);
			#endregion
		}
		
		#endregion
		
	}
}

