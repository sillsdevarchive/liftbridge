using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using SIL.LiftBridge.Properties;

namespace LiftBridge
{
	/// <summary>
	/// This class encapsulates the code related to service communication with FLEx.
	/// </summary>
	internal class LiftBridgeConnectionHelper : IDisposable
	{
		private readonly ServiceHost _host;
		private readonly ILiftBridgeService _pipe;
		private readonly bool _hostOpened;

		/// <summary>
		/// Constructs the helper setting up the local service endpoint and opening
		/// </summary>
		/// <param name="fwProjectPath">The entire FieldWorks project folder path.
		/// Must include the project folder and project name with "fwdata" extension.
		/// Empty is OK if not send_receive command.</param>
		internal LiftBridgeConnectionHelper(string fwProjectPath)
		{
			_hostOpened = true;
			string fwProjectName = ""; // will only be able to to S/R one project at a time
			if (!String.IsNullOrEmpty(fwProjectPath)) // can S/R multiple projects simultaneously
				fwProjectName = Path.GetFileNameWithoutExtension(fwProjectPath);

			try
			{
				_host = new ServiceHost(typeof(FLExService),
									   new[] { new Uri("net.pipe://localhost/FLExEndpoint" + fwProjectName) });
				//open host ready for business
				_host.AddServiceEndpoint(typeof(IFLExService), new NetNamedPipeBinding(), "FLExPipe");
				_host.Open();
			}
			catch (AddressAlreadyInUseException)
			{
				//There may be another copy of FLExBridge running, but we need to try and wakeup FLEx before we quit.
				_hostOpened = false;
			}
			var pipeFactory = new ChannelFactory<ILiftBridgeService>(new NetNamedPipeBinding(),
													   new EndpointAddress("net.pipe://localhost/FLExBridgeEndpoint" + fwProjectName + "/FLExPipe"));
			_pipe = pipeFactory.CreateChannel();
			((IContextChannel)_pipe).OperationTimeout = TimeSpan.MaxValue;
			try
			{
				//Notify FLEx that we are ready to receive requests.
				//(if we failed to create the host we still want to do this so FLEx can wake up)
				_pipe.BridgeReady();
			}
			catch(Exception)
			{
				Console.WriteLine(Resources.kFlexNotListening);
				_pipe = null; //FLEx isn't listening.
			}
		}

		public bool HostOpened { get { return _hostOpened; } }

		/// <summary>
		/// Sends the entire FieldWorks project folder path (must include the
		/// project folder and project name with "fwdata" extension) across the pipe
		/// to FieldWorks.
		/// </summary>
		/// <param name="fwProjectName">The whole fw project path</param>
		public void SendFwProjectName(string fwProjectName)
		{
			try
			{
				if (_pipe != null)
					_pipe.InformFwProjectName(fwProjectName);
			}
			catch (Exception)
			{
				Console.WriteLine(Resources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
			}
		}

		/// <summary>
		/// Signals FLEx through 2 means that the bridge work has been completed.
		/// A direct message to FLEx if it is listening, and by allowing the BridgeWorkOngoing method to complete
		/// </summary>
		internal void SignalBridgeWorkComplete(bool changesReceived)
		{
			//open a channel to flex and send the message.
			try
			{
				if(_pipe != null)
					_pipe.BridgeWorkComplete(changesReceived);
			}
			catch (Exception)
			{
				Console.WriteLine(Resources.kFlexNotListening);//It isn't fatal if FLEx isn't listening to us.
			}
			// Allow the _host to get the WaitObject, which will result in the WorkDoneCallback
			// method being called in FLEx:
			Monitor.Enter(FLExService.WaitObject);
			Monitor.PulseAll(FLExService.WaitObject);
			Monitor.Exit(FLExService.WaitObject);
		}

		/// <summary>
		/// Signals FLEx that the bridge sent a jump URL to process.
		/// </summary>
		internal void SendJumpUrlToFlex(object sender, JumpEventArgs e)
		{
			try
			{
				if (_pipe != null)
					_pipe.BridgeSentJumpUrl(e.JumpUrl);
			}
			catch(Exception)
			{
				Console.WriteLine(Resources.kFlexNotListening);//It isn't fatal if FLEx isn't listening to us.
			}
		}

		/// <summary>
		/// Interface for the service which FLEx implements
		/// </summary>
		[ServiceContract]
		private interface ILiftBridgeService
		{
			[OperationContract]
			void BridgeWorkComplete(bool changesReceived);

			[OperationContract]
			void BridgeReady();

			[OperationContract]
			void InformFwProjectName(string fwProjectName);

			[OperationContract]
			void BridgeSentJumpUrl(string jumpUrl);
		}

		/// <summary>
		/// Our service with methods that FLEx can call.
		/// </summary>
		[ServiceBehavior(UseSynchronizationContext = false)] //Create new threads for the services, don't tie them into the main UI thread.
		private class FLExService : IFLExService
		{
			internal static readonly object WaitObject = new object();
			private static bool _workComplete;
			public void BridgeWorkOngoing()
			{
				Monitor.Enter(WaitObject);
				while(!_workComplete)
				{
					try
					{
						Monitor.Wait(WaitObject, -1);
						_workComplete = true;
					}
					catch (ThreadInterruptedException)
					{
						//this exception is known as a spurious interrupt, very rare, usually comes from bad hardware
						//doesn't mean we were done, so try and wait again
					}
					catch(Exception)
					{
						//all other exceptions we are considering an end of normal operation
						_workComplete = true;
					}
				}
				Monitor.Exit(WaitObject);
			}

		}

		[ServiceContract]
		private interface IFLExService
		{
			[OperationContract]
			void BridgeWorkOngoing();
		}

		public void Dispose()
		{
			if (_hostOpened)
				_host.Close();
		}
	}

	internal class JumpEventArgs : EventArgs
	{
		private readonly string _jumpUrl;

		internal JumpEventArgs(string jumpUrl)
		{
			_jumpUrl = jumpUrl;
		}

		internal string JumpUrl
		{
			get { return _jumpUrl; }
		}
	}
}
