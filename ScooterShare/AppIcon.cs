using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ScooterShare
{
    internal static class AppIcon
    {
        private const string ResourceName = "ScooterShare.Resources.scooter.png";

        private static Bitmap _logoBitmap;
        private static Icon _windowIcon;

        public static Bitmap LogoBitmap
        {
            get
            {
                if (_logoBitmap != null)
                {
                    return _logoBitmap;
                }

                var asm = Assembly.GetExecutingAssembly();
                using (var s = asm.GetManifestResourceStream(ResourceName))
                {
                    if (s == null)
                    {
                        throw new FileNotFoundException("Embedded resource not found: " + ResourceName);
                    }

                    _logoBitmap = new Bitmap(s);
                    return _logoBitmap;
                }
            }
        }

        public static Icon WindowIcon
        {
            get
            {
                if (_windowIcon != null)
                {
                    return _windowIcon;
                }

                IntPtr hIcon = IntPtr.Zero;
                try
                {
                    hIcon = LogoBitmap.GetHicon();
                    using (var tmp = Icon.FromHandle(hIcon))
                    {
                        _windowIcon = (Icon)tmp.Clone();
                    }

                    return _windowIcon;
                }
                finally
                {
                    if (hIcon != IntPtr.Zero)
                    {
                        DestroyIcon(hIcon);
                    }
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}

