using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steganalysis
{
    public partial class LSB1 : Form
    {
        public LSB1()
        {
            InitializeComponent();
        }

        private void AddImg_Click(object sender, EventArgs e)
        {
            if(pictureBox1.Image != null)pictureBox1.Image.Dispose();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            int data = 0;
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                data = dialog.FileName.Length;
                pictureBox1.Image = new Bitmap(dialog.FileName); ;
            }
            
        }

        private void Analisys_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictureBox1.Image);
            redPictureBox.Image =  Lsb1Analis.drawPic(img, "red");
            greenPictureBox.Image = Lsb1Analis.drawPic(img, "green");
            bluePictureBox.Image = Lsb1Analis.drawPic(img, "blue");
            double[] result = Lsb1Analis.analisys(img);
            chart1.Series[0].Points.DataBindY(result);
            double zapoln = 0;
            for(int i = 0; i < result.Length; i++)
            {
                if (result[i]>0.5) zapoln++;
            }
            zapoln = zapoln/result.Length;
            for (int i=0;i<img.Height;i++)
            {
                if (result[i] > 0.5)
                {
                    for(int j = 0; j < img.Width; j++)
                    {
                        img.SetPixel(j, i, Color.FromArgb(img.GetPixel(j, i).A, 255, img.GetPixel(j, i).G, img.GetPixel(j, i).B));
                    }
                }
                else
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        img.SetPixel(j, i, Color.FromArgb(img.GetPixel(j, i).A, img.GetPixel(j, i).R, 200, img.GetPixel(j, i).B));
                    }
                }
            }
            pictureBox2.Image = img;
            label2.Text = zapoln.ToString();
        }


    }
}
