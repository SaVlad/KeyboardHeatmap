using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardHeatmap {
	public partial class EnterKey:UserControl {
		private Brush bgb = new SolidBrush(Color.White);
		public EnterKey() {
			InitializeComponent();
		}
		protected override void OnBackColorChanged(EventArgs e) {
			bgb.Dispose();
			bgb = new SolidBrush(BackColor);
		}
		protected override void OnPaintBackground(PaintEventArgs e) {
			e.Graphics.Clear(SystemColors.Control);
			e.Graphics.FillRectangles(bgb, new[]{
				new Rectangle(30, 0, 54, 66),
				new Rectangle(0, 36, 84, 30)
			});
			e.Graphics.DrawLines(SystemPens.WindowFrame, new[] {
				new Point(30, 0),
				new Point(83, 0),
				new Point(83, 65),
				new Point(0, 65),
				new Point(0, 36),
				new Point(30, 36),
				new Point(30, 0)
			});
		}

		private void label1_Click(object sender, EventArgs e) {
			InvokeOnClick(this, e);
		}
	}
}
