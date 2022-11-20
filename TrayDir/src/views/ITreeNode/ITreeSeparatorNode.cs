﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrayDir.utils;

namespace TrayDir.src.views {
	internal class ITreeSeparatorNode : ITreeNode {
		internal ITreeSeparatorNode(TrayInstanceNode tin) : base(tin) {
		}

		internal override void Refresh() {
			bool hidden = false;
			node.ImageIndex = IconUtils.QUESTION;
			node.ImageIndex = IconUtils.SEPARATOR;
			node.Text = Properties.Strings.Form_Separator;
			node.SelectedImageIndex = node.ImageIndex;
			if (hidden && node.TreeView != null) {
				ITreeNode.strikethroughFont = new Font(node.TreeView.Font.FontFamily, node.TreeView.Font.Size, FontStyle.Strikeout);
				node.NodeFont = strikethroughFont;
			} else {
				node.NodeFont = node.TreeView?.Font;
			}
		}
	}
}
