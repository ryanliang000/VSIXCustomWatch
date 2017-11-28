using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VSIXCutomWatch
{
    public partial class configForm : Form
    {
        public configForm()
        {
            InitializeComponent();
            string strDllName;
            string strDLLFunc;
            WatchConfig.GetAppConfig(out strDllName, out strDLLFunc);
            textBox1.Text = strDllName;
            textBox2.Text = strDLLFunc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strDllName = textBox1.Text;
            string strDLLFunc = textBox2.Text;
            if (strDLLFunc != "" && strDLLFunc != "")
            {
                WatchConfig.SetAppConfig(strDllName, strDLLFunc);
                Close();
            }
            else
            {

            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
