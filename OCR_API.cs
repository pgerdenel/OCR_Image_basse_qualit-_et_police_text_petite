using IronOcr;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

/* Class permettant de reconnaitre le texte d'une image 
 * prend en charge les images de basse qualité
 * prend en charge les polices très petite
 * 1. redimensionne l'image de 250%
 * 2. applique un flou gaussien sur l'image
 * 3. aplique un filtre gris sur l'image
 * 4. applique un filtre noir et blanc sur l'image
 * 5. extrait le text de l'image 
 * en 2500 secondes soit 2.5sec
 * Utilisation :
 * OCR_API o = new OCR_API(@"C:\temp\test.jpg");
 * Debug.WriteLine(o.iron_ocr_perf());
 */

namespace iron_ocr2
{
    class OCR_API
    {
        private string filename;
        private string text_retrieved;

        public OCR_API(string filename)
        {
            this.filename = filename;
        }

        public string iron_ocr_perf()
        {
            Bitmap b = (Bitmap)Image.FromFile(file);

            // on redimensionne l'image
            Bitmap b_redim = ResizeImage(b, b.Width + ((b.Width * 250) / 100), b.Height + ((b.Height * 250) / 100));

            // on svg l'image redimensionnée
            // Bitmap TempBmp = (Bitmap)b_redim.Clone();
            // TempBmp.Save(file, ImageFormat.Jpeg);

            // on applique un flou gaussien
            ApplyGaussianBlur(ref b_redim, 4);

            // on svg l'image après flou gaussien
            // Bitmap TempBmp2 = (Bitmap)b_redim.Clone();
            // TempBmp.Save(file2, ImageFormat.Jpeg);

            // on applique un filtre gris
            Bitmap b_gray = ToGrayScale(b_redim);

            // on sauvegarde l'image après filtre gris gris
            // Bitmap TempBmp3 = (Bitmap)b_gray.Clone();
            // TempBmp3.Save(file3, ImageFormat.Jpeg);

            // on applique un filtre noir et blanc sur l'image
            Bitmap b_black = ToBlackAndwhite(b_gray);

            // on sauvegarde l'image filtre noir et blanc sur l'image
            // Bitmap TempBmp4 = (Bitmap)b_black.Clone();
            // TempBmp4.Save(file_final, ImageFormat.Jpeg);

            // on extrait le texte de l'image traité
            return new AutoOcr().Read(b_black).Text;
        }

        // redimensionne une image
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // applique un flou gaussien sur l'image
        public static void ApplyGaussianBlur(ref Bitmap bmp, int Weight)
        {
            ConvolutionMatrix m = new ConvolutionMatrix();
            m.Apply(1);
            m.Pixel = Weight;
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 2;
            m.Factor = Weight + 12;

            Convolution C = new Convolution();
            C.Matrix = m;
            C.Convolution3x3(ref bmp);
        }

        // applique un filtre noir et blanc sur l'image
        public Bitmap ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            Bitmap TempBmp = (Bitmap)Bmp.Clone();

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = TempBmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    TempBmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }

            return TempBmp;
        }

        // on applique un filtre noir et blanc sur l'image
        public Bitmap ToBlackAndwhite(Bitmap bmp)
        {
            Bitmap TempBmp = (Bitmap)bmp.Clone();

            using (Graphics gr = Graphics.FromImage(TempBmp))
            {
                var gray_matrix = new float[][] {
                new float[] { 0.399f, 0.399f, 0.399f, 0, 0 },
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new float[] { 0,      0,      0,      1, 0 },
                new float[] { 0,      0,      0,      0, 1 }
            };

                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(gray_matrix));
                ia.SetThreshold((float)1.8);
                var rc = new Rectangle(0, 0, TempBmp.Width, TempBmp.Height);
                gr.DrawImage(TempBmp, rc, 0, 0, TempBmp.Width, TempBmp.Height, GraphicsUnit.Pixel, ia);
            }

            return TempBmp;
        }

        // convertit une image bitmap d'un fichier vers un stream
        public static Bitmap ByteToImage(byte[] blob)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(blob, 0, blob.Length);
                mStream.Seek(0, SeekOrigin.Begin);

                Bitmap bm = new Bitmap(mStream);
                return bm;
            }
        }

        // convert un stream byte[] en une image bitmap
        public static Bitmap BytesToBitmap(byte[] Bytes)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Bytes);
                return new Bitmap((Image)new Bitmap(stream));
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            finally
            {
                stream.Close();
            }
        }

        // convertit une image bitmap vers un stream
        public static byte[] BitmapToBytes(Bitmap Bitmap)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                Bitmap.Save(ms, Bitmap.RawFormat);
                byte[] byteImage = new Byte[ms.Length];
                byteImage = ms.ToArray();
                return byteImage;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            }
        }
        {
            Bitmap b = (Bitmap)Image.FromFile(filename);
            // on redimensionne l'image
            int new_with = b.Width + ((b.Width * 100) / 100);
            int new_height = b.Height + ((b.Height * 100) / 100);
            Bitmap b_redim = ResizeImage(b, new_with, new_height);

            // on svg l'image redimensionnée
            Bitmap TempBmp = (Bitmap)b_redim.Clone();
            //TempBmp.Save(@"c:\temp\re_redim.jpg", ImageFormat.Jpeg);

            // on applique un flou gaussien
            ApplyGaussianBlur(ref b_redim, 4);

            // on svg l'image après flou gaussien
            Bitmap TempBmp2 = (Bitmap)b_redim.Clone();
            //TempBmp.Save(@"c:\temp\re_gauss.jpg", ImageFormat.Jpeg);

            // on applique un filtre gris
            Bitmap b_gray = ToGrayScale(TempBmp2);

            // on sauvegarde l'image après filtre gris gris
            Bitmap TempBmp3 = (Bitmap)b_gray.Clone();
            //TempBmp3.Save(@"c:\temp\re_grey.jpg", ImageFormat.Jpeg);


            // on applique un filtre noir et blanc sur l'image
            Bitmap b_black = ToBlackAndwhite(TempBmp3);

            // on sauvegarde l'image filtre noir et blanc sur l'image
            Bitmap TempBmp4 = (Bitmap)b_black.Clone();
            //TempBmp4.Save(@"c:\temp\re_final.jpg", ImageFormat.Jpeg);


            // on extrait le texte de l'image gaussié et redimensioné
            var Result = new AutoOcr().Read(TempBmp4);
            //Console.WriteLine(Result.Text);
            this.text_retrieved = Result.Text;

            return this.text_retrieved;
        }*/

        // Getters & Setters
        public string Filename { get => filename; set => filename = value; }
        public string Text_retrieved { get => text_retrieved; set => text_retrieved = value; }
    }
}
