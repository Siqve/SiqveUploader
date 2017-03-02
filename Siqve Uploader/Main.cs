using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Specialized;
using Siqve_Uploader.Utils;
using System.Configuration;

namespace Siqve_Uploader {
	public partial class Main : Form {

		public static Main _myInstance;

		private NotifyIcon trayIcon;


		GlobalKeyboardHook gHook;
		Keys gkeyScreen = Properties.Settings.Default.ScreenShortcut;
		Keys gkeyArea = Properties.Settings.Default.AreaShortcut;
		Keys gkeyFile = Properties.Settings.Default.FileShortcut;

		public Main() {
			_myInstance = this;
			InitializeComponent();
			setIcon();

			gHook = new GlobalKeyboardHook();
			gHook.KeyDown += new KeyEventHandler(gHook_KeyDown);
			gHook.HookedKeys.Add(gkeyScreen);
			gHook.HookedKeys.Add(gkeyArea);
			gHook.HookedKeys.Add(gkeyFile);
		}

		private bool allowClose;

		public void gHook_KeyDown(object sender, KeyEventArgs e) {
			if (ModifierKeys.HasFlag(Keys.Shift) && ModifierKeys.HasFlag(Keys.Alt)) {
				if (e.KeyCode == gkeyScreen) {
					e.Handled = true;
					captureScreen(null, null);

				}
				else if (e.KeyCode == gkeyArea) {
					captureArea(null, null);
					e.Handled = true;
				}
				else if (e.KeyCode == gkeyFile) {
					uploadClipboard(null, null);
					e.Handled = true;
				}
			}
		}

		List<String> recent = new List<String>();

		public void setIcon() {
			Bitmap icon = Siqve_Uploader.Properties.Resources.tray_icon;
			icon.MakeTransparent(Color.White);
			Icon ico = Icon.FromHandle(icon.GetHicon());
			icon.Dispose();

			if (recent.Count > 0)
				trayIcon.Visible = false;
			trayIcon = new NotifyIcon() {
				Icon = ico,
				ContextMenu = getContextMenu(),
				Visible = true
			};
		}

		public void setIconRotation(int rotation) {
			Bitmap icon = Siqve_Uploader.Properties.Resources.tray_icon;
			rotateInternalFunction(icon, rotation);
			icon.MakeTransparent(Color.White);
			Icon ico = Icon.FromHandle(icon.GetHicon());
			icon.Dispose();

			trayIcon.Visible = false;
			trayIcon = new NotifyIcon() {
				Icon = ico,
				ContextMenu = trayIcon.ContextMenu,
				Visible = true,
			};
		}

		public void setIconText(string text) {
			trayIcon.Text = text;
		}

		public ContextMenu getContextMenu() {
			if (!recent.Any()) {
				return new ContextMenu(new MenuItem[] {
					new MenuItem("Upload Clipboard", uploadClipboard),
					new MenuItem("Capture Area", captureArea),
					new MenuItem("Capture Screen", captureScreen),
					new MenuItem("Exit", Exit)
				});
			}
			int top = 5 + recent.Count;
			MenuItem[] list = new MenuItem[top];
			int index = 0;
			foreach (string url in recent) {
				MenuItem item = new MenuItem((recent.Count - index) + ". " + url.Split(new string[] { Properties.Settings.Default.DomainName + "/" }, StringSplitOptions.None)[1]);
				item.Click += (sender, e) => copyUrl(url);
				list[recent.Count - index - 1] = item;
				index++;
			}

			list[top - 5] = new MenuItem(" ");
			list[top - 4] = new MenuItem("Upload Clipboard", uploadClipboard);
			list[top - 3] = new MenuItem("Capture Area", captureArea);
			list[top - 2] = new MenuItem("Capture Screen", captureScreen);
			list[top - 1] = new MenuItem("Exit", Exit);
			return new ContextMenu(list);
		}

		public void addLine(string line) {
			if (recent.Count == 5) {
				recent.RemoveAt(0);
			}
			recent.Add(line);
		}

		void copyUrl(string url) {
			System.Windows.Forms.Clipboard.SetText(url);
		}

		void uploadClipboard(object sender, EventArgs e) {
			try {
				StringCollection collection = Clipboard.GetFileDropList();
				if (collection.Count > 1) {
					FileUtils.UploadFiles(collection);
				}
				else {
					FileUtils.UploadFile(collection[0]);
				}
			}
			catch (Exception ex) {
				//TODO: Check if clipboard is image then upload as image, if not then upload as hastebin
			}
		}

		void captureArea(object sender, EventArgs e) {
			CaptureArea area = new CaptureArea();
			area.Show();
		}


		void captureScreen(object sender, EventArgs e) {
			Size size = ScreenUtils.getTotalScreenSize();
			Point point = ScreenUtils.getLeftTopMostPoint();
			FileUtils.UploadImage(false, Size.Empty, Point.Empty,
				point, new Point(size.Width + point.X, size.Height + point.Y), Properties.Settings.Default.PictureFormat);
		}

		void Exit(object sender, EventArgs e) {
			trayIcon.Visible = false;
			allowClose = true;
			Application.Exit();
		}

		protected override void SetVisibleCore(bool value) {
			base.SetVisibleCore(false);
		}

		protected override void OnFormClosing(FormClosingEventArgs e) {
			if (!allowClose) {
				this.Hide();
				e.Cancel = true;
			}
			base.OnFormClosing(e);
		}

		public void rotateInternalFunction(Bitmap bitmap, int stage) {
			switch (stage) {
				case 0:
					bitmap.RotateFlip(RotateFlipType.RotateNoneFlipNone);
					break;
				case 1:
					bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
					break;
				case 2:
					bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
					break;
				case 3:
					bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
					break;
			}
		}

	}

}
