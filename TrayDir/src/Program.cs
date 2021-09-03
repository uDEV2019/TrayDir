﻿using System;
using System.Windows.Forms;

namespace TrayDir
{
    static class Program
    {
        public static bool DEBUG = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool running = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1;
            if (!running || MessageBox.Show("Another process of TrayDir is already running. Do you want to start a new one?","Already running",MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    MainForm.Init();
                    SettingsForm.Init();
                    ProgramData.pd.initialized = true;
                    Application.Run(MainForm.form);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Unhandled Fatal Exception: " + e.Message + "[" + e.StackTrace + "]");
                }
            }
        }
    }
}
