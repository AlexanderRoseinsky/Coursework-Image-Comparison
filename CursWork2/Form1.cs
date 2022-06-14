using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using MathNet.Numerics.LinearAlgebra;

namespace CursWork2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private int grayGradient(UInt32 number)
        {
            int R = (int)((number & 0x00FF0000) >> 16);
            int G = (int)((number & 0x0000FF00) >> 8);
            int B = (int)(number & 0x000000FF);
            int Y = (R + G + B) / 3;
            return Y;
        }

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

        static int hammingDist(String str1, String str2)
        {
            int i = 0, count = 0;
            while (i < str1.Length)
            {
                if (str1[i] != str2[i])
                    count++;
                i++;
            }
            return count;
        }

        private string aHash(Bitmap image, int size)
        {

            int average = 0;
            string aHashOrign = "";

            image = ResizeImage(image, size, size);

            Bitmap grayImage = new Bitmap(image);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    UInt32 pixel = (UInt32)(image.GetPixel(i, j).ToArgb());
                    int Y = grayGradient(pixel);
                    average += Y;
                    Color c = Color.FromArgb(Y, Y, Y);
                    grayImage.SetPixel(i, j, c);
                }
            }
            average /= size * size;
            Bitmap lowImage = new Bitmap(grayImage);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    UInt32 pixel = (UInt32)(grayImage.GetPixel(i, j).ToArgb());
                    int Y = grayGradient(pixel);
                    if (average > Y)
                    {
                        lowImage.SetPixel(i, j, Color.Black);
                        aHashOrign += "1";
                    }
                    else
                    {
                        lowImage.SetPixel(i, j, Color.White);
                        aHashOrign += "0";
                    }
                }
            }
            return aHashOrign;
        }

        private string pHash(Bitmap image,int size)
        {
            string pHashOrigne = "";
            image = ResizeImage(image, size, size);
            double[,] matrixOfBright = new double[32, 32];
            Bitmap grayImage = new Bitmap(image);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    UInt32 pixel = (UInt32)(image.GetPixel(i, j).ToArgb());
                    int Y = grayGradient(pixel);
                    Color c = Color.FromArgb(Y, Y, Y);
                    matrixOfBright[i, j] = Convert.ToDouble(Y);
                    grayImage.SetPixel(i, j, c);
                }
            }
            double[,] matrixOfCoef = new double[32,32];
            double constForMatrix = 32 * Math.Sqrt(32);
            for(int i=0;i< size; i++)
            {
                for(int j = 0;j< size; j++)
                {
                    if(i==0)
                    {
                        matrixOfCoef[i, j] =  1 / Math.Sqrt(32)*constForMatrix;
                    }
                    else if(i>0)
                    {
                        matrixOfCoef[i,j] =0.25*Math.Cos((i*Math.PI*(2*j+1))/64)*constForMatrix;
                    }
                }
            }

            Matrix<double> matrixC = Matrix<double>.Build.DenseOfArray(matrixOfCoef);
            Matrix<double> matrixA = Matrix<double>.Build.DenseOfArray(matrixOfBright);
            Matrix<double> matrixCTranpose = matrixC.Transpose();
            Matrix<double> matrixDCT = matrixC.Multiply(matrixA.Multiply(matrixCTranpose));
            Console.WriteLine(matrixDCT);

            double averangeOfDCT = 0;

            for(int i=0;i<8;i++)
            {
                for(int j=1;j<8;j++)
                {
                    averangeOfDCT += matrixDCT[i, j];
                }
            }

            averangeOfDCT = averangeOfDCT / 63;

            for (int i=0;i<8;i++)
            {
                for(int j=0;j<8; j++)
                {
                    if(matrixDCT[i,j]<averangeOfDCT)
                    {
                        pHashOrigne += "1";
                    }
                    else
                    {
                        pHashOrigne += "0";
                    }
                }
            }

            return pHashOrigne;
        }

        private string dHash(Bitmap image,int sizeW,int sizeH)
        {
            string dHashOrigne = "";
            image = ResizeImage(image, sizeW, sizeH);
            Bitmap grayImage = new Bitmap(image);
            for (int i = 0; i < sizeW; i++)
            {
                for (int j = 0; j < sizeH; j++)
                {
                    UInt32 pixel = (UInt32)(image.GetPixel(i, j).ToArgb());
                    int Y = grayGradient(pixel);
                    Color c = Color.FromArgb(Y, Y, Y);
                    grayImage.SetPixel(i, j, c);
                }
            }
            for (int i=0;i<8;i++)
            {
                for(int j=0;j<7;j++)
                {
                    int firstPix = grayGradient((UInt32)(grayImage.GetPixel(i, j).ToArgb()));
                    int secondPix = grayGradient((UInt32)(grayImage.GetPixel(i, j+1).ToArgb()));
                    if (secondPix > firstPix)
                    {
                        dHashOrigne += "1";
                    }
                    else
                    {
                        dHashOrigne += "0";
                    }
                }
            }
            return dHashOrigne;
        }

        private string gHash(Bitmap image, int size)
        {
            string gHashOrigne = "";
            image = ResizeImage(image, size,size);
            Bitmap grayImage = new Bitmap(image);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    UInt32 pixel = (UInt32)(image.GetPixel(i, j).ToArgb());
                    int Y = grayGradient(pixel);
                    Color c = Color.FromArgb(Y, Y, Y);
                    grayImage.SetPixel(i, j, c);
                }
            }
            for(int i=0;i<size-1;i++)
            {
                int sumOf1Row = 0;
                int sumOf1Col = 0;
                int sumOf2Row = 0;
                int sumOf2Col = 0;
                for (int j=0;j<size;j++)
                {
                    sumOf1Row += grayGradient((UInt32)(grayImage.GetPixel(i, j).ToArgb()));
                    sumOf2Row += grayGradient((UInt32)(grayImage.GetPixel(i+1, j).ToArgb()));
                }
                for (int j = 0; j < size; j++)
                {
                    sumOf1Col += grayGradient((UInt32)(grayImage.GetPixel(j, i).ToArgb()));
                    sumOf2Col += grayGradient((UInt32)(grayImage.GetPixel(j, i+1).ToArgb()));
                }
                if(sumOf1Row>sumOf2Row)
                {
                    gHashOrigne += "1";
                }
                else
                {
                    gHashOrigne += "0";
                }
                if(sumOf1Col>sumOf2Col)
                {
                    gHashOrigne += "1";
                }
                else
                {
                    gHashOrigne += "0";
                }
            }
            return gHashOrigne;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                label1.Text = browserDialog.SelectedPath;
                DirectoryInfo directoryInfo = new DirectoryInfo(label1.Text);
                foreach (var it in directoryInfo.GetFiles())
                {
                    if (it.Extension == ".jpg" || it.Extension == ".jpeg" || it.Extension == ".JPG")
                    {
                        listBox1.Items.Add(it.FullName);
                    }
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.SelectedIndex;
            Bitmap bitmap = new Bitmap(listBox1.Items[index].ToString());
            pictureBox1.Image = bitmap;
            if (pictureBox1.Image != null)
            {
                groupBox1.Visible = true;
            }
            else
            {
                groupBox1.Visible = false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Clear();
            int size = 8;
            int cnt = 0;
            Bitmap picture = new Bitmap(pictureBox1.Image);
            textBox1.Text = aHash(picture,size);
            for(int i=0;i<listBox1.Items.Count;i++)
            {
                if(listBox1.Items[i].ToString()!=listBox1.SelectedItem.ToString())
                {
                    Bitmap secondBitmap = new Bitmap(listBox1.Items[i].ToString());
                    string aHashSecond = aHash(secondBitmap, size);
                    int distance = hammingDist(textBox1.Text, aHashSecond);
                    if(distance<10)
                    {
                        cnt++;
                    }
                }
            }
            textBox2.Text = cnt.ToString();
        }
         
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            int size = 32;
            int cnt = 0;
            Bitmap picture = new Bitmap(pictureBox1.Image);
            textBox1.Text = pHash(picture, size);
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (listBox1.Items[i].ToString() != listBox1.SelectedItem.ToString())
                {
                    Bitmap secondBitmap = new Bitmap(listBox1.Items[i].ToString());
                    string pHashSecond = pHash(secondBitmap, size);
                    int distance = hammingDist(textBox1.Text, pHashSecond);
                    if (distance < 5)
                    {
                        cnt++;
                    }
                }
            }
            textBox2.Text = cnt.ToString();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            int imageW = 9;
            int imageH = 8;
            int cnt = 0;
            Bitmap picture = new Bitmap(pictureBox1.Image);
            textBox1.Text = dHash(picture, imageW,imageH);
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (listBox1.Items[i].ToString() != listBox1.SelectedItem.ToString())
                {
                    Bitmap secondBitmap = new Bitmap(listBox1.Items[i].ToString());
                    string dHashSecond = dHash(secondBitmap, imageW,imageH);
                    int distance = hammingDist(textBox1.Text, dHashSecond);
                    if (distance < 5)
                    {
                        cnt++;
                    }
                }
            }
            textBox2.Text = cnt.ToString();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            int imageSize = 32;
            int cnt = 0;
            Bitmap picture = new Bitmap(pictureBox1.Image);
            textBox1.Text = gHash(picture, imageSize);
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (listBox1.Items[i].ToString() != listBox1.SelectedItem.ToString())
                {
                    Bitmap secondBitmap = new Bitmap(listBox1.Items[i].ToString());
                    string gHashSecond = gHash(secondBitmap, imageSize);
                    int distance = hammingDist(textBox1.Text, gHashSecond);
                    if (distance < 5)
                    {
                        cnt++;
                    }
                }
            }
            textBox2.Text = cnt.ToString(); 
        }
    }
}
