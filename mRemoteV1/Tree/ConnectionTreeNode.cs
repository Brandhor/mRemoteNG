using mRemoteNG.App;
using mRemoteNG.Connection;
using mRemoteNG.Container;
using mRemoteNG.Images;
using System;
using System.Windows.Forms;
using mRemoteNG.Messages;
using mRemoteNG.Root.PuttySessions;
using mRemoteNG.Tree.Root;

namespace mRemoteNG.Tree
{
	public class ConnectionTreeNode : TreeNode
    {
        public new ConnectionTreeNode Parent
        {
            get { return (ConnectionTreeNode)base.Parent; }
        }

        public ConnectionTreeNode()
        {
            Name = Language.strNewConnection;
            Text = Name;
        }

        public ConnectionTreeNode(string ConnectionName = "")
        {
            Name = ConnectionName;
            Text = Name;
        }

        #region Public Methods
		public string GetConstantID()
		{
			if (GetNodeType(this) == TreeNodeType.Connection)
				return (Tag as ConnectionInfo).ConstantID;
			else if (GetNodeType(this) == TreeNodeType.Container)
				return (Tag as ContainerInfo).ConnectionInfo.ConstantID;
				
			return null;
		}
		
		public static ConnectionTreeNode GetNodeFromPositionID(int id)
		{
			foreach (ConnectionInfo connection in Runtime.ConnectionList)
			{
				if (connection.PositionID == id)
				{
					if (connection.IsContainer)
						return (connection.Parent as ContainerInfo).TreeNode;
					else
						return connection.TreeNode;
				}
			}
				
			return null;
		}
		
		public static ConnectionTreeNode GetNodeFromConstantID(string id)
		{
            foreach (ConnectionInfo connectionInfo in Runtime.ConnectionList)
			{
				if (connectionInfo.ConstantID == id)
				{
					if (connectionInfo.IsContainer)
						return (connectionInfo.Parent as ContainerInfo).TreeNode;
					else
						return connectionInfo.TreeNode;
				}
			}
				
			return null;
		}
		
		public static TreeNodeType GetNodeType(ConnectionTreeNode treeNode)
		{
			try
			{
                if (treeNode == null || treeNode.Tag == null)
					return TreeNodeType.None;
					
				if (treeNode.Tag is PuttySessionsNodeInfo)
					return TreeNodeType.PuttyRoot;
				else if (treeNode.Tag is RootNodeInfo)
					return TreeNodeType.Root;
				else if (treeNode.Tag is ContainerInfo)
					return TreeNodeType.Container;
				else if (treeNode.Tag is PuttySessionInfo)
					return TreeNodeType.PuttySession;
				else if (treeNode.Tag is ConnectionInfo)
					return TreeNodeType.Connection;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Couldn\'t get node type" + Environment.NewLine + ex.Message, true);
			}
				
			return TreeNodeType.None;
		}
		
		public static TreeNodeType GetNodeTypeFromString(string str)
		{
			try
			{
				switch (str.ToLower())
				{
					case "root":
						return TreeNodeType.Root;
					case "container":
						return TreeNodeType.Container;
					case "connection":
						return TreeNodeType.Connection;
				}
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Couldn\'t get node type from string" + Environment.NewLine + ex.Message, true);
			}
				
			return TreeNodeType.None;
		}
		
		public bool IsEmpty()
		{
			try
			{
				if (Nodes.Count > 0)
					return false;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "IsEmpty (Tree.Node) failed" + Environment.NewLine + ex.Message, true);
			}
				
			return true;
		}
		
		public static ConnectionTreeNode AddNode(TreeNodeType nodeType, string name = null)
		{
			try
			{
				var treeNode = new ConnectionTreeNode();
				string defaultName = "";
					
				switch (nodeType)
				{
					case TreeNodeType.Connection:
					case TreeNodeType.PuttySession:
						defaultName = Language.strNewConnection;
                        treeNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
                        treeNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;
						break;
					case TreeNodeType.Container:
						defaultName = Language.strNewFolder;
                        treeNode.ImageIndex = (int)TreeImageType.Container;
                        treeNode.SelectedImageIndex = (int)TreeImageType.Container;
						break;
					case TreeNodeType.Root:
						defaultName = Language.strNewRoot;
                        treeNode.ImageIndex = (int)TreeImageType.Root;
                        treeNode.SelectedImageIndex = (int)TreeImageType.Root;
						break;
				}
					
				if (!string.IsNullOrEmpty(name))
					treeNode.Name = name;
				else
					treeNode.Name = defaultName;
				treeNode.Text = treeNode.Name;
					
				return treeNode;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "AddNode failed" + Environment.NewLine + ex.Message, true);
			}
				
			return null;
		}
		
		public static void CloneNode(ConnectionTreeNode oldTreeNode, ConnectionTreeNode parentNode = null)
		{
			try
			{
				if (GetNodeType(oldTreeNode) == TreeNodeType.Connection)
                    CloneConnectionNode(oldTreeNode, parentNode);
				else if (GetNodeType(oldTreeNode) == TreeNodeType.Container)
                    CloneContainerNode(oldTreeNode, parentNode);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg, string.Format(Language.strErrorCloneNodeFailed, ex.Message));
			}
		}

        private static void CloneContainerNode(ConnectionTreeNode oldTreeNode, ConnectionTreeNode parentNode)
        {
            ContainerInfo oldContainerInfo = (ContainerInfo) oldTreeNode.Tag;

            ContainerInfo newContainerInfo = oldContainerInfo.Copy();
            ConnectionInfo newConnectionInfo = oldContainerInfo.ConnectionInfo.Copy();
            newContainerInfo.ConnectionInfo = newConnectionInfo;

            var newTreeNode = new ConnectionTreeNode(newContainerInfo.Name);
            newTreeNode.Tag = newContainerInfo;
            newTreeNode.ImageIndex = (int)TreeImageType.Container;
            newTreeNode.SelectedImageIndex = (int)TreeImageType.Container;
            newContainerInfo.ConnectionInfo.Parent = newContainerInfo;

            Runtime.ContainerList.Add(newContainerInfo);

            if (parentNode == null)
            {
                oldTreeNode.Parent.Nodes.Insert(oldTreeNode.Index + 1, newTreeNode);
                ConnectionTree.Instance.SelectedNode = newTreeNode;
            }
            else
            {
                parentNode.Nodes.Add(newTreeNode);
            }

            foreach (ConnectionTreeNode childTreeNode in oldTreeNode.Nodes)
            {
                CloneNode(childTreeNode, newTreeNode);
            }

            newTreeNode.Expand();
        }

        private static void CloneConnectionNode(ConnectionTreeNode oldTreeNode, ConnectionTreeNode parentNode)
        {
            ConnectionInfo oldConnectionInfo = (ConnectionInfo)oldTreeNode.Tag;

            ConnectionInfo newConnectionInfo = oldConnectionInfo.Copy();
            ConnectionInfoInheritance newInheritance = oldConnectionInfo.Inheritance.Copy();
            newInheritance.Parent = newConnectionInfo;
            newConnectionInfo.Inheritance = newInheritance;

            Runtime.ConnectionList.Add(newConnectionInfo);

            var newTreeNode = new ConnectionTreeNode(newConnectionInfo.Name);
            newTreeNode.Tag = newConnectionInfo;
            newTreeNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
            newTreeNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;

            newConnectionInfo.TreeNode = newTreeNode;

            if (parentNode == null)
            {
                oldTreeNode.Parent.Nodes.Insert(oldTreeNode.Index + 1, newTreeNode);
                ConnectionTree.Instance.SelectedNode = newTreeNode;
            }
            else
            {
                ContainerInfo parentContainerInfo = parentNode.Tag as ContainerInfo;
                if (parentContainerInfo != null)
                {
                    newConnectionInfo.Parent = parentContainerInfo;
                }
                parentNode.Nodes.Add(newTreeNode);
            }
        }
		
		public static void SetNodeImage(ConnectionTreeNode treeNode, TreeImageType Img)
		{
			SetNodeImageIndex(treeNode, (int)Img);
		}
		
        public static void RenameNode(ConnectionInfo connectionInfo, string newName)
        {
            if (newName == null || newName.Length <= 0)
                return;

            connectionInfo.Name = newName;
            if (Settings.Default.SetHostnameLikeDisplayName)
                connectionInfo.Hostname = newName;
        }
        #endregion

        #region Private Methods
        private delegate void SetNodeImageIndexDelegate(ConnectionTreeNode treeNode, int imageIndex);
        private static void SetNodeImageIndex(ConnectionTreeNode treeNode, int imageIndex)
        {
            if (treeNode == null || treeNode.TreeView == null)
            {
                return;
            }
            if (treeNode.TreeView.InvokeRequired)
            {
                treeNode.TreeView.Invoke(new SetNodeImageIndexDelegate(SetNodeImageIndex), new object[] { treeNode, imageIndex });
                return;
            }

            treeNode.ImageIndex = imageIndex;
            treeNode.SelectedImageIndex = imageIndex;
        }
        #endregion
    }
}