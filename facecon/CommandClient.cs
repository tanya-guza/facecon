using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using FaceCon.CommandService;


namespace FaceCon.FaceCon
{
	/// <summary>
	/// Класс, предназначенный для аутентификации и выполнения
	/// команд на сервере
	/// </summary>	
	public class CommandClient : System.ServiceModel.ClientBase<ICommandService>,
		ICommandService
	{
		private string sessionId;
		
		public string SessionId
		{
			get
			{
				return sessionId;
			}
		}
		
		public CommandClient (Binding binding, EndpointAddress address)
	    	: base (binding, address)
	  	{
	  	}	
		
		public string authenticate(string imageData)
		{
			sessionId = Channel.authenticate(imageData);
			return sessionId;
		}
		
		public string executeCommand(string command, string arguments)
		{
			return executeCommand(sessionId, command,arguments);
		}
		
		public string executeCommand(string sessionId, string command,
		                             string arguments)
		{
			return Channel.executeCommand(sessionId, command, arguments);
		}
	}
}

