using System;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Drawing;
using System.IO;
using Windows.Media.Ocr;
using System.Diagnostics;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Globalization;
using System.Text;
using Rivals2Tracker.Data;

namespace Rivals2Tracker.Models
{
    class RivalsOcrEngine
    {
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const double p1c_Xoffset = 0.245;
        private const double p1c_Yoffset = 0.9028;
        private const double p1c_Xselection = 0.09258;
        private const double p1c_Yselection = 0.0583;

        private const double p1e_Xoffset = 0.3617;
        private const double p1e_Yoffset = 0.8896;
        private const double p1e_Xselection = 0.0313;
        private const double p1e_Yselection = 0.0285;

        private const double p2c_Xoffset = 0.6699;
        private const double p2c_Yoffset = 0.9028;
        private const double p2c_Xselection = 0.09258;
        private const double p2c_Yselection = 0.0604;

        private const double p2e_Xoffset = 0.7871;
        private const double p2e_Yoffset = 0.8896;
        private const double p2e_Xselection = 0.3477;
        private const double p2e_Yselection = 0.0285;


        private static OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en"));

        public static async Task<RivalsOcrResult> Capture()
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();

            // Old Hard-set system
            //Rectangle player1Crop = new Rectangle(625, 1300, 237, 84);
            //Rectangle player1EloCrop = new Rectangle(926, 1281, 80, 41);
            //Rectangle player2Crop = new Rectangle(1715, 1300, 237, 84);
            //Rectangle player2EloCrop = new Rectangle(2015, 1281, 80, 41);

            nint hWnd = FindWindow(null, "Rivals2  ");
            // nint hWnd = FindWindow(null, "Photos Legacy");
            // nint hWnd = FindWindow(null, "CaptureAll-09-01-19-56-00.jpg");
            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("Window not found.");
                return new RivalsOcrResult(false, "Failed to Find Window", false);
            }

            GetWindowRect(hWnd, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Rectangle player1Crop = new Rectangle(Convert.ToInt32(width * p1c_Xoffset), Convert.ToInt32(height * p1c_Yoffset), Convert.ToInt32(width * p1c_Xselection), Convert.ToInt32(height * p1c_Yselection));
            Rectangle player1EloCrop = new Rectangle(Convert.ToInt32(width * p1e_Xoffset), Convert.ToInt32(height * p1e_Yoffset), Convert.ToInt32(width * p1e_Xselection), Convert.ToInt32(height * p1e_Yselection));
            Rectangle player2Crop = new Rectangle(Convert.ToInt32(width * p2c_Xoffset), Convert.ToInt32(height * p2c_Yoffset), Convert.ToInt32(width * p2c_Xselection), Convert.ToInt32(height * p2c_Yselection));
            Rectangle player2EloCrop = new Rectangle(Convert.ToInt32(width * p2e_Xoffset), Convert.ToInt32(height * p2e_Yoffset), Convert.ToInt32(width * p2e_Xselection), Convert.ToInt32(height * p2e_Yselection));

            using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics ga = Graphics.FromImage(bmp))
            {
                ga.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            string filename = $"CaptureAll-{DateTime.Now:MM-dd-HH-mm-ss}.jpg";

            Bitmap player1bmp = bmp.Clone(player1Crop, bmp.PixelFormat);
            Bitmap player2bmp = bmp.Clone(player2Crop, bmp.PixelFormat);
            Bitmap player1elobmp = bmp.Clone(player1EloCrop, bmp.PixelFormat);
            Bitmap player2elobmp = bmp.Clone(player2EloCrop, bmp.PixelFormat);

            if (GlobalData.SaveCaptures)
            {
                bmp.Save(filename, ImageFormat.Jpeg);
                player1bmp.Save($"p1-{filename}", ImageFormat.Jpeg);
                player2bmp.Save($"p2-{filename}", ImageFormat.Jpeg);
                player1elobmp.Save($"p1elo-{filename}", ImageFormat.Jpeg);
                player2elobmp.Save($"p2elo-{filename}", ImageFormat.Jpeg);
            }

            SoftwareBitmap p1sbmp = await ConvertToSoftwareBitmap(player1bmp);
            SoftwareBitmap p2sbmp = await ConvertToSoftwareBitmap(player2bmp);
            SoftwareBitmap p1elosbmp = await ConvertToSoftwareBitmap(player1elobmp);
            SoftwareBitmap p2elosbmp = await ConvertToSoftwareBitmap(player2elobmp);

            OcrResult player1text = await ocrEngine.RecognizeAsync(p1sbmp);
            OcrResult player2text = await ocrEngine.RecognizeAsync(p2sbmp);
            OcrResult player1elotext = await ocrEngine.RecognizeAsync(p1elosbmp);
            OcrResult player2elotext = await ocrEngine.RecognizeAsync(p2elosbmp);

            RivalsPlayer player1 = new RivalsPlayer(player1text.Text, player1elotext.Text);
            RivalsPlayer player2 = new RivalsPlayer(player2text.Text, player2elotext.Text);

            RivalsMatch match = new RivalsMatch(player1, player2);

            sw.Stop();
          
            string finaltext = $"Player 1: {player1text.Text}~~ Elo: {player1elotext.Text} \n\n Player 2: {player2text.Text}~~ Elo: {player2elotext.Text} \n\n perf: {sw.ElapsedMilliseconds}";
            Console.WriteLine(finaltext);

            if (!match.IsValid(out MatchValidityFlag validityFlag))
            {
                return new RivalsOcrResult(match, validityFlag);
            }
            else
            {
                return new RivalsOcrResult(match);
            }
        }

        static async Task<SoftwareBitmap> ConvertToSoftwareBitmap(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;

            using IRandomAccessStream stream = ms.AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(stream);
            return await decoder.GetSoftwareBitmapAsync();
        }
    }
}
