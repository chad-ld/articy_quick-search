using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using GlobalLowLevelHooks;
using System;
using Articy.Utils.Manifests;
using System.Runtime.InteropServices;
using System.Text;

using Articy.Api;
using Articy.Api.Plugins;

//using Texts = LIds.LastDragonMedia.TabToFocus;

namespace LastDragonMedia.TabToFocus
{
    /// <summary>
    /// public implementation part of plugin code, contains all overrides of the plugin class.
    /// </summary>
    public partial class Plugin : MacroPlugin
    {
        public override string DisplayName => "Hotkey to Focus";

        public override string ContextName => "Hotkey to Focus";

        KeyboardHook keyboardHook = new KeyboardHook();
        MouseHook mouseHook = new MouseHook();

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public Plugin()
        {
            //this runs once when the plugin is initialized (which is every time a project is opened)
            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            keyboardHook.Install();
            mouseHook.Install();

            Register(KeyboardHook.VKeys.OEM_3, HotkeyCommand);
        }

        ~Plugin() {
            keyboardHook.Uninstall();
            mouseHook.Uninstall();

            commands.Clear();
        }


        private Dictionary<KeyboardHook.VKeys, Action> commands = new Dictionary<KeyboardHook.VKeys, Action>();
        internal void Register(KeyboardHook.VKeys key, Action command)
        {
            commands.Add(key, command);
        }


        public override string Initialize()
        {
            return base.Initialize();
        }

        private void KeyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            if (commands.ContainsKey(key))
            {
                string windowName = GetActiveWindowTitle();
                if (windowName != null)
                    if (windowName.ToLower().IndexOf("articy:draft") != -1)
                    {
                        ApplicationManifest.GetUiDispatcher().BeginInvoke((Action)(() =>
                        {
                            commands[key]();
                        }));
                    }
            }
        }

        private void SelectAfter(AsyncTask task)
        {

        }

        public override List<MacroCommandDescriptor> GetMenuEntries(List<ObjectProxy> aSelectedObjects, ContextMenuContext aContext)
        {
            List<MacroCommandDescriptor> result = new List<MacroCommandDescriptor>();
            switch (aContext)
            {
                case ContextMenuContext.Global:
                    // entries for the "global" commands of the ribbon menu are requested
                    return result;
                case ContextMenuContext.ContentArea:
                    MacroCommandDescriptor cmd = new MacroCommandDescriptor
                    {
                        CaptionLid = "This is a new context menu entry!",
                        Execute = Command,
                    };
                    result.Add(cmd);
                    return result;
                case ContextMenuContext.Other:
                    return result;
                case ContextMenuContext.Navigator:
                    return result;
                case ContextMenuContext.Search:
                    return result;
                case ContextMenuContext.Conflicts:
                    return result;
                default:
                    // normal context menu when working in the content area, navigator, search
                    return result;
            }
        }

        private void Command(MacroCommandDescriptor aDescriptor, List<ObjectProxy> aSelectedObjects)
        {
            Debug.Write("got here");
            //PopupNav navWindow = new PopupNav();
            ////Session.ShowDialog(navWindow);
            //ObjectProxy o = Session.GetProjectRoot();
            //Session.BringObjectIntoView(o);

            //AsyncTask task = new AsyncTask(null,)
            //                PopupNav navWindow = new PopupNav();
            //                MouseHook.POINT point = MouseHook.GetCursorPosition();
            //                Debug.WriteLine(point.X);
            //                Debug.WriteLine(point.Y);
            //                Debug.WriteLine(MouseHook.GetScalingFactor());
            //                navWindow.Top = point.Y;
            //                navWindow.Left = point.X;

        }
        private void HotkeyCommand()
        {
            ObjectProxy o = null;
            Session.SelectObject(ref o, aFilter: FilterSearchObject);
            Session.BringObjectIntoView(o);
        }

        private bool FilterSearchObject(ObjectProxy aObject)
        {
            //return all objects as available to be searched
            return true;
        }

        public override Brush GetIcon(string aIconName)
        {
            switch (aIconName)
            {
                // if you have specified the "IconFile" in the PluginManifest.xml you don't need this case
                // unless you want to have an icon that differs when the plugin is loaded from the non-loaded case
                // or you want to put all icons within the resources of your plugin assembly
                /*
                case "$self":
                    // get the main icon for the plugin
                    return Session.CreateBrushFromFile(Manifest.ManifestPath+"Resources\\Icon.png");
                */
                default:
                    break;
            }
            return null;
        }
    }
}
