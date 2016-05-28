﻿using mRemoteNG.Tree;
using System.Collections;
using System.Windows.Forms;

namespace mRemoteNG.Tools.Sorting
{
    public class TreeNodeSorter : IComparer
    {
        public SortOrder Sorting { get; set; }

        public TreeNodeSorter(SortOrder sortOrder = SortOrder.None)
        {
            Sorting = sortOrder;
        }

        public int Compare(object x, object y)
        {
            ConnectionTreeNode tx = (ConnectionTreeNode)x;
            ConnectionTreeNode ty = (ConnectionTreeNode)y;

            switch (Sorting)
            {
                case SortOrder.Ascending:
                    return string.Compare(tx.Text, ty.Text);
                case SortOrder.Descending:
                    return string.Compare(ty.Text, tx.Text);
                default:
                    return 0;
            }
        }
    }
}