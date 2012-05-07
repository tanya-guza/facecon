using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.DirectoryServices;

using Mono.Data.Sqlite;
using Mono.Data.Sqlite.Orm;
using Mono.Data.Sqlite.Orm.ComponentModel;

using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceCon.CommandService
{
	/// <summary>
	/// Class for mapping information about user in database
	/// </summary>
	public class User
	{
		
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Name {get; set;}
		public long Uid {get; set;}
	}
	
	/// <summary>
	/// Class for mapping information about user photos in database
	/// </summary>
	public class Photo
	{
		[PrimaryKey, AutoIncrement]
		public int Id {get; set; }
		[Indexed("IX_UserId")]
		public int UserId {get; set; }
		public string ImageData {get; set;}
		
		private Image<Gray, byte> image;
		[IgnoreAttribute]
		public Image<Gray, byte> Image
		{
			get
			{
				if (image == null)
				{
					image = UserInfoManager.DeserializeImage(ImageData);

				}
				
				return image;
			}
		}
	}
	
	/// <summary>
	/// Class for managing information about users and their photos
	/// </summary>
	public class UserInfoManager
	{
		#region Private variables
		private const string CONNECTION_STRING = "URI=file:/etc/FaceManager.db,version=3";
		private const string DATABASE_LOCATION = "/etc/";
		private SqliteSession session;
		#endregion
		
		#region Utility methods
		/// <summary>
		/// Connect to database
		/// </summary>
		public bool connect()
		{	
			try
			{
				session = new SqliteSession(CONNECTION_STRING);
				setup();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return false;
			}
		}
		/// <summary>
		/// Setup database tables if they don't exist
		/// </summary>
		private void setup()
		{
			try
			{
				session.CreateTable<User>();
			}
			catch (Exception)
			{
				Console.WriteLine("Table 'users' already exists");
			}
			
			try
			{
				session.CreateTable<Photo>();
			}
			catch (Exception)
			{
				Console.WriteLine("Table 'photos' already exists");
			}
			
			List<User> users = ParseEtcPasswd();
			foreach(var user in users)
			{
				SaveUser(user);
			}
			
		}
		/// <summary>
		/// Check if user can write to selected DB location
		/// </summary>
		public bool havePermissions()
		{
			var permission = new FileIOPermission(FileIOPermissionAccess.Write,
			                                      DATABASE_LOCATION);
			var permissionSet = new PermissionSet(PermissionState.None);
			permissionSet.AddPermission(permission);
			
			return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
		}
		
		public static string SerializeImage (Image<Gray, byte> image)
		{
			var sb = new System.Text.StringBuilder ();
			(new XmlSerializer (typeof(Image<Gray, Byte>))).Serialize (new StringWriter (sb), image);
		
			return sb.ToString ();
		}
		
		public static Image<Gray, byte> DeserializeImage(string data)
		{
			XmlDocument xDoc = new XmlDocument();
			xDoc.LoadXml(data);
			Image<Gray, Byte> image = (Image<Gray, Byte>)
			(new XmlSerializer(typeof(Image<Gray, Byte>))).Deserialize(new XmlNodeReader(xDoc));
	
			return image;
		}
		
		/// <summary>
		/// Parses the /etc/passwd.
		/// </summary>
		/// <returns>
		/// The etc password.
		/// </returns>
		public static List<User> ParseEtcPasswd()
		{
			List<User> users = new List<User>();
			StreamReader reader = new StreamReader("/etc/passwd");
			
			string passwdString = null;
			while( (passwdString = reader.ReadLine()) != null)
			{
				string []substrings = passwdString.Split(':');
				
				int uid = Int32.Parse(substrings[2]);
				
				// We will look only for user accounts ( UID >= 1000)
				if (uid >= 1000)
				{
					User user = new User();
					user.Name = substrings[0];
					user.Uid  = uid;
					users.Add(user);
				}
			}
			return users;
		}
		
		#endregion
		
		#region User methods
		/// <summary>
		/// Creates new record in users table or updates existing one
		/// </summary>
		/// <param name='user'>
		/// User data
		/// </param>
		public void SaveUser(User user)
		{
			if (user.Id == 0 && (session.ExecuteScalar<Int32>(
				"SELECT COUNT(*) FROM User WHERE Uid = ?", user.Uid)) == 0)
			{
				session.Insert<User>(user);
			} else
			{
				session.Update<User>(user);
			}
		}
		
		/// <summary>
		/// Finds the user by name
		/// </summary>
		/// <param name='Name'>
		/// Name
		/// </param>
		public User FindUser(string Name)
		{
			return session.ExecuteScalar<User>(
				"SELECT FROM User WHERE Name == ?", Name);
		}
		
		/// <summary>
		/// Deletes the user
		/// </summary>
		/// <param name='user'>
		/// User
		/// </param>
		public void DeleteUser(User user)
		{
			session.Delete<User>(user);
		}
		
		public List<User> ListUsers()
		{
			return new List<User>(session.Query<User>("SELECT * FROM User"));
		}
		/// <summary>
		/// Lists the user photo records.
		/// </summary>
		/// <returns>
		/// The user photos.
		/// </returns>
		/// <param name='user'>
		/// User.
		/// </param>
		public List<Photo> ListUserPhotoData(User user)
		{
			
			IEnumerable<Photo> photoData = session.Query<Photo>(
				"SELECT * FROM Photo WHERE UserId = ?", user.Id);
	
			return new List<Photo>(photoData);
		}
		#endregion
		
		#region Photo methods
		/// <summary>
		/// Creates new record in photo table or updates existing one
		/// </summary>
		/// <param name='photo'>
		/// User data
		/// </param>
		public void SavePhoto(Photo photo)
		{
			if (photo.Id == 0)
			{
				session.Insert<Photo>(photo);
			} else
			{
				session.Update<Photo>(photo);
			}
		}
		
		/// <summary>
		/// Finds the user by name
		/// </summary>
		/// <param name='Name'>
		/// Name
		/// </param>
		public Photo FindPhoto(int Id)
		{
			return session.ExecuteScalar<Photo>(
				"SELECT FROM User WHERE Id == ?", Id);
		}
		
		/// <summary>
		/// Deletes the photo
		/// </summary>
		/// <param name='photo'>
		/// Photo
		/// </param>
		public void DeleteUser(Photo photo)
		{
			session.Delete<Photo>(photo);
		}
		#endregion
		
		
	}
}

