using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    public static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    const uint WM_PAINT = 0x000F;

    [DllImport("user32.dll")]
    public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    const uint WM_GETTEXT = 0x000D;
    static readonly IntPtr HWND_TOP = IntPtr.Zero;
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOZORDER = 0x0004;

    private static void Main(string[] args)
    {
        DisableConsoleQuickEdit.Go();
        Console.Title = "OurProgram - Working";

        string tempFile = Path.Combine(Path.GetTempPath(), $"edit{DateTime.Now.ToString("s").Replace(":", ".")}.txt");
        File.WriteAllText(tempFile, "Type your text here and Save and exit to continue!");

        Console.WriteLine("Hello, World!"[1..^2]);
        string path = @"C:\WINDOWS\system32\notepad.exe";
        Process proc = Process.Start(path, tempFile);
        proc.WaitForInputIdle();

        while (proc.MainWindowHandle == IntPtr.Zero && !proc.HasExited)
        {
            Thread.Sleep(100);
            proc.Refresh();
        }

        if (proc.HasExited)
        {
            Console.WriteLine("Program existed");
            Console.ReadLine();
            Environment.Exit(0);
        }

        IntPtr hConsoleHandle = FindWindowByCaption(IntPtr.Zero, Console.Title);
        IntPtr hNotepadHandle = proc.MainWindowHandle;

        SetParent(hNotepadHandle, hConsoleHandle);

        RECT rect;
        GetWindowRect(hConsoleHandle, out rect);
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        SetWindowPos(hNotepadHandle, HWND_TOP, 0, 60, width-20, height, SWP_NOZORDER);

        //SendMessage(hNotepadHandle, WM_PAINT, IntPtr.Zero, IntPtr.Zero);
        proc.WaitForExit();
        proc.Kill();

        string result = File.ReadAllText(tempFile);
        File.Delete(tempFile);

        Console.Clear();

        Console.WriteLine(result);
        Console.WriteLine("Done");
        Console.ReadLine();
    }
}
