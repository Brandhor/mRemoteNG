using System.Collections.Generic;
using System;
using AxWFICALib;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using AxMSTSCLib;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using mRemoteNG.App;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Images;
using mRemoteNG.Connection;
using mRemoteNG.Tree;
using mRemoteNG.Container;

namespace mRemoteNG.Config.Import
{
	public class PuttyConnectionManager
	{
		public static void Import(string fileName, ConnectionTreeNode parentTreeNode)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);
				
			XmlNode configurationNode = xmlDocument.SelectSingleNode("/configuration");
			//Dim version As New Version(configurationNode.Attributes("version").Value)
			//If Not version = New Version(0, 7, 1, 136) Then
			//    Throw New FileFormatException(String.Format("Unsupported file version ({0}).", version))
			//End If
				
			foreach (XmlNode rootNode in configurationNode.SelectNodes("./root"))
			{
				ImportRootOrContainer(rootNode, parentTreeNode);
			}
		}
			
		private static void ImportRootOrContainer(XmlNode xmlNode, ConnectionTreeNode parentTreeNode)
		{
			string xmlNodeType = xmlNode.Attributes["type"].Value;
			switch (xmlNode.Name)
			{
				case "root":
					if (!(string.Compare(xmlNodeType, "database", ignoreCase: true) == 0))
					{
						throw (new FileFormatException(string.Format("Unrecognized root node type ({0}).", xmlNodeType)));
					}
					break;
				case "container":
					if (!(string.Compare(xmlNodeType, "folder", ignoreCase: true) == 0))
					{
						throw (new FileFormatException(string.Format("Unrecognized root node type ({0}).", xmlNodeType)));
					}
					break;
				default:
					// ReSharper disable once LocalizableElement
					throw (new ArgumentException("Argument must be either a root or a container node.", "xmlNode"));
			}
				
			if (parentTreeNode == null)
			{
				throw (new InvalidOperationException("parentInfo.TreeNode must not be null."));
			}
				
			string name = xmlNode.Attributes["name"].Value;
				
			var treeNode = new ConnectionTreeNode(name);
			parentTreeNode.Nodes.Add(treeNode);
				
			ContainerInfo containerInfo = new ContainerInfo();
			containerInfo.TreeNode = treeNode;
			containerInfo.Name = name;

            ConnectionInfo connectionInfo = CreateConnectionInfo(name);
			connectionInfo.Parent = containerInfo;
			connectionInfo.IsContainer = true;
			containerInfo.ConnectionInfo = connectionInfo;
				
			// We can only inherit from a container node, not the root node or connection nodes
			if (ConnectionTreeNode.GetNodeType(parentTreeNode) == TreeNodeType.Container)
			{
				containerInfo.Parent = (ContainerInfo)parentTreeNode.Tag;
			}
			else
			{
				connectionInfo.Inheritance.DisableInheritance();
			}
				
			treeNode.Name = name;
			treeNode.Tag = containerInfo;
			treeNode.ImageIndex = (int)TreeImageType.Container;
			treeNode.SelectedImageIndex = (int)TreeImageType.Container;
				
			foreach (XmlNode childNode in xmlNode.SelectNodes("./*"))
			{
				switch (childNode.Name)
				{
					case "container":
						ImportRootOrContainer(childNode, treeNode);
						break;
					case "connection":
						ImportConnection(childNode, treeNode);
						break;
					default:
						throw (new FileFormatException(string.Format("Unrecognized child node ({0}).", childNode.Name)));
				}
			}
				
			containerInfo.IsExpanded = bool.Parse(xmlNode.Attributes["expanded"].InnerText);
			if (containerInfo.IsExpanded)
			{
				treeNode.Expand();
			}
				
			Runtime.ContainerList.Add(containerInfo);
		}
			
		private static void ImportConnection(XmlNode connectionNode, ConnectionTreeNode parentTreeNode)
		{
			string connectionNodeType = connectionNode.Attributes["type"].Value;
			if (!(string.Compare(connectionNodeType, "PuTTY", ignoreCase: true) == 0))
			{
				throw (new FileFormatException(string.Format("Unrecognized connection node type ({0}).", connectionNodeType)));
			}
				
			string name = connectionNode.Attributes["name"].Value;
			var treeNode = new ConnectionTreeNode(name);
			parentTreeNode.Nodes.Add(treeNode);

            ConnectionInfo connectionInfo = ConnectionInfoFromXml(connectionNode);
			connectionInfo.TreeNode = treeNode;
			connectionInfo.Parent = (ContainerInfo)parentTreeNode.Tag;
				
			treeNode.Name = name;
			treeNode.Tag = connectionInfo;
			treeNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
            treeNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;
				
			Runtime.ConnectionList.Add(connectionInfo);
		}
			
		private static ConnectionInfo CreateConnectionInfo(string name)
		{
			ConnectionInfo connectionInfo = new ConnectionInfo();
			connectionInfo.Inheritance = new ConnectionInfoInheritance(connectionInfo);
			connectionInfo.Name = name;
			return connectionInfo;
		}
			
		private static ConnectionInfo ConnectionInfoFromXml(XmlNode xmlNode)
		{
			XmlNode connectionInfoNode = xmlNode.SelectSingleNode("./connection_info");
				
			string name = connectionInfoNode.SelectSingleNode("./name").InnerText;
			ConnectionInfo connectionInfo = CreateConnectionInfo(name);
				
			string protocol = connectionInfoNode.SelectSingleNode("./protocol").InnerText;
			switch (protocol.ToLowerInvariant())
			{
				case "telnet":
					connectionInfo.Protocol = ProtocolType.Telnet;
					break;
				case "ssh":
					connectionInfo.Protocol = ProtocolType.SSH2;
					break;
				default:
					throw (new FileFormatException(string.Format("Unrecognized protocol ({0}).", protocol)));
			}
				
			connectionInfo.Hostname = connectionInfoNode.SelectSingleNode("./host").InnerText;
			connectionInfo.Port = Convert.ToInt32(connectionInfoNode.SelectSingleNode("./port").InnerText);
			connectionInfo.PuttySession = connectionInfoNode.SelectSingleNode("./session").InnerText;
			// ./commandline
			connectionInfo.Description = connectionInfoNode.SelectSingleNode("./description").InnerText;
				
			XmlNode loginNode = xmlNode.SelectSingleNode("./login");
			connectionInfo.Username = loginNode.SelectSingleNode("login").InnerText;
			connectionInfo.Password = loginNode.SelectSingleNode("password").InnerText;
			// ./prompt
				
			// ./timeout/connectiontimeout
			// ./timeout/logintimeout
			// ./timeout/passwordtimeout
			// ./timeout/commandtimeout
				
			// ./command/command1
			// ./command/command2
			// ./command/command3
			// ./command/command4
			// ./command/command5
				
			// ./options/loginmacro
			// ./options/postcommands
			// ./options/endlinechar
				
			return connectionInfo;
		}
	}
}