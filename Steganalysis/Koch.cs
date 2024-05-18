using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
            if (dialog.ShowDialog() == DialogResult.OK) 
            {
                try
                {
                    pictureBox1.Image = new Bitmap(dialog.FileName);
                }
                catch
                {
                    return;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null || comboBox1.SelectedItem == null) return;
            ZhaoKoch alg = new ZhaoKoch(comboBox1.SelectedItem.ToString());
            double[] result = alg.chartData(pictureBox1.Image);
            double[] zapoln = alg.Analys(result);
            label2.Text = zapoln[0].ToString();
            Axis ax = new Axis();
            ax.Title = "Блок изображение";
            Axis ay = new Axis();
            ay.Title = "Значение абсолютной разности коэффициентов";
            chart1.ChartAreas[0].AxisX = ax;
            chart1.ChartAreas[0].AxisY = ay;
            chart1.Series[0].Points.DataBindY(result);
        }
    }
}
