using System;
using System.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;
using mRemoteNG.App;
using mRemoteNG.App.Update;


namespace mRemoteNG.UI.Window
{
	public partial class AnnouncementWindow : BaseWindow
	{
        #region Public Methods
		public AnnouncementWindow(DockContent panel)
		{
			WindowType = WindowType.Announcement;
			DockPnl = panel;
			InitializeComponent();
		}
        #endregion
				
        #region Private Fields
		private AppUpdater _appUpdate;
        #endregion
				
        #region Private Methods
		public void Announcement_Load(object sender, EventArgs e)
		{
			webBrowser.Navigated += webBrowser_Navigated;
					
			ApplyLanguage();
			CheckForAnnouncement();
		}
				
		private void ApplyLanguage()
		{
					
		}
				
		private void webBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
		{
			// This can only be set once the WebBrowser control is shown, it will throw a COM exception otherwise.
			webBrowser.AllowWebBrowserDrop = false;
					
			webBrowser.Navigated -= webBrowser_Navigated;
		}
				
		private void CheckForAnnouncement()
		{
			if (_appUpdate == null)
			{
				_appUpdate = new AppUpdater();
				//_appUpdate.Load += _appUpdate.Update_Load;
			}
			else if (_appUpdate.IsGetAnnouncementInfoRunning)
			{
				return ;
			}
					
			_appUpdate.GetAnnouncementInfoCompletedEvent += GetAnnouncementInfoCompleted;
					
			_appUpdate.GetAnnouncementInfoAsync();
		}
				
		private void GetAnnouncementInfoCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (InvokeRequired)
			{
				AsyncCompletedEventHandler myDelegate = new AsyncCompletedEventHandler(GetAnnouncementInfoCompleted);
				Invoke(myDelegate, new object[] {sender, e});
				return ;
			}
					
			try
			{
				_appUpdate.GetAnnouncementInfoCompletedEvent -= GetAnnouncementInfoCompleted;
						
				if (e.Cancelled)
				{
					return ;
				}
				if (e.Error != null)
				{
					throw (e.Error);
				}
						
				webBrowser.Navigate(_appUpdate.CurrentAnnouncementInfo.Address);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddExceptionMessage(Language.strUpdateGetAnnouncementInfoFailed, ex);
			}
		}
        #endregion
	}
}
