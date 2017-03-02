using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Specialized;
using Siqve_Uploader.Utils;
using Renci.SshNet;

namespace Siqve_Uploader {
	class FileUtils {

		private static ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
		private static EncoderParameters myEncoderParameters = new EncoderParameters(1);

		static FileUtils() {
			myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
		}


		private static string host = Properties.Settings.Default.Host;
		private static string username = Properties.Settings.Default.Username;
		private static string password = Properties.Settings.Default.Password;
		private static int port = Properties.Settings.Default.SFTPPort;

		public static async void UploadImage(bool showCursor, Size curSize, Point curPos, Point TopLeft,
			Point BottomRight, string extension) {
			int longRunningTask = await LongRunningOperationAsync();

			Size pictureSize = new Size(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y);
			using (Bitmap bitmap = new Bitmap(pictureSize.Width, pictureSize.Height)) {
				using (Graphics g = Graphics.FromImage(bitmap)) {
					g.CopyFromScreen(TopLeft, Point.Empty, pictureSize);

					if (showCursor) {
						Rectangle cursorBounds = new Rectangle(curPos, curSize);
						Cursors.Default.Draw(g, cursorBounds);
					}
				}

				MemoryStream ms = new MemoryStream();
				switch (extension) {
					case ".bmp":
						bitmap.Save(ms, ImageFormat.Bmp);
						break;
					case ".jpg":
						bitmap.Save(ms, jpgEncoder, myEncoderParameters);
						break;
					case ".gif":
						bitmap.Save(ms, ImageFormat.Gif);
						break;
					case ".tiff":
						bitmap.Save(ms, ImageFormat.Tiff);
						break;
					case ".png":
						bitmap.Save(ms, ImageFormat.Png);
						break;
					default:
						bitmap.Save(ms, ImageFormat.Jpeg);
						break;
				}
				bitmap.Dispose();
				using (SftpClient client = new SftpClient(host, port, username, password)) {
					UploadUtils.UploadType type = UploadUtils.UploadType.IMAGE;
					client.Connect();
					client.ChangeDirectory(type.getDestination());
					client.BufferSize = 2 * 1024;

					string name = NameUtils.RandomString(8) + extension;
					while (UploadUtils.Instance.checkFileExists(client, name, type)) {
						name = NameUtils.RandomString(8) + extension;
					}
					using (var tempPath = new TempFile(@System.IO.Path.GetTempPath() + name)) {
						using (var fileStream = new FileStream(tempPath.Path, FileMode.Create)) {
							ms.Seek(0, SeekOrigin.Begin);
							ms.CopyTo(fileStream);
						}
						UploadUtils.Instance.UploadFile(client, tempPath.Path, name, type);
					}
				}
			}
		}

		public static async void UploadFiles(StringCollection collection) {
			int longRunningTask = await LongRunningOperationAsync();
			using (SftpClient client = new SftpClient(host, port, username, password)) {
				UploadUtils.UploadType type = UploadUtils.UploadType.FILE;
				client.Connect();
				client.ChangeDirectory(type.getDestination());
				client.BufferSize = 2 * 1024;

				string name = NameUtils.RandomString(8) + ".zip";
				while (UploadUtils.Instance.checkFileExists(client, name, type)) {
					name = NameUtils.RandomString(8) + ".zip";
				}

				using (var tempPath = new TempFile(@System.IO.Path.GetTempPath() + name)) {
					using (var ms = new MemoryStream()) {
						using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true)) {
							for (int i = 0; i < collection.Count; i++) {
								archive.CreateEntryFromFile(collection[i], Path.GetFileName(collection[i]));
							}
						}

						using (var fileStream = new FileStream(tempPath.Path, FileMode.Create)) {
							ms.Seek(0, SeekOrigin.Begin);
							ms.CopyTo(fileStream);
						}

						UploadUtils.Instance.UploadFile(client, tempPath.Path, name, type);
					}
				}
			}
		}

		public static async void UploadFile(string path) {
			int longRunningTask = await LongRunningOperationAsync();
			string filePath = path;
			using (SftpClient client = new SftpClient(host, port, username, password)) {
				UploadUtils.UploadType type = UploadUtils.UploadType.FILE;
				client.Connect();
				client.ChangeDirectory(type.getDestination());
				client.BufferSize = 2 * 1024;

				while (UploadUtils.Instance.checkFileExists(client, Path.GetFileName(path), type)) {
					path = path.Replace(Path.GetFileName(path), NameUtils.RandomString(8) + Path.GetExtension(path));
				}

				UploadUtils.Instance.UploadFile(client, filePath, Path.GetFileName(path), type);
			}
		}

		private static ImageCodecInfo GetEncoder(ImageFormat format) {
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs) {
				if (codec.FormatID == format.Guid) {
					return codec;
				}
			}
			return null;
		}

		public static async Task<int> LongRunningOperationAsync() {
			await Task.Delay(100);
			return 1;
		}

	}
}
