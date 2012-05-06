using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Mono.Data.Sqlite;

using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceCon.CommandService
{
	public struct UserInfo
	{
		public string Name;
		public long uid;
		public List<Image<Gray, byte>> photos; 
	}
	
	public class UserInfoManager
	{
		private const string CONNECTION_STRING = "URI=file:/etc/FaceManager.db,version=3";
		private const string DATABASE_LOCATION = "/etc/";
		
		private IDbConnection connection;
		public bool connect()
		{
			connection = (IDbConnection) new SqliteConnection(CONNECTION_STRING);
			
			try
			{
				connection.Open();
				setup();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return false;
			}
		}
		
		public List<UserInfo> load()
		{
			return null;
		}
		
		public void save(List<UserInfo> users)
		{
		}
		
		private void setup()
		{
			try
			{
				IDbCommand setupCommand = connection.CreateCommand();
				setupCommand.CommandText = "CREATE TABLE users(id INTEGER " +
					"PRIMARY KEY, login VARCHAR(32), uid INTEGER);";
				setupCommand.ExecuteNonQuery();	
			}
			catch (Exception)
			{
				Console.WriteLine("Table 'users' already exists");
			}
			
			try
			{
				IDbCommand setupCommand = connection.CreateCommand();
				setupCommand.CommandText = "CREATE TABLE photos(id INTEGER " +
					"PRIMARY KEY, user_id INTEGER, photo BLOB);";
				setupCommand.ExecuteNonQuery();	
			}
			catch (Exception)
			{
				Console.WriteLine("Table 'photos' already exists");
			}
		}
		
		public bool havePermissions()
		{
			var permission = new FileIOPermission(FileIOPermissionAccess.Write,
			                                      DATABASE_LOCATION);
			var permissionSet = new PermissionSet(PermissionState.None);
			permissionSet.AddPermission(permission);
			
			return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
		}
	}
}

