using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpChip
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            numericUpDownIPF.Value = AppSettings.IPF;
        }

        private void numericUpDownIPF_ValueChanged(object sender, EventArgs e)
        {
            AppSettings.IPF = (int)numericUpDownIPF.Value;
        }
    }
}
