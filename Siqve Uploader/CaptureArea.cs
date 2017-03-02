using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;


namespace Siqve_Uploader {
	public partial class CaptureArea : Form {

		Label sizeLabel = new DoubleBufferedLabel();
		private bool drawing = false;
		private Point currentPos;
		private Point startPos;
		GlobalKeyboardHook gHook;


		[DllImport("User32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool Repaint);

		private void resize(System.Object sender, System.EventArgs e) {
			Size size = ScreenUtils.getTotalScreenSize();
			this.MaximumSize = size;
			Point point = ScreenUtils.getLeftTopMostPoint();
			bool Result = MoveWindow(this.Handle, point.X, point.Y, size.Width, size.Height, true);
		}


		public CaptureArea() {
			Load += resize;
			InitializeComponent();
			this.Cursor = Cursors.Cross;
			this.Size = ScreenUtils.getTotalScreenSize();
			this.FormBorderStyle = FormBorderStyle.None;
			this.TopMost = true;
			this.BackColor = Color.White;
			this.TransparencyKey = Color.Lime;
			this.Opacity = 0.1;
			this.DoubleBuffered = true;
			if (Screen.AllScreens.Length == 1) {
				this.WindowState = FormWindowState.Maximized;
			}
			gHook = new GlobalKeyboardHook();
			gHook.KeyDown += new KeyEventHandler(gHook_KeyDown);
			gHook.HookedKeys.Add(Keys.Escape);
			gHook.HookedKeys.Add(Keys.Enter);
			sizeLabel.Text = "(0,0)";
			this.Controls.Add(sizeLabel);
		}

		public void gHook_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) {
				this.drawing = false;
				updateSizeLabel();
				this.Invalidate();
				this.Close();
			}
			else if (e.KeyCode == Keys.Enter && drawing) {
				save();
			}
		}

		private void save() {
			drawing = false;
			updateSizeLabel();
			this.Invalidate();
			this.Close();
			Rectangle rect = getRectangle();
			if (rect == null || rect.Size == new Size(0, 0)) {
				return;
			}
			Point point = ScreenUtils.getLeftTopMostPoint();
			Point location = new Point(rect.Location.X + point.X, rect.Location.Y + point.Y);
			FileUtils.UploadImage(false, Size.Empty, Point.Empty, location, new Point(location.X + rect.Width, location.Y + rect.Height), ".png");
		}


		private Rectangle getRectangle() {
			return new Rectangle(
				Math.Min(startPos.X, currentPos.X),
				Math.Min(startPos.Y, currentPos.Y),
				Math.Abs(startPos.X - currentPos.X),
				Math.Abs(startPos.Y - currentPos.Y));
		}


		private void mouseDown(object sender, MouseEventArgs e) {
			currentPos = startPos = e.Location;
			this.Opacity = 0.4;
			updateSizeLabel();
			drawing = true;
		}

		private void mouseMove(object sender, MouseEventArgs e) {
			currentPos = e.Location;
			updateSizeLabel();
			this.Invalidate();
		}

		private void mouseUp(object sender, MouseEventArgs e) {
			drawing = false;
			updateSizeLabel();
			this.Invalidate();
			save();
		}

		private void formPaint(object sender, PaintEventArgs e) {
			if (drawing) {
				Rectangle rect = getRectangle();
				e.Graphics.FillRectangle(Brushes.Lime, rect);
				e.Graphics.DrawRectangle(Pens.Black, rect);
			}
		}

		private void updateSizeLabel() {
			sizeLabel.Visible = drawing;
			if (currentPos.X < startPos.X && currentPos.Y < startPos.Y)
				sizeLabel.Location = new Point(currentPos.X + 5, currentPos.Y - 23);
			else
				sizeLabel.Location = new Point(currentPos.X + 5, currentPos.Y + 5);
			Rectangle rect = getRectangle();
			sizeLabel.Text = "(" + rect.Width + "," + rect.Height + ")";
		}
	}
}
