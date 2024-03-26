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
    public partial class Koch : Form
    {
        public Koch()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                pictureBox1.Image = new Bitmap(dialog.FileName); ;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ZhaoKoch alg = new ZhaoKoch(comboBox1.SelectedItem.ToString());
            double[] result = alg.chartData(pictureBox1.Image);
            double zapoln = alg.testAnalys(result);
            label3.Text = zapoln.ToString();
            chart1.Series[0].Points.DataBindY(result);
        }
    }
}
