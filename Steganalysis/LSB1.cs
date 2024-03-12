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
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                pictureBox1.Image = new Bitmap(dialog.FileName); ;
            }
        }

        private void Analisys_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictureBox1.Image);
            redPictureBox.Image =  Lsb1Analis.drawPic(img, "red");
            greenPictureBox.Image = Lsb1Analis.drawPic(img, "green");
            bluePictureBox.Image = Lsb1Analis.drawPic(img, "blue");
            chart1.Series[0].Points.DataBindY(Lsb1Analis.analisysB(img));
        }


    }
}
