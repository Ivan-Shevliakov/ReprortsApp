using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShowNotificationSystemWindows
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            button1.Click += ButtonOnclick;
            SystemSounds.Hand.Play();
        }
        private void ButtonOnclick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
