using System;
using Gtk;
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
		private ButtonBox userManagementButtons;
		private Button addUserButton;
		private Button editUserButton;
		private Button deleteUserButton;
		
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
			userInfoManager = new UserInfoManager();
			if (userInfoManager.havePermissions() && userInfoManager.connect())
			{
				BuildInterface();
				
			} else
			{
				Console.WriteLine("No required permissions!");
				Application.Quit();
			}
		}
		
		private void BuildInterface ()
		{
			VBox usersPane = new VBox();
			VBox photosPane = new VBox();
			HBox panes = new HBox();
			
			// Users pane
			usersLabel = new Label("Users");
			
			addUserButton = new Button("Add user");
			editUserButton = new Button("Edit user");
			deleteUserButton = new Button("Delete user");
			
			userManagementButtons = new HButtonBox();
			userManagementButtons.LayoutStyle = ButtonBoxStyle.Start;
			userManagementButtons.PackStart(addUserButton);
			userManagementButtons.PackStart(editUserButton);
			userManagementButtons.PackStart(deleteUserButton);
			
			usersView = new TreeView();
			usersView.SetSizeRequest(320, 240);
			
			usersPane.PackStart(usersLabel, false, false, 8);
			usersPane.PackStart(usersView);
			usersPane.PackStart(userManagementButtons);
			
			// Photos pane
			photosLabel = new Label("Photos");
			addPhotoButton = new Button("Add photo");
			deletePhotoButton = new Button("Delete photo");
			
			photoManagementButtons = new HButtonBox();
			photoManagementButtons.LayoutStyle = ButtonBoxStyle.Start;
			photoManagementButtons.PackStart(addPhotoButton);
			photoManagementButtons.PackStart(deletePhotoButton);
			
			photosView = new TreeView();
			photosView.SetSizeRequest(320, 240);
			
			photosPane.PackStart(photosLabel, false, false, 8);
			photosPane.PackStart(photosView);
			photosPane.PackStart(photoManagementButtons);
			
			panes.PackStart(usersPane);
			panes.PackStart(photosPane);
			
			// Control buttons
			controlButtons = new HButtonBox();
			saveButton = new Button("Save");
			cancelButton = new Button("Cancel");
			controlButtons.PackStart(saveButton, false, false, 8);
			controlButtons.PackStart(cancelButton, false, false, 8);
			controlButtons.LayoutStyle = ButtonBoxStyle.End;
			
			VBox rows = new VBox();
			rows.PackStart(panes);
			rows.PackStart(controlButtons);
			this.Add(rows);
			this.ShowAll();
			
			// Handlers
			this.DeleteEvent += new DeleteEventHandler(OnDelete);
		}
		
		void OnDelete(object sender, DeleteEventArgs args)
		{
			Application.Quit();
		}
	}
}

