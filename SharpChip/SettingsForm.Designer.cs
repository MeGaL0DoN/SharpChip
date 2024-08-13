namespace SharpChip
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            numericUpDownIPF = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numericUpDownIPF).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 11.1F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(191, 92);
            label1.Name = "label1";
            label1.Size = new Size(412, 50);
            label1.TabIndex = 0;
            label1.Text = "Instructions Per Frame";
            // 
            // numericUpDownIPF
            // 
            numericUpDownIPF.Location = new Point(244, 197);
            numericUpDownIPF.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numericUpDownIPF.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownIPF.Name = "numericUpDownIPF";
            numericUpDownIPF.Size = new Size(300, 47);
            numericUpDownIPF.TabIndex = 2;
            numericUpDownIPF.Value = new decimal(new int[] { 15, 0, 0, 0 });
            numericUpDownIPF.ValueChanged += numericUpDownIPF_ValueChanged;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(17F, 41F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(numericUpDownIPF);
            Controls.Add(label1);
            Name = "SettingsForm";
            Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)numericUpDownIPF).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private NumericUpDown numericUpDownIPF;
    }
}