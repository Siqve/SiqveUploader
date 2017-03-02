using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Siqve_Uploader {
	class ScreenUtils {

		public static Size getTotalScreenSize() {
			int width = 0;
			int height = 0;
			int yOffset = Math.Abs(Screen.AllScreens.Min(s => s.WorkingArea.Y)) + Math.Abs(Screen.AllScreens.Max(s => s.WorkingArea.Y));
			foreach (Screen screen in Screen.AllScreens) {
				if (screen.Bounds.Height > height)
					height = screen.Bounds.Height;
				width += Screen.PrimaryScreen.Bounds.Width;
			}
			return new Size(width, height + yOffset);
		}

		public static Point getLeftTopMostPoint() {
			return new Point(Screen.AllScreens.Min(s => s.Bounds.Left), Screen.AllScreens.Min(s => s.Bounds.Top));
		}

	}
}
