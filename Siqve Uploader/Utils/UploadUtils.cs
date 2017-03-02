using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Siqve_Uploader.Utils {
	class UploadUtils {

		private static UploadUtils instance;
		private UploadUtils() { }

		public static UploadUtils Instance {
			get {
				if (instance == null) {
					instance = new UploadUtils();
				}
				return instance;
			}
		}

		public void UploadFile(SftpClient client, string filepath, string name, UploadType type) {
			using (FileStream fs = new FileStream(filepath, FileMode.Open)) {

				var result = client.BeginUploadFile(fs, name) as SftpUploadAsyncResult;
				Main form = Main._myInstance;

				double fsLength = Convert.ToDouble(fs.Length);
				long sizeLimit = Properties.Settings.Default.SizeLimit;
				if (sizeLimit != 0 && fs.Length > sizeLimit) 
					return;

				int lastStage = -1;
				while (!result.IsCompleted) {
					Thread.Sleep(100);
					double progress = (double)(Convert.ToDouble(result.UploadedBytes) / fsLength) * 100.0;
					form.setIconText("Uploading: \n" + Math.Round(progress) + "% - ("
						+ Math.Round((result.UploadedBytes / 1000000.0), 2) + "MB/" + Math.Round((fsLength / 1000000.0), 2) + "MB)");
					int rotationStage = (int)(progress / 25);
					if (rotationStage != lastStage) {
						lastStage = rotationStage;
						form.setIconRotation((int)(progress / 25));
					}

				}
				string url = "http://" + type.getUrlPrefix() + "." + Properties.Settings.Default.Domain + "/" + name;
				System.Windows.Forms.Clipboard.SetText(url);

				System.Media.SoundPlayer player = new System.Media.SoundPlayer(Siqve_Uploader.Properties.Resources.decay);
				player.Play();
				form.addLine(url);
				form.setIcon();
			}
		}

		public bool checkFileExists(SftpClient client, string fileName, UploadType type) {
			try {
				return client.Exists(fileName);
			}
			catch (Exception ex) {
				return false;
			}
		}

		public sealed class UploadType {

			private readonly String destination;
			private readonly String urlPrefix;

			public static readonly UploadType FILE = new UploadType("file", "f");
			public static readonly UploadType IMAGE = new UploadType("image", "i");

			private UploadType(String destination, String urlPrefix) {
				this.destination = destination;
				this.urlPrefix = urlPrefix;
			}

			public String getDestination() {
				return @"siqveuploader/" + destination;
			}
			public String getUrlPrefix() {
				return urlPrefix;
			}

		}

	}

}
