﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using TrayDir.utils;

namespace TrayDir {
	public class ITray {
		public NotifyIcon notifyIcon;

		private TrayInstance instance;
		private List<IPathMenuItem> pathMenuItems = new List<IPathMenuItem>();
		private List<IVirtualFolderMenuItem> virtualFolderMenuItems = new List<IVirtualFolderMenuItem>();
		private List<IPluginMenuItem> pluginMenuItems = new List<IPluginMenuItem>();
		private List<IWebLinkMenuItem> webLinkMenuItems = new List<IWebLinkMenuItem>();

		private ToolStripMenuItem showMenuItem;
		private ToolStripMenuItem hideMenuItem;
		private ToolStripMenuItem exitMenuItem;

		private EventHandler showForm;
		private EventHandler hideForm;
		private EventHandler exitForm;

		private Timer ClosedTimer = new Timer();
		private bool menuVisible = false;
		public List<IMenuItem> menuItems {
			get {
				List<IMenuItem> iml = new List<IMenuItem>();
				iml.AddRange(pathMenuItems);
				iml.AddRange(virtualFolderMenuItems);
				iml.AddRange(pluginMenuItems);
				iml.AddRange(webLinkMenuItems);
				return iml;
			}
		}
		public Icon icon {
			get { return notifyIcon.Icon; }
			set { notifyIcon.Icon = value; }
		}

		public ITray(TrayInstance instance) {
			this.instance = instance;
			notifyIcon = new NotifyIcon();
			notifyIcon.Visible = !instance.settings.HideFromTray;
			notifyIcon.MouseClick += notifyIcon_Click;
			UpdateTrayIcon();
			notifyIcon.DoubleClick += notifyIcon_DoubleClick;
			ClosedTimer.Tick += ClosedTimer_Tick;
		}

		private void ClosedTimer_Tick(object sender, EventArgs e) {
			menuVisible = false;
		}

		public void notifyIcon_DoubleClick(object obj, EventArgs args) {
			if (((MouseEventArgs)args).Button == MouseButtons.Left) {
				MainForm.form.onShowInstance = instance;
				MainForm.form.ShowApp(obj, args);
				notifyIcon.ContextMenuStrip.Hide();
			}
		}
		public void notifyIcon_Click(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left && ProgramData.pd.settings.app.ShowMenuOnLeftClick) {
				if (menuVisible) {
					notifyIcon.ContextMenuStrip.Hide();
				} else {
					MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
					mi.Invoke(notifyIcon, null);
				}
			}
		}
		private void Reset(Object o, EventArgs e) {
			foreach (IMenuItem m in menuItems) {
				m.ResetClicks();
			}
		}
		private ToolStripMenuItem MakeAndAddMenuItem(ToolStripMenuItem menuItem, string text, bool visible, EventHandler eh) {
			if (!(eh is null) && (menuItem == null)) {
				menuItem = new ToolStripMenuItem(text, null, eh);
				menuItem.Visible = visible;
			}
			if (menuItem != null) {
				notifyIcon.ContextMenuStrip.Items.Add(menuItem);
			}
			return menuItem;
		}
		public void setEventHandlers(EventHandler showForm, EventHandler hideForm, EventHandler exitForm) {
			this.showForm = showForm;
			this.hideForm = hideForm;
			this.exitForm = exitForm;
		}
		public void Rebuild() {
			if (MainForm.form != null) {
				ClearList(pathMenuItems.ConvertAll<IMenuItem>(m => (IMenuItem)m));
				pathMenuItems.Clear();
				ClearList(pluginMenuItems.ConvertAll<IMenuItem>(m => (IMenuItem)m));
				pluginMenuItems.Clear();
				ClearList(virtualFolderMenuItems.ConvertAll<IMenuItem>(m => (IMenuItem)m));
				virtualFolderMenuItems.Clear();
				BuildTrayMenu();
				GC.Collect();
			}
		}
		private void ClearList(List<IMenuItem> list) {
			foreach (IMenuItem item in list) {
				item.Clear();
			}
			list.Clear();
		}
		public void MenuOpened(Object obj, EventArgs args) {
			ClosedTimer.Stop();
			menuVisible = true;
			IMenuItemIconUtils.AssignIcons();
			foreach (IMenuItem child in pathMenuItems) {
				child.MenuOpened();
			}
			foreach (IMenuItem child in pluginMenuItems) {
				child.MenuOpened();
			}
			foreach (IMenuItem child in virtualFolderMenuItems) {
				child.MenuOpened();
			}
			MainForm.form.iconLoadTimer.Start();
		}
		public void MenuClosed(Object obj, EventArgs args) {
			MainForm.form.iconLoadTimer.Stop();
			Reset(obj, args);
			ClosedTimer.Start();
		}
		public void RefreshPluginMenuItemList() {
			foreach (TrayInstancePlugin tiPlugin in instance.plugins) {
				bool miFound = false;
				foreach (IMenuItem mi in pluginMenuItems) {
					if (mi.tiItem == tiPlugin) miFound = true;
				}
				if (!miFound) {
					IPluginMenuItem mi = new IPluginMenuItem(instance, null, tiPlugin, null);
					pluginMenuItems.Add(mi);
				}
			}
			List<IPluginMenuItem> deletable = new List<IPluginMenuItem>();
			foreach (IPluginMenuItem mi in pluginMenuItems) {
				bool miFound = false;
				foreach (TrayInstancePlugin tiPlugin in instance.plugins) {
					if (mi.tiItem == tiPlugin) miFound = true;
				}
				if (miFound) {
					mi.Load();
				} else {
					deletable.Add(mi);
				}
			}
			foreach (IPluginMenuItem mi in deletable) {
				pluginMenuItems.Remove(mi);
			}
		}
		public void RefreshVirtualFolderMenuItemList() {
			foreach (TrayInstanceVirtualFolder tiVirtualFolder in instance.vfolders) {
				bool miFound = false;
				foreach (IMenuItem mi in virtualFolderMenuItems) {
					if (mi.tiItem == tiVirtualFolder) miFound = true;
				}
				if (!miFound) {
					IVirtualFolderMenuItem mi = new IVirtualFolderMenuItem(instance, null, tiVirtualFolder, null);
					virtualFolderMenuItems.Add(mi);
				}
			}
			List<IVirtualFolderMenuItem> deletable = new List<IVirtualFolderMenuItem>();
			foreach (IVirtualFolderMenuItem mi in virtualFolderMenuItems) {
				bool miFound = false;
				foreach (TrayInstanceVirtualFolder tiVirtualFolder in instance.vfolders) {
					if (mi.tiItem == tiVirtualFolder) miFound = true;
				}
				if (miFound) {
					mi.Load();
				} else {
					deletable.Add(mi);
				}
			}
			foreach (IVirtualFolderMenuItem mi in deletable) {
				virtualFolderMenuItems.Remove(mi);
			}
		}
		public void RefreshWebLinkMenuItemList() {
			foreach (TrayInstanceWebLink tiWebLink in instance.weblinks) {
				bool miFound = false;
				foreach (IMenuItem mi in webLinkMenuItems) {
					if (mi.tiItem == tiWebLink) miFound = true;
				}
				if (!miFound) {
					IWebLinkMenuItem mi = new IWebLinkMenuItem(instance, null, tiWebLink, null);
					webLinkMenuItems.Add(mi);
				}
			}
			List<IWebLinkMenuItem> deletable = new List<IWebLinkMenuItem>();
			foreach (IWebLinkMenuItem mi in webLinkMenuItems) {
				bool miFound = false;
				foreach (TrayInstanceWebLink tiVirtualFolder in instance.weblinks) {
					if (mi.tiItem == tiVirtualFolder) miFound = true;
				}
				if (miFound) {
					mi.Load();
				} else {
					deletable.Add(mi);
				}
			}
			foreach (IWebLinkMenuItem mi in deletable) {
				webLinkMenuItems.Remove(mi);
			}
		}
		public void RefreshPathMenuItemList() {
			foreach (TrayInstancePath tiPath in instance.paths) {
				bool miFound = false;
				foreach (IPathMenuItem mi in pathMenuItems) {
					if (mi.tiItem == tiPath) miFound = true;
				}
				if (!miFound) {
					IPathMenuItem mi = new IPathMenuItem(instance, null, tiPath, null);
					pathMenuItems.Add(mi);
				}
			}
			List<IMenuItem> deletable = new List<IMenuItem>();
			foreach (IPathMenuItem mi in pathMenuItems) {
				bool miFound = false;
				foreach (TrayInstancePath tiPath in instance.paths) {
					if (mi.tiItem == tiPath) miFound = true;
				}
				if (miFound) {
					mi.Load();
				} else {
					deletable.Add(mi);
				}
			}
			foreach (IPathMenuItem mi in deletable) {
				pathMenuItems.Remove(mi);
			}
		}
		public void BuildTrayMenu() {
			if (notifyIcon.ContextMenuStrip is null) {
				notifyIcon.ContextMenuStrip = new ContextMenuStrip();
				notifyIcon.ContextMenuStrip.Opened += MenuOpened;
				notifyIcon.ContextMenuStrip.Closed += MenuClosed;
			} else {
				notifyIcon.ContextMenuStrip.Items.Clear();
			}

			//            notifyIcon.ContextMenuStrip.AutoClose = true;

			showMenuItem = MakeAndAddMenuItem(showMenuItem, Properties.Strings.MenuItem_Show, false, showForm);
			hideMenuItem = MakeAndAddMenuItem(hideMenuItem, Properties.Strings.MenuItem_Hide, true, hideForm);

			notifyIcon.ContextMenuStrip.Items.Add("-");

			RefreshPathMenuItemList();
			RefreshVirtualFolderMenuItemList();
			RefreshWebLinkMenuItemList();
			RefreshPluginMenuItemList();

			AddTrayTree(instance.nodes.children, notifyIcon.ContextMenuStrip.Items, null);

			notifyIcon.ContextMenuStrip.Items.Add("-");
			exitMenuItem = MakeAndAddMenuItem(exitMenuItem, Properties.Strings.MenuItem_Exit, true, exitForm);

			UpdateTrayIcon();

			foreach (IMenuItem mi in menuItems) {
				mi.EnqueueImgLoad();
				mi.UpdateVisibility();
			}
		}
		private void AddTrayTree(List<TrayInstanceNode> nodes, ToolStripItemCollection collection, IMenuItem parent) {
			foreach (TrayInstanceNode node in nodes) {
				switch (node.type) {
					case TrayInstanceNode.NodeType.Path:
						if (node.id < instance.paths.Count) {
							foreach (IPathMenuItem mi in pathMenuItems) {
								if (mi.tiItem == instance.paths[node.id]) {
									mi.tiNode = node;
									if (parent != null) parent.nodeChildren.Add(mi);
									if (!((TrayInstancePath)mi.tiItem).shortcut && (instance.settings.ExpandFirstPath && nodes.Count == 1 && collection == notifyIcon.ContextMenuStrip.Items)) {
										mi.AddToCollectionExpanded(collection);
									} else {
										mi.AddToCollection(collection);
									}
									break;
								}
							}
						}
						break;
					case TrayInstanceNode.NodeType.Plugin:
						if (node.id < instance.plugins.Count) {
							foreach (IPluginMenuItem mi in pluginMenuItems) {
								if (mi.tiItem == instance.plugins[node.id]) {
									mi.tiNode = node;
									if (parent != null) parent.nodeChildren.Add(mi);
									mi.AddToCollection(collection);
									AddTrayTree(node.children, mi.menuItem.DropDownItems, mi);
									break;
								}
							}
						}
						break;
					case TrayInstanceNode.NodeType.VirtualFolder:
						if (node.id < instance.vfolders.Count) {
							foreach (IVirtualFolderMenuItem mi in virtualFolderMenuItems) {
								if (mi.tiItem == instance.vfolders[node.id]) {
									mi.tiNode = node;
									if (parent != null) parent.nodeChildren.Add(mi);
									mi.AddToCollection(collection);
									AddTrayTree(node.children, mi.menuItem.DropDownItems, mi);
									break;
								}
							}
						}
						break;
					case TrayInstanceNode.NodeType.Separator:
						collection.Add("-");
						break;
					case TrayInstanceNode.NodeType.WebLink:
						if (node.id < instance.weblinks.Count) {
							foreach (IWebLinkMenuItem mi in webLinkMenuItems) {
								if (mi.tiItem == instance.weblinks[node.id]) {
									mi.tiNode = node;
									if (parent != null) parent.nodeChildren.Add(mi);
									mi.AddToCollection(collection);
									AddTrayTree(node.children, mi.menuItem.DropDownItems, mi);
									break;
								}
							}
						}
						break;
				}
			}
		}
		public void UpdateTrayIcon() {
			Icon i = this.GetInstanceIcon();
			if (i != null && ((instance.iconPath != null && instance.iconPath != string.Empty) || (instance.iconData == null))) {
				notifyIcon.Icon = i;
				instance.iconData = TrayUtils.IconToBytes(i);
			}
			instance.iconPath = null;
			notifyIcon.Text = instance.instanceName;
			notifyIcon.Icon = i;
		}
		public void Hide() {
			notifyIcon.Visible = false;
		}
		public void Show() {
			notifyIcon.Visible = true;
		}
		public void SetFormHiddenMenu() {
			notifyIcon.ContextMenuStrip.Items[0].Visible = true;
			notifyIcon.ContextMenuStrip.Items[1].Visible = false;
		}
		public void SetFormShownMenu() {
			showMenuItem.Visible = false;
			hideMenuItem.Visible = true;
		}
		public Icon GetInstanceIcon() {
			Icon i;
			try {
				if (instance.iconData != null) {
					i = TrayUtils.BytesToIcon(instance.iconData);
				} else if (AppUtils.PathIsFile(instance.iconPath)) {
					i = Icon.ExtractAssociatedIcon(instance.iconPath);
				} else {
					i = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().Location);
				}
			}
			catch (Exception e) {
				MessageBox.Show(String.Format(Properties.Strings.Form_ErrorLoadingIcon, e.Message));
				i = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().Location);
			}
			return i;
		}

		internal void SetText(string text) {
			notifyIcon.Text = text;
		}
	}
}
