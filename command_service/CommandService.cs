using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using System.Xml.Serialization;
using System.Text;

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
		public AuthenticationHandler Authenticated
		{
			get;set;
		}
		
		public string authenticate(string imageData)
		{
			if (Authenticated != null)
			{
				Authenticated(imageData);
			}
			
//			var image = deserializeImage(imageData);
//			CvInvoke.cvNamedWindow ("w1");
//			CvInvoke.cvShowImage ("w1", image.Ptr);
//			CvInvoke.cvWaitKey (0);
//			CvInvoke.cvDestroyWindow ("w1");
			return "qwerty";
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

