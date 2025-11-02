using Slipstream.Data;
using Slipstream.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Slipstream.Services
{
    class RivalsOcrEngine
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, nint lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern nint FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

        delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const double p1char_Xoffset = 0.2432;
        private const double p1char_Yoffset = 0.9190;
        private const double p1char_Xselection = 0.09258;
        private const double p1char_Yselection = 0.0270;

        private const double p1tag_Xoffset = 0.2442;
        private const double p1tag_Yoffset = 0.8928;
        private const double p1tag_Xselection = 0.09258;
        private const double p1tag_Yselection = 0.0300;

        private const double p1elo_Xoffset = 0.3617;
        private const double p1elo_Yoffset = 0.8716;
        private const double p1elo_Xselection = 0.0313;
        private const double p1elo_Yselection = 0.0285;

        private const double p2char_Xoffset = 0.6689;
        private const double p2char_Yoffset = 0.9200;
        private const double p2char_Xselection = 0.09258;
        private const double p2char_Yselection = 0.0270;

        private const double p2tag_Xoffset = 0.6689;
        private const double p2tag_Yoffset = 0.8928;
        private const double p2tag_Xselection = 0.09258;
        private const double p2tag_Yselection = 0.0300;

        private const double p2elo_Xoffset = 0.7871;
        private const double p2elo_Yoffset = 0.8716;
        private const double p2elo_Xselection = 0.0313;
        private const double p2elo_Yselection = 0.0285;

        private static Color charUpperThreshold = Color.FromArgb(0x75, 0x7B, 0xC3);
        private static Color tagUpperThreshold = Color.FromArgb(0xD0, 0xD0, 0xD0);

        private static OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en"));

        public static async Task<RivalsOcrResult> Capture()
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();

            nint hWndFound = 0;

#if DEBUG
            hWndFound = FindWindow(null, "Rivals2  ");
#endif

#if DEBUGPHOTO
            // DO NOT USE PHOTOS LEGACY -- The handler is stupid and doesn't work. Use new Photos
            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().Contains("All_Capture"))
                {
                    hWndFound = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
#endif

#if RELEASE
            hWndFound = FindWindow(null, "Rivals2  ");
#endif

            if (hWndFound == nint.Zero)
            {
                Console.WriteLine("Window not found.");
                return new RivalsOcrResult(false, "Failed to Find Window", false);
            }

            GetWindowRect(hWndFound, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // REMEMBER:  "Out Of Memory" in the bmp.Clone() areas means the Rectangle is out of bounds because GDI is a troll.
            Rectangle player1char_crop = new Rectangle(Convert.ToInt32(width * p1char_Xoffset), Convert.ToInt32(height * p1char_Yoffset), Convert.ToInt32(width * p1char_Xselection), Convert.ToInt32(height * p1char_Yselection));
            Rectangle player1tag_crop = new Rectangle(Convert.ToInt32(width * p1tag_Xoffset), Convert.ToInt32(height * p1tag_Yoffset), Convert.ToInt32(width * p1tag_Xselection), Convert.ToInt32(height * p1tag_Yselection));
            Rectangle player1elo_crop = new Rectangle(Convert.ToInt32(width * p1elo_Xoffset), Convert.ToInt32(height * p1elo_Yoffset), Convert.ToInt32(width * p1elo_Xselection), Convert.ToInt32(height * p1elo_Yselection));

            Rectangle player2char_crop = new Rectangle(Convert.ToInt32(width * p2char_Xoffset), Convert.ToInt32(height * p2char_Yoffset), Convert.ToInt32(width * p2char_Xselection), Convert.ToInt32(height * p2char_Yselection));
            Rectangle player2tag_crop = new Rectangle(Convert.ToInt32(width * p2tag_Xoffset), Convert.ToInt32(height * p2tag_Yoffset), Convert.ToInt32(width * p2tag_Xselection), Convert.ToInt32(height * p2tag_Yselection));
            Rectangle player2elo_crop = new Rectangle(Convert.ToInt32(width * p2elo_Xoffset), Convert.ToInt32(height * p2elo_Yoffset), Convert.ToInt32(width * p2elo_Xselection), Convert.ToInt32(height * p2elo_Yselection));

            using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics ga = Graphics.FromImage(bmp))
            {
                ga.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            string filename = $"Capture-{DateTime.Now:MM-dd-HH-mm-ss}.jpg";

            Bitmap player1char_bmp = bmp.Clone(player1char_crop, bmp.PixelFormat);
            Bitmap player1tag_bmp = bmp.Clone(player1tag_crop, bmp.PixelFormat);
            Bitmap player1elo_bmp = bmp.Clone(player1elo_crop, bmp.PixelFormat);

            Bitmap player2char_bmp = bmp.Clone(player2char_crop, bmp.PixelFormat);
            Bitmap player2tag_bmp = bmp.Clone(player2tag_crop, bmp.PixelFormat);
            Bitmap player2elo_bmp = bmp.Clone(player2elo_crop, bmp.PixelFormat);

            SoftwareBitmap p1charsbmp = await ConvertToSoftwareBitmap(player1char_bmp);
            SoftwareBitmap p2charsbmp = await ConvertToSoftwareBitmap(player2char_bmp);
            SoftwareBitmap p1tagbmp = await ConvertToSoftwareBitmap(player1tag_bmp);
            SoftwareBitmap p2tagbmp = await ConvertToSoftwareBitmap(player2tag_bmp);
            SoftwareBitmap p1elosbmp = await ConvertToSoftwareBitmap(player1elo_bmp);
            SoftwareBitmap p2elosbmp = await ConvertToSoftwareBitmap(player2elo_bmp);

            OcrResult player1elo_text = await ocrEngine.RecognizeAsync(p1elosbmp);
            OcrResult player2elo_text = await ocrEngine.RecognizeAsync(p2elosbmp);

            Bitmap p1TagBmp_HighContrast = ApplyBrightnessColorThreshold(ScaleImage(player1tag_bmp), tagUpperThreshold);
            SoftwareBitmap p1TagsBmp_HighContrast = await ConvertToSoftwareBitmap(ScaleImage(p1TagBmp_HighContrast));
            OcrResult player1tag_text = await ocrEngine.RecognizeAsync(p1TagsBmp_HighContrast);

            Bitmap p2TagBmp_HighContrast = ApplyBrightnessColorThreshold(ScaleImage(player2tag_bmp), tagUpperThreshold);
            SoftwareBitmap p2TagsBmp_HighContrast = await ConvertToSoftwareBitmap(ScaleImage(p2TagBmp_HighContrast));
            OcrResult player2tag_text = await ocrEngine.RecognizeAsync(p2TagsBmp_HighContrast);

            Bitmap p1CharBmp_HighContrast = ApplyBlueColorThreshold(ScaleImage(player1char_bmp), charUpperThreshold);
            SoftwareBitmap p1CharsBmp_HighContrast = await ConvertToSoftwareBitmap(ScaleImage(p1CharBmp_HighContrast));
            OcrResult player1char_text = await ocrEngine.RecognizeAsync(p1CharsBmp_HighContrast);

            Bitmap p2CharBmp_HighContrast = ApplyBlueColorThreshold(ScaleImage(player2char_bmp), charUpperThreshold);
            SoftwareBitmap p2CharsBmp_HighContrast = await ConvertToSoftwareBitmap(ScaleImage(p2CharBmp_HighContrast));
            OcrResult player2char_text = await ocrEngine.RecognizeAsync(p2CharsBmp_HighContrast);

            if (GlobalData.IsSaveCaptures)
            {
                string subDirectory = "OCRCaptures";
                Directory.CreateDirectory(subDirectory);

                bmp.Save(Path.Combine(subDirectory, $"All_Capture_{DateTime.Now:MM-dd-HH-mm-ss}.jpg"), ImageFormat.Jpeg);
                player1char_bmp.Save(Path.Combine(subDirectory, $"p1_char_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                player2char_bmp.Save(Path.Combine(subDirectory, $"p2_char_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                player1elo_bmp.Save(Path.Combine(subDirectory, $"p1_elo_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                player2elo_bmp.Save(Path.Combine(subDirectory, $"p2_elo_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                p1TagBmp_HighContrast.Save(Path.Combine(subDirectory, $"p1_tag_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                p2TagBmp_HighContrast.Save(Path.Combine(subDirectory, $"p2_tag_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                p1CharBmp_HighContrast.Save(Path.Combine(subDirectory, $"p1_char_hc_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
                p2CharBmp_HighContrast.Save(Path.Combine(subDirectory, $"p2_char_hc_{Path.GetFileName(filename)}"), ImageFormat.Jpeg);
            }

            RivalsPlayer player1 = new RivalsPlayer(player1char_text.Text, player1tag_text.Text, player1elo_text.Text);
            RivalsPlayer player2 = new RivalsPlayer(player2char_text.Text, player2tag_text.Text, player2elo_text.Text);

            RivalsMatch match = new RivalsMatch(player1, player2);

            sw.Stop();
          
            string finaltext = $"Player 1: {player1char_text.Text}~~ Elo: {player1elo_text.Text} \n\n Player 2: {player2char_text.Text}~~ Elo: {player2elo_text.Text} \n\n perf: {sw.ElapsedMilliseconds}";
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

        private static Bitmap EnhanceContrast(Bitmap bmp, float contrast, float brightness)
        {
            Bitmap adjustedImage = new Bitmap(bmp.Width, bmp.Height);

            using (Graphics g = Graphics.FromImage(adjustedImage))
            {
                float adjustedContrast = contrast / 100.0f;
                float translation = 0.5f * (1.0f - adjustedContrast);

                // Create a color matrix to adjust contrast and brightness
                float[][] colorMatrixElements = {
                    new float[] { adjustedContrast, 0, 0, 0, 0 },
                    new float[] { 0, adjustedContrast, 0, 0, 0 },
                    new float[] { 0, 0, adjustedContrast, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { translation + brightness, translation + brightness, translation + brightness, 0, 1 }
                };

                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                // Draw the adjusted image
                g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
            }

            return adjustedImage;
        }

        private static Bitmap ApplyBlueColorThreshold(Bitmap bmp, Color upperThreshold)
        {
            Bitmap thresholdedImage = new Bitmap(bmp.Width, bmp.Height);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);

                    if (pixelColor.B >= upperThreshold.B)
                    {
                        thresholdedImage.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        thresholdedImage.SetPixel(x, y, Color.Black);
                    }
                }
            }

            return thresholdedImage;
        }

        private static Bitmap ApplyBrightnessColorThreshold(Bitmap bmp, Color upperThreshold)
        {
            Bitmap thresholdedImage = new Bitmap(bmp.Width, bmp.Height);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);

                    if (pixelColor.B >= upperThreshold.B && pixelColor.R >= upperThreshold.R && pixelColor.G >= upperThreshold.G)
                    {
                        thresholdedImage.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        thresholdedImage.SetPixel(x, y, Color.Black);
                    }
                }
            }

            return thresholdedImage;
        }

        // This ended up being the key to making this system work - the capture needs to be a little bigger than it is by default.
        private static Bitmap ScaleImage(Bitmap bmp, int scaleFactor = 2)
        {
            int newWidth = bmp.Width * scaleFactor;
            int newHeight = bmp.Height * scaleFactor;
            Bitmap scaledImage = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(scaledImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, 0, 0, newWidth, newHeight);
            }

            return scaledImage;
        }
    }
}
