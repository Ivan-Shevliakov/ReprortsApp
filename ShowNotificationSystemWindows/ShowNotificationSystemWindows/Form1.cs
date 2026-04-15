using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
 using System.Media;

namespace ShowNotificationSystemWindows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.CenterToScreen();
            button1.Click += ButtonOnclick;
            SystemSounds.Hand.Play();
        }
        private void ButtonOnclick(object sender, EventArgs e)
        {
            this.Close();
        }
    

    }
}
