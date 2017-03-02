using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Siqve_Uploader {
	public class DoubleBufferedLabel : Label {
		public DoubleBufferedLabel() {
			this.DoubleBuffered = true;
			this.Font = new Font(this.Font, FontStyle.Bold);
		}
	}
}

