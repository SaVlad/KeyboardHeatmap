using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardHeatmap {
	public class MouseHook {
		public event Action<MouseButtons> KeyPressed;
		private IntPtr hHook;
		private LowLevelMouseProc dCallback;

		public MouseHook() {
			dCallback = MouseCallback;
			using(Process process = Process.GetCurrentProcess())
			using(ProcessModule module = process.MainModule)
				hHook = SetWindowsHookEx(WH_MOUSE_LL, dCallback, GetModuleHandle(module.ModuleName), 0);
		}
		private IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if(nCode >= 0) {
				//Debug.WriteLine("Mouse clicked");
				//Debug.WriteLine("wParam: " + Convert.ToString((int)wParam, 2));
				if(wParam == (IntPtr) WM_LBUTTONUP)
					KeyPressed?.Invoke(MouseButtons.Left);
				else if(wParam == (IntPtr) WM_RBUTTONUP)
					KeyPressed?.Invoke(MouseButtons.Right);
			}
			return CallNextHookEx(hHook, nCode, wParam, lParam);
		}
		public void Dispose() => UnhookWindowsHookEx(hHook);

		private const int WM_LBUTTONUP = 0x0202;
		private const int WM_RBUTTONUP = 0x0205;
		private const int WH_MOUSE_LL = 14;
		private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}
