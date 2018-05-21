namespace KeyboardHeatmap {
	partial class EnterKey {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Bold);
			this.label1.Location = new System.Drawing.Point(0, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 30);
			this.label1.TabIndex = 0;
			this.label1.Text = "↵";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// EnterKey
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.label1);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Name = "EnterKey";
			this.Size = new System.Drawing.Size(84, 66);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
	}
}
