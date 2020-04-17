using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RubicTrainDataGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult dialogResult;
            string backgroundsFile = null;
            string destFile = null;
            MessageBox.Show("Alegeti folderul cu fundaluri. Acesta trebuie sa contina DAOR poze in format BMP");
            do
            {
                dialogResult = DialogResult.Cancel;
                dialogResult = folderBrowserDialog1.ShowDialog();
            } while (dialogResult != DialogResult.OK);
            backgroundsFile = folderBrowserDialog1.SelectedPath;
            MessageBox.Show("Alegeti folderul destinatie.");
            do
            {
                dialogResult = DialogResult.Cancel;
                dialogResult = folderBrowserDialog1.ShowDialog();
            } while (dialogResult != DialogResult.OK);
            destFile = folderBrowserDialog1.SelectedPath;
            string[] backgrounds = System.IO.Directory.GetFiles(backgroundsFile);
            foreach (var file in backgrounds)
            {
                for (int nrpoza = 1; nrpoza < 2; nrpoza++)
                {
                    Color[] colors = { Color.White, Color.Yellow, Color.Orange, Color.Red, Color.Green, Color.Blue };
                    Bitmap[] Faces = new Bitmap[3];
                    int[,,] stickersLabels = new int[3, 3, 3];//face,line,column;
                    Random random = new Random();
                    int stickersize = 10 + random.Next(6, 12);
                    int col = random.Next();
                    for (int i = 0; i < 3; i++)
                    {
                        random = new Random(random.Next());
                        Faces[i] = Sticker(Color.FromArgb(col % 256, col % 256, col % 256), stickersize * 3 + stickersize * 3 / 6);

                        for (int j = 0; j < 9; j++)
                        {
                            int label = random.Next(0, 6);
                            Bitmap sticker = Sticker(colors[label], stickersize);
                            stickersLabels[i, j / 3, 2 - j % 3] = label;
                            for (int x = 0; x < stickersize; x++)
                            {
                                for (int y = 0; y < stickersize; y++)
                                {
                                    Faces[i].SetPixel((j / 3) * (stickersize + stickersize / 5) + x, (j % 3) * (stickersize + stickersize / 5) + y, sticker.GetPixel(x, y));
                                }
                            }
                        }
                    }
                    Bitmap Pic = new Bitmap(file);
                    Vector3[] points = { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };//stanga sus ->,v,^   fata jos ->^
                    PointF[] pointsToScreen = new PointF[points.Length];
                    double tetaor = random.Next(25, 45) * Math.PI / 180;
                    double tetaver = random.Next(25, 65) * Math.PI / 180;
                    tetaor += GetPoisson(29) / 29 / Math.PI - 0.3;
                    tetaver += GetPoisson(29) / 29 / Math.PI - 0.3;
                    int setoffx = random.Next(5, Math.Max(60 - 3 * stickersize, 6));
                    int setoffy = random.Next(5, Math.Max(60 - 3 * stickersize, 6));
                    float minx = float.MaxValue, miny = float.MaxValue;
                    for (int ind = 0; ind < points.Length; ind++)
                    {
                        points[ind] = rotate(points[ind], tetaor, tetaver);
                        pointsToScreen[ind] = ProjectToObserver(points[ind], stickersize);
                        //pointsToScreen[ind] = new Point(pointsToScreen[ind].X + setoffx, pointsToScreen[ind].Y + setoffy);
                        if (minx > pointsToScreen[ind].X) minx = pointsToScreen[ind].X;
                        if (miny > pointsToScreen[ind].Y) miny = pointsToScreen[ind].Y;
                    }
                    for (int ind = 0; ind < points.Length; ind++)
                    {
                        pointsToScreen[ind] = new PointF(pointsToScreen[ind].X + setoffx - minx, pointsToScreen[ind].Y + setoffy - miny);
                    }

                    using (Graphics g = Graphics.FromImage(Pic))
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            PointF[] parallelgram = new PointF[3];
                            if (k == 0)
                            {
                                parallelgram[0] = pointsToScreen[0];
                                parallelgram[1] = pointsToScreen[1];
                                parallelgram[2] = pointsToScreen[3];
                            }
                            else if (k == 1)
                            {
                                parallelgram[0] = pointsToScreen[1];
                                parallelgram[1] = pointsToScreen[2];
                                parallelgram[2] = pointsToScreen[4];
                            }
                            else if (k == 2)
                            {
                                parallelgram[0] = pointsToScreen[3];
                                parallelgram[1] = pointsToScreen[4];
                                parallelgram[2] = pointsToScreen[6];
                            }

                            g.DrawImage(Faces[k], parallelgram);

                        }

                    }
                    string ylabels = null;
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            for (int k = 0; k < 3; k++)
                                ylabels += stickersLabels[i, j, k].ToString() + " ";
                    System.IO.FileStream fileStream = System.IO.File.Create(destFile+"//" + System.IO.Path.GetFileNameWithoutExtension(file) + "_" + nrpoza.ToString() + ".txt");
                    byte[] info = new UTF8Encoding(true).GetBytes(ylabels);
                    fileStream.Write(info, 0, info.Length);
                    fileStream.Close();
                    string filee = destFile+"//" + System.IO.Path.GetFileNameWithoutExtension(file) + "_" + nrpoza.ToString() + ".bmp";
                    Pic.Save(filee);
                    //break;
                }
            }

        }
        PointF ProjectToObserver(Vector3 vector3, int size)
        {
            return new PointF((float)(vector3.Z * (size + 10)), (float)(vector3.Y * -(size + 10)));
        }
        class Vector3
        {
            public double X, Y, Z;
            public Vector3(double z, double y, double x)
            {
                X = x; Y = y; Z = z;
            }
        }
        Vector3 rotate(Vector3 point, double pitch, double roll)
        {
            var cosa = 1;
            var sina = 0;

            var cosb = Math.Cos(pitch);
            var sinb = Math.Sin(pitch);

            var cosc = Math.Cos(roll);
            var sinc = Math.Sin(roll);

            var Axx = cosa * cosb;
            var Axy = cosa * sinb * sinc - sina * cosc;
            var Axz = cosa * sinb * cosc + sina * sinc;

            var Ayx = sina * cosb;
            var Ayy = sina * sinb * sinc + cosa * cosc;
            var Ayz = sina * sinb * cosc - cosa * sinc;

            var Azx = -sinb;
            var Azy = cosb * sinc;
            var Azz = cosb * cosc;


            var px = point.X;
            var py = point.Y;
            var pz = point.Z;
            return new Vector3(Axx * px + Axy * py + Axz * pz, Ayx * px + Ayy * py + Ayz * pz, Azx * px + Azy * py + Azz * pz);

        }

        Bitmap Sticker(Color color, int length = 10)
        {
            Bitmap sticker = new Bitmap(length, length);
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                    sticker.SetPixel(i, j, color);
            int seed = length;
            for (int i = 0; i < length * length; i++)
            {
                Random rnd = new Random(i * seed);
                seed = rnd.Next(0, length);
                int x1 = rnd.Next(0, length);
                int x2 = rnd.Next(0, length);
                int y1 = rnd.Next(0, length);
                int y2 = rnd.Next(0, length);
                int dp = rnd.Next(0, 35);

                if (sticker.GetPixel(x1, y1).R - dp >= 0)
                    sticker.SetPixel(x1, y1, Color.FromArgb(sticker.GetPixel(x1, y1).R - dp, sticker.GetPixel(x1, y1).G, sticker.GetPixel(x1, y1).B));
                if (sticker.GetPixel(x1, y1).G - dp >= 0)
                    sticker.SetPixel(x1, y1, Color.FromArgb(sticker.GetPixel(x1, y1).R, sticker.GetPixel(x1, y1).G - dp, sticker.GetPixel(x1, y1).B));
                if (sticker.GetPixel(x1, y1).B - dp >= 0)
                    sticker.SetPixel(x1, y1, Color.FromArgb(sticker.GetPixel(x1, y1).R, sticker.GetPixel(x1, y1).G, sticker.GetPixel(x1, y1).B - dp));
                if (sticker.GetPixel(x2, y2).R + dp <= 255)
                    sticker.SetPixel(x2, y2, Color.FromArgb(sticker.GetPixel(x2, y2).R + dp, sticker.GetPixel(x2, y2).G, sticker.GetPixel(x2, y2).B));
                if (sticker.GetPixel(x2, y2).G + dp <= 255)
                    sticker.SetPixel(x2, y2, Color.FromArgb(sticker.GetPixel(x2, y2).R, sticker.GetPixel(x2, y2).G + dp, sticker.GetPixel(x2, y2).B));
                if (sticker.GetPixel(x2, y2).B + dp <= 255)
                    sticker.SetPixel(x2, y2, Color.FromArgb(sticker.GetPixel(x2, y2).R, sticker.GetPixel(x2, y2).G, sticker.GetPixel(x2, y2).B + dp));
            }
            return sticker;
        }
        public static int GetPoisson(double lambda)
        {
            return (lambda < 30.0) ? PoissonSmall(lambda) : PoissonLarge(lambda);
        }

        private static int PoissonSmall(double lambda)
        {
            // Algorithm due to Donald Knuth, 1969.
            double p = 1.0, L = Math.Exp(-lambda);
            int k = 0;
            do
            {
                Random random = new Random(k);
                k++;
                p *= random.NextDouble();
            }
            while (p > L);
            return k - 1;
        }
        static double LogFactorial(int n)
        {
            int fact = n;
            while (n > 0) fact *= --n;
            return Math.Log((double)fact);
        }
        private static int PoissonLarge(double lambda)
        {
            double c = 0.767 - 3.36 / lambda;
            double beta = Math.PI / Math.Sqrt(3.0 * lambda);
            double alpha = beta * lambda;
            double k = Math.Log(c) - lambda - Math.Log(beta);

            for (; ; )
            {
                Random random = new Random((int)lambda);
                double u = random.NextDouble();
                double x = (alpha - Math.Log((1.0 - u) / u)) / beta;
                int n = (int)Math.Floor(x + 0.5);
                if (n < 0)
                    continue;
                double v = random.NextDouble();
                double y = alpha - beta * x;
                double temp = 1.0 + Math.Exp(y);
                double lhs = y + Math.Log(v / (temp * temp));
                double rhs = k + n * Math.Log(lambda) - LogFactorial(n);
                if (lhs <= rhs)
                    return n;
            }
        }
    }
}
/*foreach(var file in Files)
            {
                //if (System.IO.Path.GetExtension(file) == ".bmp") continue;
                Bitmap myBmp = (System.Drawing.Bitmap) Bitmap.FromFile(file);
                Bitmap resized = new Bitmap(myBmp, new Size(72,72));
                resized.Save(System.IO.Path.ChangeExtension(file, ".bmp"));

                //System.IO.File.Delete(file);

            }*/
