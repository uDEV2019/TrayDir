﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TrayDir
{
    class AppUtils
    {
        public static ToolStripMenuItem RecursivePathFollow(TrayInstanceSettings settings, string path)
        {
            return RecursivePathFollow(settings, path, 0);
        }
        public static ToolStripMenuItem RecursivePathFollow(TrayInstanceSettings settings, string path, int depth)
        {
            ToolStripMenuItem menuitem = new ToolStripMenuItem();
            menuitem.Size = new Size(359, 44);
            menuitem.Text = "";
            try
            {
                //detect whether its a directory or file
                if (PathIsDirectory(path))
                {
                    menuitem.Text = new DirectoryInfo(path).Name;
                    if (depth < 4)
                    {
                        foreach (string fp in Directory.GetFileSystemEntries(path))
                        {
                            menuitem.DropDownItems.Add(RecursivePathFollow(settings, fp));
                        }
                    }
                }
                else if (PathIsFile(path))
                {
                    menuitem.Text = Path.GetFileName(path);
                    if (settings.ShowFileExtensions)
                    {
                        menuitem.Text = Path.GetFileName(path);
                    }
                    else
                    {
                        menuitem.Text = Path.GetFileNameWithoutExtension(path);
                    }
                }
                else
                {
                    MessageBox.Show("Problem Parsing " + path);
                }
            }
            catch
            {
                MessageBox.Show("Problem Parsing " + path);
            }


            EventHandler textbox_select = new EventHandler(delegate (object obj, EventArgs args)
            {
                if (PathIsDirectory(path) & settings.ExploreFoldersInTrayMenu)
                {
                    OpenPath(new DirectoryInfo(path).FullName, settings.RunAsAdmin);
                }
                else if (PathIsFile(path))
                {
                    OpenPath(Path.GetFullPath(path), settings.RunAsAdmin);
                }
            });

            menuitem.Click += textbox_select;
            return menuitem;
        }
        public static bool PathIsDirectory(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                //detect whether its a directory or file
                return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
            }
            catch
            {
                return false;
            }

        }
        public static bool PathIsFile(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                //detect whether its a directory or file
                return !((attr & FileAttributes.Directory) == FileAttributes.Directory);
            }
            catch
            {
                return false;
            }
        }
        public static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
        public static void OpenPath(string path, bool runAsAdmin)
        {
            try
            {
                if (runAsAdmin)
                {
                    ExecuteAsAdmin(path);
                }
                else
                {
                    Process.Start(path);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Opening: " + path + '\n' + e.Message);
            }
        }
        public static void ExplorePath(string path)
        {
            try
            {
                if (PathIsFile(path))
                {
                    Process.Start("explorer.exe", new FileInfo(path).Directory.FullName);
                }
                else
                {
                    Process.Start("explorer.exe", path);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Exploring: " + path + '\n' + e.Message);
            }
        }
        private static void ExecuteAsAdmin(string path)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = path;
            proc.StartInfo.Verb = "runas";
            try
            {
                if (PathIsFile(path))
                {
                    proc.Start();
                }
                else
                {
                    Process.Start("explorer.exe", path);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Executing As Admin: " + path + '\n' + e.Message);
            }
        }
        public static bool StrToBool(string value)
        {
            return value == "1" ? true : false;
        }
    }
}
