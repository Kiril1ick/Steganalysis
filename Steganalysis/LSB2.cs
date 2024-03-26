using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Steganalysis
{
    public partial class LSB2 : Form
    {
        public LSB2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            int data = 0;
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                data = dialog.FileName.Length;
                pictureBox1.Image = new Bitmap(dialog.FileName); ;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RSAnalysis rsa = new RSAnalysis(2, 2);
            Bitmap image = new Bitmap(pictureBox1.Image);
            pictureBox2.Image = Lsb1Analis.drawPic(image);
            double average = 0;
            double[] results = rsa.doAnalysis(image, RSAnalysis.ANALYSIS_COLOUR_RED, true);
            label6.Text = results[26].ToString();
            average += results[26];
            results = rsa.doAnalysis(image, RSAnalysis.ANALYSIS_COLOUR_GREEN, true);
            label7.Text = results[26].ToString();
            average += results[26];
            results = rsa.doAnalysis(image, RSAnalysis.ANALYSIS_COLOUR_BLUE, true);
            label8.Text = results[26].ToString();
            average += results[26];
            average = average / 3;
            label1.Text = average.ToString();
            Console.WriteLine();
        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
