using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptimizePooling
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnSetGridCnt_Click(object sender, EventArgs e)
        {
           
            string sGridCnt = txtGridCnt.Text;
            if (sGridCnt == "")
            {
                SetErrorInfo("样本数不得为空！");
                return;
            }
            int gridCnt = 0;
            bool bOk = int.TryParse(sGridCnt, out gridCnt);
            if (gridCnt < 1 || gridCnt > 10)
            {
                SetErrorInfo("样本数必须在1~10之间！");
                return;
            }
           
            EnableControls(false);

        }

        private void SetErrorInfo(string info)
        {
            txtInfo.Text = info;
            txtInfo.ForeColor = Color.Red;
            txtInfo.BackColor = Color.White;
        }

        private void EnableControls(bool bEnable)
        {
            txtGridCnt.Enabled = bEnable;
        }
    }
}
