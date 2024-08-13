using System.Drawing.Drawing2D;

namespace SharpChip
{
    public partial class MainForm : Form
    {
        private PictureBox pictureBox = new PictureBox();
        private Bitmap screenBuffer = new Bitmap(64, 32);

        private ChipCore chipCore = new ChipCore();
        bool emulationPaused = false;

        public MainForm()
        {
            InitializeComponent();

            pictureBox = new InterpolatingPictureBox
            {
                Image = screenBuffer,
                SizeMode = PictureBoxSizeMode.Zoom,
                InterpolationMode = InterpolationMode.NearestNeighbor,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(pictureBox);
            pictureBox.BringToFront();

            chipCore.drawToBuffer(screenBuffer);
            pictureBox.Invalidate();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[]? files = (string[]?)e?.Data?.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
                chipCore.LoadRom(files[0]);
        }

        int checkChipKeyPress(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D1: return 1;
                case Keys.D2: return 2;
                case Keys.D3: return 3;
                case Keys.D4: return 0xC;
                case Keys.Q: return 4;
                case Keys.W: return 5;
                case Keys.E: return 6;
                case Keys.R: return 0xD;
                case Keys.A: return 7;
                case Keys.S: return 8;
                case Keys.D: return 9;
                case Keys.F: return 0xE;
                case Keys.Z: return 0xA;
                case Keys.X: return 0;
                case Keys.C: return 0xB;
                case Keys.V: return 0xF;
            }

            return -1;
        }

        private void changePauseState()
        {
            if (emulationPaused)
            {
                emulationPaused = false;
                this.Text = "SharpChip";
            }
            else
            {
                emulationPaused = true;
                this.Text = "SharpChip (Paused)";
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
                changePauseState();
            else
            {
                int key = checkChipKeyPress(e);
                if (key != -1) chipCore.setKey(key, true);
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            int key = checkChipKeyPress(e);
            if (key != -1) chipCore.setKey(key, false);
        }

        private void chipTimer_Tick(object sender, EventArgs e)
        {
            if (!chipCore.RomLoaded || emulationPaused) return;

            chipCore.updateTimers();

            for (int i = 0; i < AppSettings.IPF; i++)
                chipCore.execute();

            chipCore.drawToBuffer(screenBuffer);
            pictureBox.Invalidate();
        }

        private void loadROToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
                chipCore.LoadRom(openFileDialog1.FileName);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool wasPaused = emulationPaused;
            if (!wasPaused) changePauseState();

            var frm = new SettingsForm();
            frm.StartPosition = FormStartPosition.CenterParent;

           frm.ShowDialog();
            if (!wasPaused) changePauseState();
        }
    }

    public class InterpolatingPictureBox : PictureBox
    {
        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs eventArgs)
        {
            eventArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            eventArgs.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(eventArgs);
        }
    }
}