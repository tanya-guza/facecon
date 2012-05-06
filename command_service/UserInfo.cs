using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

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
	}
	
	public class UserInfoManager
	{
		#region Private variables
		private const string CONNECTION_STRING = "URI=file:FaceManager.db,version=3";
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
		
		public static Image<Gray, byte> DeserializeImage(string data)
		{
			XmlDocument xDoc = new XmlDocument();
			xDoc.LoadXml(data);
			Image<Gray, Byte> image = (Image<Gray, Byte>)
			(new XmlSerializer(typeof(Image<Gray, Byte>))).Deserialize(new XmlNodeReader(xDoc));
	
			return image;
		}
		
		#endregion
		
		#region User methods
		/// <summary>
		/// Creates new record in users table or updates existing one
		/// </summary>
		/// <param name='user'>
		/// User data
		/// </param>
		void SaveUser(User user)
		{
			if (user.Id == 0)
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
		User FindUser(string Name)
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
		void DeleteUser(User user)
		{
			session.Delete<User>(user);
		}
		
		/// <summary>
		/// Lists the user photo images.
		/// </summary>
		/// <returns>
		/// The user photos.
		/// </returns>
		/// <param name='user'>
		/// User.
		/// </param>
		List<Image<Gray, byte>> ListUserPhotos(User user)
		{
			List<Image<Gray, byte>> photos = new List<Image<Gray, byte>>();
			IEnumerable<Photo> photoData = session.Query<Photo>(
				"SELECT * FROM Photo WHERE UserId = ?", user.Id);
			
			foreach(var photo in photoData)
			{
				photos.Add(DeserializeImage(photo.ImageData));
			}
			
			return photos;
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
		List<Photo> ListUserPhotoData(User user)
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
		void SavePhoto(Photo photo)
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
		Photo FindPhoto(int Id)
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
		void DeleteUser(Photo photo)
		{
			session.Delete<Photo>(photo);
		}
		#endregion
		
		
	}
}

