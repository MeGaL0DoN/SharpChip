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
