using System;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;

namespace HuffmanCoder
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void fileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                textBox.Text = openFileDialog.FileName;
        }

        private void codeButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(textBox.Text))
            {
                byte[] data;
                long fSize;
                fSize = new System.IO.FileInfo(textBox.Text).Length;
                data = new byte[fSize];
                data = File.ReadAllBytes(textBox.Text);

                Program.CodeFile(data, textBox.Text);
            }
            else
                System.Windows.MessageBox.Show("Nie zaznaczyłeś żadnego pliku do zakodowania.");
        }

        private void decodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(textBox.Text))
            {
                byte[] data;
                long fSize;

                fSize = new System.IO.FileInfo(textBox.Text).Length;
                data = new byte[fSize];
                data = File.ReadAllBytes(textBox.Text);

                Program.DecodeFile(data, textBox.Text);
            }
            else
                System.Windows.MessageBox.Show("Nie zaznaczyłeś żadnego pliku do zdekodowania.");
        }
        public static class IconHelper
        {
            [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hwnd, int index);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x,
        int y, int width, int height, uint flags);

            [DllImport("user32.dll")]
            static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr
        lParam);

            const int GWL_EXSTYLE = -20;
            const int WS_EX_DLGMODALFRAME = 0x0001;
            const int SWP_NOSIZE = 0x0001;
            const int SWP_NOMOVE = 0x0002;
            const int SWP_NOZORDER = 0x0004;
            const int SWP_FRAMECHANGED = 0x0020;
            const uint WM_SETICON = 0x0080;

            public static void RemoveIcon(Window window)
            {
                // Get this window's handle
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                // Change the extended window style to not show a window icon
                int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
                // Update the window's non-client area to reflect the changes
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE |
        SWP_NOZORDER | SWP_FRAMECHANGED);
            }
        }
    }
}
