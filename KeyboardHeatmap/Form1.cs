using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace KeyboardHeatmap {
	public partial class Form1:Form {
		const int max_percent = 10;
		private const string AppName = "KeyboardHeatmap";
		private static string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "keyboard_heatmap.data");
		private static Color[] gradient;
		private KeyboardHook kHook;
		private MouseHook mHook;
		private Control lastClick;
		private long total = 0;
		private string[] names;
		private Keys[] keys;
		private Key[] elements;
		private long[] counter;
		private long[] mouse = new long[5];
		private int[] top = new[] { 0, 1, 2, 3, 4 };
		private Tuple<Label, Label>[] topLabels;
		private Comparison<int> comparison;
		private bool dirty = false;
		private bool busy = false;

		public Form1() {
			InitializeComponent();
			RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			object val = key.GetValue(AppName);
			if(val != null) {
				if(val.ToString() == ExePath())
					checkBox1.Checked = true;
				else
					key.DeleteValue(AppName);
			}
			notifyIcon1.Icon = SystemIcons.Application;
			names = new string[] {
				"Escape","F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11",
				"F12","Tilde","1","2","3","4","5","6","7","8","9","0",
				"Minus","Equal","Backslash","Backspace","Tab","Q","W","E","R","T","Y","U",
				"I","O","P","Left Square","Right Square","CapsLock","A","S","D","F","G",
				"H","J","K","L","Seicolon","Quote","Left Shift","Z","X","C","V","B","N",
				"M","Left Angle","Right Angle","?","Right shift","Left Ctrl","Left Win","Space",
				"Right Win","Apps","Right Control","PtrScr","ScrLock","Pause","Insert","Home",
				"PageUp","Delete","End","Next","Up","Left","Down","Right","NumLock",
				"Divide","Multiply","Subtract","NumPad 7","NumPad 8","NumPad 9","Add","NumPad 4",
				"NumPad 5","NumPad 6","NumPad 1","NumPad 2","NumPad 3","Return","NumPad 0","Numpad Decimal",
				"Left Alt","Right Alt"
			};
			keys = new Keys[] {
				Keys.Escape,Keys.F1,Keys.F2,Keys.F3,Keys.F4,Keys.F5,Keys.F6,Keys.F7,Keys.F8,Keys.F9,Keys.F10,Keys.F11,
				Keys.F12,Keys.Oemtilde,Keys.D1,Keys.D2,Keys.D3,Keys.D4,Keys.D5,Keys.D6,Keys.D7,Keys.D8,Keys.D9,Keys.D0,
				Keys.OemMinus,Keys.Oemplus,Keys.Oem5,Keys.Back,Keys.Tab,Keys.Q,Keys.W,Keys.E,Keys.R,Keys.T,Keys.Y,Keys.U,
				Keys.I,Keys.O,Keys.P,Keys.OemOpenBrackets,Keys.Oem6,Keys.Capital,Keys.A,Keys.S,Keys.D,Keys.F,Keys.G,
				Keys.H,Keys.J,Keys.K,Keys.L,Keys.Oem1,Keys.Oem7,Keys.LShiftKey,Keys.Z,Keys.X,Keys.C,Keys.V,Keys.B,Keys.N,
				Keys.M,Keys.Oemcomma,Keys.OemPeriod,Keys.OemQuestion,Keys.RShiftKey,Keys.LControlKey,Keys.LWin,Keys.Space,
				Keys.RWin,Keys.Apps,Keys.RControlKey,Keys.PrintScreen,Keys.Scroll,Keys.Pause,Keys.Insert,Keys.Home,
				Keys.PageUp,Keys.Delete,Keys.End,Keys.Next,Keys.Up,Keys.Left,Keys.Down,Keys.Right,Keys.NumLock,
				Keys.Divide,Keys.Multiply,Keys.Subtract,Keys.NumPad7,Keys.NumPad8,Keys.NumPad9,Keys.Add,Keys.NumPad4,
				Keys.NumPad5,Keys.NumPad6,Keys.NumPad1,Keys.NumPad2,Keys.NumPad3,Keys.Return,Keys.NumPad0,Keys.Decimal,
				Keys.LMenu, Keys.RMenu
			};
			elements = new Key[] {
				kEsc,kF1,kF2,kF3,kF4,kF5,kF6,kF7,kF8,kF9,kF10,kF11,kF12,kTilde,k1,k2,k3,k4,k5,k6,k7,k8,k9,k0,kDMinus,
				kEqual,kBackSlash,kBack,kTab,kQ,kW,kE,kR,kT,kY,kU,kI,kO,kP,kLB,kRB,kCapsLk,kA,kS,kD,kF,kG,kH,kJ,kK,kL,
				kColon,kQuote,kLShift,kZ,kX,kC,kV,kB,kN,kM,kALB,kARB,kSlash,kRShift,kLCtrl,kLWin,kSpace,
				kRWin,kApps,kRCtrl,kPrtScr,kScrLk,kPaus,kIns,kHom,kPgUp,kDel,kEnd,kPgDn,kUp,kLeft,kDown,kRight,kNumLk,
				kDivide,kMultiply,kMinus,kN7,kN8,kN9,kPlus,kN4,kN5,kN6,kN1,kN2,kN3,kNEnter,kN0,kNDot,kLAlt,kRAlt
			};
			topLabels = new[] {
				new Tuple<Label, Label>(lblKey1, lblCount1),
				new Tuple<Label, Label>(lblKey2, lblCount2),
				new Tuple<Label, Label>(lblKey3, lblCount3),
				new Tuple<Label, Label>(lblKey4, lblCount4),
				new Tuple<Label, Label>(lblKey5, lblCount5),
			};
			gradient = new Color[max_percent];
			for(int i = 0; i < max_percent; ++i)
				gradient[i] = Color.FromArgb(255,
					(int) (255 - 255 / (float) (max_percent - 1) * i),
					(int) (255 - 255 / (float) (max_percent - 1) * i));
			counter = new long[keys.Length];
			if(keys.Length != elements.Length)
				MessageBox.Show(this, "Keys length: " + keys.Length + "\r\nElements length: " + elements.Length);
			comparison = (a, b) => counter[a].CompareTo(counter[b]);
			lastClick = kEsc;
			try {
				kHook = new KeyboardHook();
				kHook.KeyPressed += KeyPressed;
				kHook.Emergency += EmergencyPressed;
			} catch(Win32Exception we) {
				Error("Hook exception", "Windows rejected keyboard hook. Try clicking \"Rehook\"\r\n" + we.Message);
			}
			try {
				mHook = new MouseHook();
				mHook.KeyPressed += MousePressed;
			} catch(Win32Exception we) {
				Error("Hook exception", "Windows rejected mouse hook. Try clicking \"Rehook\"\r\n" + we.Message);
			}
			LoadCount(DefaultPath);
			SaveCount(DefaultPath);
			UpdateLabels();
			timer1.Start();
		}
		private string ExePath()
			=> "\"" + Assembly.GetEntryAssembly().Location + "\"";
		private void Error(string caption, string text)
			=> MessageBox.Show(this, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		private long GetGradientIndex(long count)
			=> total == 0 ? 0 : Math.Min(max_percent - 1, (long) Math.Floor(100d * count / total));

		private void OpenWindow() {
			notifyIcon1.Visible = false;
			Show();
		}
		private void CloseWindow() {
			notifyIcon1.Visible = true;
			Hide();
		}

		private void Export(string path) {
			try {
				using(FileStream fs = File.Create(path))
				using(TextWriter tw = new StreamWriter(fs)) {
					tw.WriteLine("Code,Name,Presses");
					tw.WriteLine("{0},{1},{2}", (int) MouseButtons.Left, "LMB", mouse[0]);
					tw.WriteLine("{0},{1},{2}", (int) MouseButtons.Middle, "MMB", mouse[1]);
					tw.WriteLine("{0},{1},{2}", (int) MouseButtons.Right, "RMB", mouse[2]);
					tw.WriteLine("{0},{1},{2}", (int) MouseButtons.Right, "XButton1", mouse[3]);
					tw.WriteLine("{0},{1},{2}", (int) MouseButtons.Right, "XButton2", mouse[4]);
					int[] t = new int[counter.Length];
					for(int i = 0; i < t.Length; ++i)
						t[i] = i;
					BubbleSort(ref t, comparison);
					foreach(int i in t)
						tw.WriteLine("{0},{1},{2}", (int) keys[i], names[i], counter[i]);
				}
			} catch(IOException ioe) {
				Error("IO error", "Error while writing file\r\n" + ioe.Message);
			}
		}
		private void Import(string path) {
			int i = 0;
			try {
				using(FileStream fs = File.OpenRead(path))
				using(TextReader tr = new StreamReader(fs)) {
					tr.ReadLine();
					string[] line;
					int index;
					long count;
					int offset = 0;
					for(i = 0; i < counter.Length + 3; ++i) {
						line = tr.ReadLine().Split(',');
						index = int.Parse(line[0]);
						count = long.Parse(line[2]);
						if(index == (int) MouseButtons.Left) {
							mouse[0] = count;
							offset++;
						} else if(index == (int) MouseButtons.Middle) {
							mouse[1] = count;
							offset++;
						} else if(index == (int) MouseButtons.Right) {
							mouse[2] = count;
							offset++;
						} else if(index == (int) MouseButtons.XButton1) {
							mouse[3] = count;
							offset++;
						} else if(index == (int) MouseButtons.XButton2) {
							mouse[4] = count;
							offset++;
						} else
							total += (counter[index] = count);
						if((i + offset) < 5)
							top[(i - offset)] = index;
					}
				}
			} catch(FileNotFoundException) {
				Error("Failed to import file", "File not found");
			} catch(IOException ioe) {
				Error("IO error", "Error while reading file\r\n" + ioe.Message);
			} catch(OutOfMemoryException) {
				Error("Not enough memory", "Failed to import file because program ran out of available memory");
			} catch(FormatException) {
				Error("Invalid data", "File is corrupted. Number not found where expected at line " + (i + 1));
			} catch(Exception exc) {
				Error("Unknown exception", "Failed to import from file due to unknown error\r\n" + exc);
			}
		}

		private void SaveCount(string path) {
			dirty = true;
			if(busy)
				return;
			dirty = false;
			try {
				using(FileStream fs = File.Create(path))
				using(DeflateStream ds = new DeflateStream(fs, CompressionMode.Compress))
				using(BinaryWriter bw = new BinaryWriter(ds)) {
					fs.Write(new byte[] { 75, 72, 77, 68 }, 0, 4); //KHMD
					bw.Write(top[0]);
					bw.Write(top[1]);
					bw.Write(top[2]);
					bw.Write(top[3]);
					bw.Write(top[4]);
					foreach(long count in counter)
						bw.Write(count);
					bw.Write(mouse[0]);
					bw.Write(mouse[1]);
					bw.Write(mouse[2]);
					bw.Write(mouse[3]);
					bw.Write(mouse[4]);
				}
			} catch(IOException ioe) {
				Error("IO error", "Error while writing file\r\n" + ioe.Message);
			}
			busy = false;
			if(dirty)
				SaveCount(path);
		}
		private void LoadCount(string path) {
			try {
				if(!File.Exists(path))
					return;
				using(FileStream fs = File.OpenRead(path))
				using(DeflateStream ds = new DeflateStream(fs, CompressionMode.Decompress))
				using(BinaryReader br = new BinaryReader(ds)) {
					fs.Read(new byte[4], 0, 4);
					top[0] = br.ReadInt32();
					top[1] = br.ReadInt32();
					top[2] = br.ReadInt32();
					top[3] = br.ReadInt32();
					top[4] = br.ReadInt32();
					for(int i = 0; i < counter.Length; ++i)
						total += (counter[i] = br.ReadInt64());
					mouse[0] = br.ReadInt64();
					mouse[1] = br.ReadInt64();
					mouse[2] = br.ReadInt64();
					mouse[3] = br.ReadInt64();
					mouse[4] = br.ReadInt64();
				}
			} catch(IOException ioe) {
				Error("IO error", "Error while reading file\r\n" + ioe.Message);
			}
		}

		private void BubbleSort<T>(ref T[] array, Comparison<T> comp) where T : IComparable<T> {
			bool sorted = false;
			while(!sorted) {
				sorted = true;
				for(int i = 0; i < array.Length - 1; ++i)
					if(comp(array[i], array[i + 1]) < 0) {
						T t = array[i];
						array[i] = array[i + 1];
						array[i + 1] = t;
						sorted = false;
					}
			}
		}
		private void CalculateTop(int index) {
			int[] t;
			if(Array.Exists(top, i => i == index))
				t = new[] { top[0], top[1], top[2], top[3], top[4] };
			else
				t = new[] { top[0], top[1], top[2], top[3], top[4], index };
			BubbleSort(ref t, comparison);
			top = t;

		}
		private void UpdateLabels() {
			int index = Array.FindIndex(elements, k => k == lastClick);
			lblCode.Text = "0x" + ((int) keys[index]).ToString("X");
			lblKey.Text = names[index] + " key:";
			lblCurrent.Text = counter[index].ToString();
			lblTotal.Text = total.ToString();
			lmb.Text = mouse[0].ToString();
			mmb.Text = mouse[1].ToString();
			rmb.Text = mouse[2].ToString();
			xb1.Text = mouse[3].ToString();
			xb2.Text = mouse[4].ToString();
			for(int i = 0; i < 5; ++i) {
				topLabels[i].Item1.Text = names[top[i]] + " key";
				topLabels[i].Item2.Text = counter[top[i]].ToString();
			}
			label7.Text = GetGradientIndex(counter[index]).ToString();
		}
		private void EmergencyPressed() {
			kHook.Dispose();
			mHook.Dispose();
			OpenWindow();
			MessageBox.Show(this, "Keyboard and mouse has been unhooked\r\nKey presses will not be recorded.\r\nClick \"Rehook\" to enable again", "Emergency", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		private void KeyPressed(Keys key) {
			int index = Array.FindIndex(keys, k => k == key);
			if(index == -1) {
				//Error("Unknown key", "Unknown key pressed\r\n" + key);
				return;
			}
			++counter[index];
			++total;
			SaveCount(DefaultPath);
			CalculateTop(index);
			UpdateLabels();
		}
		private void MousePressed(MouseButtons key) {
			if(key == MouseButtons.Left)
				mouse[0]++;
			else if(key == MouseButtons.Middle)
				mouse[1]++;
			else if(key == MouseButtons.Right)
				mouse[2]++;
			UpdateLabels();
		}

		private void Key_Click(object sender, EventArgs e) {
			if(sender == kLAlt || sender == kRAlt)
				return;
			if(sender == kEnter)
				sender = kNEnter;
			lastClick = sender as Control;
			UpdateLabels();
		}

		private void bReset_Click(object sender, EventArgs e) {
			if(MessageBox.Show(this, "Are you sure you want to reset all counters?", "Confirm action", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
				counter = new long[keys.Length];
				mouse = new long[5];
				top = new int[] { 0, 1, 2, 3, 4 };
				SaveCount(DefaultPath);
				UpdateLabels();
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			if(e.CloseReason == CloseReason.UserClosing)
				e.Cancel = true;
			CloseWindow();
		}

		private void bExport_Click(object sender, EventArgs e) {
			if(saveFileDialog1.ShowDialog(this) == DialogResult.OK)
				Export(saveFileDialog1.FileName);
		}

		private void bImport_Click(object sender, EventArgs e) {
			if(openFileDialog1.ShowDialog(this) == DialogResult.OK)
				Import(saveFileDialog1.FileName);
		}

		private void button1_Click(object sender, EventArgs e) {
			try {
				kHook?.Dispose();
			} catch { }
			try {
				kHook = new KeyboardHook();
				kHook.KeyPressed += KeyPressed;
				kHook.Emergency += EmergencyPressed;
			} catch(Win32Exception we) {
				Error("Hook exception", "Windows rejected keyboard hook. Try clicking \"Rehook\"\r\n" + we.Message);
			}
			try {
				mHook?.Dispose();
			} catch { }
			try {
				mHook = new MouseHook();
				mHook.KeyPressed += MousePressed;
			} catch(Win32Exception we) {
				Error("Hook exception", "Windows rejected mouse hook. Try clicking \"Rehook\"\r\n" + we.Message);
			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
			SaveCount(DefaultPath);
			kHook.Dispose();
			mHook.Dispose();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			SaveCount(DefaultPath);
			kHook.Dispose();
			mHook.Dispose();
			Application.Exit();
		}

		private void notifyIcon1_MouseDown(object sender, MouseEventArgs e) {
			if(e.Button == MouseButtons.Left)
				OpenWindow();
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e) {
			RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if(checkBox1.Checked)
				key.SetValue(AppName, ExePath());
			else
				key.DeleteValue(AppName);
		}

		private void timer1_Tick(object sender, EventArgs e) {
			for(int i = 0; i < counter.Length; ++i) {
				elements[i].BackColor = gradient[GetGradientIndex(counter[i])];
				elements[i].Invalidate();
				if(keys[i] == Keys.Return) {
					kEnter.BackColor = gradient[GetGradientIndex(counter[i])];
					kEnter.Invalidate();
				}
			}
		}

		private void bFull_Click(object sender, EventArgs e) {
			bFull.Enabled = false;
			bFull.ForeColor = SystemColors.InactiveCaption;
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("<html><head><title>Keyboard Heatmap | {0}</title>", DateTime.Now.ToShortDateString());
			sb.AppendFormat("<style>{0}{1}{2}{3}{4}{5}{6}{7}</style>",
				".top{font-weight:bold}", "table{border-spacing:0;}",
				"td{padding:0 0.1vw;vertical-align:top;}", "table{font-size:2vw;width:100%;}",
				"#mouse{width:40%;position:absolute;top:8;left:34%;}",
				".inactive{color:lightgray}", ".mrow{background:linear-gradient(90deg,#0A0,white)}",
				"");
			sb.Append("</head><body><center><table><tr><td><table><tr><th>Key name</th><th colspan=\"2\">Count</th></tr>");
			List<Tuple<string, long, string>> list = new List<Tuple<string, long, string>>();
			for(int i = 0; i < keys.Length; ++i)
				list.Add(new Tuple<string, long, string>(names[i], counter[i],
					ColorTranslator.ToHtml(elements[i].BackColor)));
			list.Sort((a, b) => b.Item2.CompareTo(a.Item2));
			int c = 0;
			foreach(Tuple<string, long, string> t in list)
				sb.AppendFormat(
					"<tr style=\"background:linear-gradient(90deg,{4},white)\"{2}><td class=\"key-name\">{0}</td><td class=\"key-count\">{1}</td><td class=\"key-percent\">{3}</td></tr>",
					t.Item1, t.Item2, c++ < 5 ? " class=\"top\"" : "",
					(100f * t.Item2 / total).ToString("0.00") + "%", t.Item3);
			sb.Append("</table></td><td><table><tr><th>Key name</th><th colspan=\"2\">Count</th></tr>");
			long mttl = mouse[0] + mouse[1] + mouse[2] + mouse[3] + mouse[4];
			sb.AppendFormat("<tr class=\"mrow\"><td class=\"key-name\">{0}</td><td class=\"key-count\">{1}</td><td class=\"key-percent\">{2}</td><tr>", "Left mouse button",
				mouse[0], (100f * mouse[0] / mttl).ToString("0.00") + "%");
			sb.AppendFormat("<tr class=\"mrow\"><td class=\"key-name\">{0}</td><td class=\"key-count\">{1}</td><td class=\"key-percent\">{2}</td><tr>", "Right mouse button",
				mouse[2], (100f * mouse[2] / mttl).ToString("0.00") + "%");
			/*sb.AppendFormat("<tr class=\"mrow inactive\"><td class=\"key-name\">{0}</td><td class=\"key-count\">{1}</td><td class=\"key-percent\">{2}</td><tr>", "Middle mouse button",
				mouse[1], (100f * mouse[1] / mttl).ToString("0.00") + "%");
			sb.AppendFormat("<tr class=\"mrow inactive\"><td class=\"key-name\">{0}</td><td class=\"key-count\">{1}</td><td class=\"key-percent\">{2}</td><tr>", "XButton 1",
				mouse[3], (100f * mouse[3] / mttl).ToString("0.00") + "%");
			sb.AppendFormat("<tr class=\"mrow inactive\"><td class=\"key-name\">{0}</td><td class=\"key-count\">{1}</td><td class=\"key-percent\">{2}</td><tr>", "XButton 2",
				mouse[4], (100f * mouse[4] / mttl).ToString("0.00") + "%");*/
			sb.Append("</table><table style=\"margin-top:3em\">");
			sb.AppendFormat("<tr><th>Total presses:</th><td>{0}</td></tr>", total);
			sb.AppendFormat("<tr><th>Total clicks:</th><td>{0}</td></tr>", mttl);
			sb.Append("</table></td></tr></table></center></body></html>");
			File.WriteAllText("Keyboard Heatmap " + DateTime.Now.ToShortDateString() + ".html", sb.ToString());
			bFull.Enabled = true;
			bFull.ForeColor = SystemColors.ControlText;
		}
	}
}
