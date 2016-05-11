﻿using mRemoteNG.App;
using mRemoteNG.App.Info;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.Window;
using System;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;

namespace mRemoteNG.Config.Settings
{
    public class LayoutSettingsLoader
    {
        private frmMain _MainForm;

        public LayoutSettingsLoader(frmMain MainForm)
        {
            _MainForm = MainForm;
        }

        public void LoadPanelsFromXML()
        {
            try
            {
                Windows.treePanel = null;
                Windows.configPanel = null;
                Windows.errorsPanel = null;

                while (_MainForm.pnlDock.Contents.Count > 0)
                {
                    DockContent dc = (DockContent)_MainForm.pnlDock.Contents[0];
                    dc.Close();
                }

                Startup.CreatePanels();

                string oldPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + (new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.ProductName + "\\" + SettingsFileInfo.LayoutFileName;
                string newPath = SettingsFileInfo.SettingsPath + "\\" + SettingsFileInfo.LayoutFileName;
                if (File.Exists(newPath))
                {
                    _MainForm.pnlDock.LoadFromXml(newPath, GetContentFromPersistString);
#if !PORTABLE
				}
				else if (File.Exists(oldPath))
				{
					_MainForm.pnlDock.LoadFromXml(oldPath, GetContentFromPersistString);
#endif
                }
                else
                {
                    Startup.SetDefaultLayout();
                }
            }
            catch (Exception ex)
            {
                Runtime.Log.Error("LoadPanelsFromXML failed" + Environment.NewLine + ex.Message);
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            // pnlLayout.xml persistence XML fix for refactoring to mRemoteNG
            if (persistString.StartsWith("mRemote."))
                persistString = persistString.Replace("mRemote.", "mRemoteNG.");

            try
            {
                if (persistString == typeof(ConfigWindow).ToString())
                    return Windows.configPanel;

                if (persistString == typeof(ConnectionTreeWindow).ToString())
                    return Windows.treePanel;

                if (persistString == typeof(ErrorAndInfoWindow).ToString())
                    return Windows.errorsPanel;

                if (persistString == typeof(ScreenshotManagerWindow).ToString())
                    return Windows.screenshotPanel;
            }
            catch (Exception ex)
            {
                Runtime.Log.Error("GetContentFromPersistString failed" + Environment.NewLine + ex.Message);
            }

            return null;
        }
    }
}