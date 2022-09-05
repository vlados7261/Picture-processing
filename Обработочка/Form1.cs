using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Обработочка
{
    public partial class Form1 : Form
    {
        private List<byte[,,]> cifra1;
        private List<byte[,,]> cifra2;
        private List<double> proc;
        private byte[,] kol;
        private bool q = true;
        private Color MainColor;
        private bool mod = true;
        public int x;
        public int y;
        public int colz;
        public int mesto;
        public int k;
        
        public Form1()
        {
            InitializeComponent();

        }
        private void Processing(int i)
        {
            if (i % 3 == 0)
            {
                int ramka = hScrollBar1.Value;
                int y = i / 3 / cifra1[k].GetLength(2);
                int x = i / 3 % cifra1[k].GetLength(2);
                if (Math.Pow(MainColor.R - cifra1[k][0, y, x], 2) + Math.Pow(MainColor.G - cifra1[k][1, y, x], 2) + Math.Pow(MainColor.B - cifra1[k][2, y, x], 2) < ramka * ramka)
                {
                    kol[y, x] = 1;
                    cifra2[k][0, y, x] = (byte)(255 - MainColor.R);
                    cifra2[k][1, y, x] = (byte)(255 - MainColor.G);
                    cifra2[k][2, y, x] = (byte)(255 - MainColor.B);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < cifra1.Count; i++)
            {
                cifra2[i] = (byte[,,])cifra1[i].Clone();
                proc[i] = 0;
            }
            button2.Text = "ORIGINAL";
            if (mod)
            {
                k = 0;
                while (k<cifra1.Count)
                {
                    kol = new byte[cifra1[k].GetLength(1), cifra1[k].GetLength(2)];
                    Parallel.For(0, cifra1[k].GetLength(1) *cifra1[k].GetLength(2) * 3, Processing);
                    for (int i=0; i<cifra1[k].GetLength(1); i++)
                    {
                        for (int j = 0; j < cifra1[k].GetLength(2); j++)
                        {
                            if (kol[i, j] == 1)
                            {
                                proc[k] += 1;
                            }
                        }
                    }
                    proc[k] *= (double)100/(cifra1[k].GetLength(1) * cifra1[k].GetLength(2));
                    k += 1;
                }
                button2.Enabled = true;
                textBox1.Text = proc[mesto].ToString();
            }
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            MainColor = colorDialog1.Color;
            pictureBox2.BackColor = MainColor;
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            label1.Text = hScrollBar1.Value.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (mod == true)
            {
                mod = false;
                button3.Text = "Contrast changing";
            }
            else
            {
                mod = true;
                button3.Text = "Pixels counting";
            }
        }
        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            button5.Enabled = true;
            cifra1 = new List<byte[,,]>();
            cifra2 = new List<byte[,,]>();
            mesto = 0;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                button2.Enabled = false;
                string[] files = openFileDialog1.FileNames;
                proc = new List<double>();
                foreach (string f in files)
                {
                    cifra1.Add(BitmapToRgb(new Bitmap(@f)));
                    cifra2.Add(BitmapToRgb(new Bitmap(@f)));
                    proc.Add(0);
                }
                pictureBox1.Image = RgbToBmp(cifra1[mesto]);
                button1.Enabled = true;
                if (cifra1.Count == 1)
                {
                    button5.Enabled = false;
                }
            }
        }
        public unsafe static byte[,,] BitmapToRgb(Bitmap bmp)
        {
            int width = bmp.Width,
                height = bmp.Height;
            byte[,,] res = new byte[3, height, width];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                fixed (byte* _res = res)
                {
                    byte* _r = _res, _g = _res + width * height, _b = _res + 2 * width * height;
                    for (int h = 0; h < height; h++)
                    {
                        curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                        for (int w = 0; w < width; w++)
                        {
                            *_b = *(curpos++); ++_b;
                            *_g = *(curpos++); ++_g;
                            *_r = *(curpos++); ++_r;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return res;
        }
        public unsafe static Bitmap RgbToBmp(byte[,,] rgb)
        {
            Bitmap bmp = new Bitmap(rgb.GetLength(2),rgb.GetLength(1));
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, rgb.GetLength(2), rgb.GetLength(1)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos = (byte*)bd.Scan0;
                for (int h = 0; h < rgb.GetLength(1); h++)
                {
                    for (int w = 0; w < rgb.GetLength(2); w++)
                    {
                        curpos[2] = rgb[0, h, w];
                        curpos[1] = rgb[1, h, w];
                        curpos[0] = rgb[2, h, w];
                        curpos += 3;
                    }

                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            using (Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height))
            {
                pictureBox1.DrawToBitmap(bmp, pictureBox1.ClientRectangle);
                MainColor= bmp.GetPixel(e.X, e.Y);
                pictureBox2.BackColor = MainColor;
                colorDialog1.Color=MainColor;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (mesto + 1 < cifra1.Count)
            {
                pictureBox1.Image = RgbToBmp(cifra1[mesto + 1]);
                button2.Text = "ORIGINAL";
                mesto += 1;
                button4.Enabled = true;
                textBox1.Text = proc[mesto].ToString();
            }
            if (mesto  == cifra1.Count - 1)
            {
                button5.Enabled=false;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (mesto - 1 >= 0)
            {
                pictureBox1.Image = RgbToBmp(cifra1[mesto - 1]);
                button2.Text = "ORIGINAL";
                mesto -= 1;
                button5.Enabled = true;
               // textBox1.Text = proc[mesto].ToString();
            }
            if (mesto  == 0)
            {
                button4.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (q)
            {
                pictureBox1.Image = RgbToBmp(cifra2[mesto]);
                button2.Text = "CHANGED";
                q = false;
            }
            else
            {
                pictureBox1.Image = RgbToBmp(cifra1[mesto]);
                button2.Text = "ORIGINAL";
                q = true;
            }

        }
    }
}
