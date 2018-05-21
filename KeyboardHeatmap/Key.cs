using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardHeatmap {
	public partial class Key:UserControl {
		public string KeyName {
			get => label1.Text;
			set => label1.Text = value;
		}
		public float FontSize {
			get => label1.Font.Size;
			set {
				Font old = label1.Font;
				label1.Font = new Font(old.FontFamily, value, FontStyle.Bold);
				old.Dispose();
			}
		}
		public bool IsActive {
			get => label1.ForeColor == Color.Black;
			set => label1.ForeColor = value ? Color.Black : SystemColors.GrayText;
		}
		public Key() {
			InitializeComponent();
		}

		private void label1_Click(object sender, EventArgs e) {
			InvokeOnClick(this, e);
		}
	}
}
