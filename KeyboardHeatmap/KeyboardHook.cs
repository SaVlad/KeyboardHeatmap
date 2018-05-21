using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardHeatmap {
	public class KeyboardHook:IDisposable {
		public event Action<Keys> KeyPressed;
		public event Action Emergency;
		private IntPtr hHook;
		private KeyBoardProc dCallback;

		public KeyboardHook() {
			dCallback = KeyboardCallback;
			using(Process process = Process.GetCurrentProcess())
			using(ProcessModule module = process.MainModule)
				hHook = SetWindowsHookEx(WH_KEYBOARD_LL, dCallback, GetModuleHandle(module.ModuleName), 0);
		}
		private IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if(nCode >= 0) {
				Keys k = (Keys) Marshal.ReadInt32(lParam);
				if(wParam == (IntPtr) WM_SYSKEYUP && k == Keys.Pause)
					Emergency?.Invoke();
				else if(wParam == (IntPtr) WM_KEYUP)
					KeyPressed?.Invoke(k);
			}
			return CallNextHookEx(hHook, nCode, wParam, lParam);
		}
		public void Dispose() => UnhookWindowsHookEx(hHook);

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYUP = 0x0105;
		private delegate IntPtr KeyBoardProc(int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, KeyBoardProc lpfn, IntPtr hMod, uint dwThreadId);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}
