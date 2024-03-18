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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LSB1 lsb = new LSB1();
            lsb.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LSB2 lsb = new LSB2();
            lsb.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Koch lsb = new Koch();
            lsb.Show();
        }
    }
}
