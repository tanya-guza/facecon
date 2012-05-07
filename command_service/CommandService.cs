using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using System.Xml.Serialization;
using System.Text;

using System.Collections.Generic;


using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceCon.CommandService
{
	public delegate void AuthenticationHandler(string imageData);
	
	[ServiceContract]
	public interface ICommandService
	{
		[OperationContract]
		string authenticate(string imageData);
		
		[OperationContract]
		string executeCommand(string sessionId, string command,
		                      string arguments);
	}
	
	public class CommandService: ICommandService
	{
		private EigenObjectRecognizer recognizer;
		private UserInfoManager userInfoManager;
		private Dictionary<string, User> sessions = new Dictionary<string, User>();
					
		List<Image<Gray, byte>> images  = new List<Image<Gray, byte>>();
		List<string> labels = new List<string>();
		
		public CommandService()
		{
			userInfoManager = new UserInfoManager();
			userInfoManager.connect();
			

			
			foreach(var user in userInfoManager.ListUsers())
			{
				foreach (var photo in userInfoManager.ListUserPhotoData(user))
				{
					labels.Add(user.Name);
					images.Add(photo.Image);
					
					Console.WriteLine("Added image");
				}
			}
			
			MCvTermCriteria crit = new MCvTermCriteria(1.0); 
			
			this.recognizer = new EigenObjectRecognizer(images.ToArray(),
			                                            labels.ToArray(),
			                                            2000,
			                                            ref crit);
		}
		
		public AuthenticationHandler Authenticated
		{
			get;set;
		}
		
		public string authenticate(string imageData)
		{
				
			var image = UserInfoManager.DeserializeImage(imageData).Clone();
			Console.WriteLine("Auth called");
			//var result = recognizer.Recognize(image); 
			var result = recognizer.Recognize(image);
			
			if (result != null)
			{
				string s = DateTime.Now.ToFileTime().ToString();
				sessions[s] = this.userInfoManager.FindUser(result.Label);
				return s;
			} else
			{
				return "INTRUDER";
			}
		}
		
		public string executeCommand(string sessionId, string command,
		                      string arguments)
		{
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.EnableRaisingEvents=false; 
			proc.StartInfo.FileName = command;
			proc.StartInfo.Arguments = arguments;
			proc.Start();
			proc.WaitForExit();
			
			return proc.StandardOutput.ReadToEnd();
		}
		
	}
	
}

