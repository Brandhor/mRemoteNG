using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using mRemoteNG.App;
using System.IO;
using System.ComponentModel;
using mRemoteNG.Connection;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.My;


namespace mRemoteNG.Tools
{
	public class ExternalTool
	{
        #region Public Properties
		public string DisplayName { get; set; }
		public string FileName { get; set; }
		public bool WaitForExit { get; set; }
		public string Arguments { get; set; }
		public bool TryIntegrate { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
		
        public Icon Icon
		{
			get
			{
				if (File.Exists(FileName))
					return MiscTools.GetIconFromFile(FileName);
				else
					return null;
			}
		}
		
        public Image Image
		{
			get
			{
				if (Icon != null)
					return Icon.ToBitmap();
				else
					return Resources.mRemote_Icon.ToBitmap();
			}
		}
        #endregion
		
		public ExternalTool(string displayName = "", string fileName = "", string arguments = "")
		{
			this.DisplayName = displayName;
			this.FileName = fileName;
			this.Arguments = arguments;
		}

        public void Start(ConnectionInfo startConnectionInfo = null)
		{
			try
			{
			    if (string.IsNullOrEmpty(FileName))
			    {
			        Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "ExternalApp.Start() failed: FileName cannot be blank.", false);
			        return;
			    }
				
				ConnectionInfo = startConnectionInfo;
				
				if (TryIntegrate)
					StartIntegrated();
                else
                    StartExternalProcess();
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddExceptionMessage("ExternalApp.Start() failed.", ex);
			}
		}

        private void StartExternalProcess()
        {
            Process process = new Process();
            SetProcessProperties(process, ConnectionInfo);
            process.Start();

            if (WaitForExit)
            {
                process.WaitForExit();
            }
        }

        private void SetProcessProperties(Process process, ConnectionInfo startConnectionInfo)
        {
            ArgumentParser argParser = new ArgumentParser(startConnectionInfo);
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = argParser.ParseArguments(FileName);
            process.StartInfo.Arguments = argParser.ParseArguments(Arguments);
        }
		
		public void StartIntegrated()
		{
			try
			{
                ConnectionInfo newConnectionInfo = BuildConnectionInfoForIntegratedApp();
                ConnectionInitiator connectionInitiator = new ConnectionInitiator();
                connectionInitiator.InitiateConnection(newConnectionInfo);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddExceptionMessage(message: "ExternalApp.StartIntegrated() failed.", ex: ex, logOnly: true);
			}
		}

        private ConnectionInfo BuildConnectionInfoForIntegratedApp()
        {
            ConnectionInfo newConnectionInfo = GetAppropriateInstanceOfConnectionInfo();
            SetConnectionInfoFields(newConnectionInfo);
            return newConnectionInfo;
        }

        private ConnectionInfo GetAppropriateInstanceOfConnectionInfo()
        {
            ConnectionInfo newConnectionInfo = default(ConnectionInfo);
            if (this.ConnectionInfo == null)
                newConnectionInfo = new ConnectionInfo();
            else
                newConnectionInfo = this.ConnectionInfo.Copy();
            return newConnectionInfo;
        }

        private void SetConnectionInfoFields(ConnectionInfo newConnectionInfo)
        {
            newConnectionInfo.Protocol = ProtocolType.IntApp;
            newConnectionInfo.ExtApp = DisplayName;
            newConnectionInfo.Name = DisplayName;
            newConnectionInfo.Panel = Language.strMenuExternalTools;
        }
	}
}