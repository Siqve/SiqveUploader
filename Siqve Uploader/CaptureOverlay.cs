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
	public partial class CaptureOverlay : Form {


		[DllImport("User32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool Repaint);

		private static readonly Color transparencyColor = Color.Lime;
		private static readonly Brush transparencyBrush = Brushes.Lime;

		private Label sizeLabel = new DoubleBufferedLabel();
		private bool drawing = false;
		private Point currentPos;
		private Point startPos;
		private GlobalKeyboardHook gHook;

		public CaptureOverlay() {
			Load += resize;
			InitializeComponent();
			this.Cursor = Cursors.Cross;
			this.FormBorderStyle = FormBorderStyle.None;
			this.TopMost = true;
			this.BackColor = Color.White;
			this.TransparencyKey = transparencyColor;
			this.Opacity = 0.1;
			this.DoubleBuffered = true;

			//If only one screen then Maximized state is sufficient
			if (Screen.AllScreens.Length == 1)
				this.WindowState = FormWindowState.Maximized;

			hookKeys();
			sizeLabel.Text = "(0,0)";
			this.Controls.Add(sizeLabel);
		}
		private void resize(System.Object sender, System.EventArgs e) {
			Size size = ScreenUtils.getTotalScreenSize();
			Point point = ScreenUtils.getLeftTopMostPoint();
			this.MaximumSize = size;
			bool Result = MoveWindow(this.Handle, point.X, point.Y, size.Width, size.Height, true);
		}

		private void hookKeys() {
			gHook = new GlobalKeyboardHook();
			gHook.KeyDown += new KeyEventHandler(gHook_KeyDown);
			gHook.HookedKeys.Add(Keys.Escape);
			gHook.HookedKeys.Add(Keys.Enter);
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
			this.drawing = false;
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
				e.Graphics.FillRectangle(transparencyBrush, rect);
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
