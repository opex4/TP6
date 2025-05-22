namespace TP6
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            picDisplay = new PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            btnRain = new Button();
            btnRestart = new Button();
            ((System.ComponentModel.ISupportInitialize)picDisplay).BeginInit();
            SuspendLayout();
            // 
            // picDisplay
            // 
            picDisplay.Location = new Point(12, 12);
            picDisplay.Name = "picDisplay";
            picDisplay.Size = new Size(741, 417);
            picDisplay.TabIndex = 0;
            picDisplay.TabStop = false;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 20;
            // 
            // btnRain
            // 
            btnRain.Location = new Point(759, 12);
            btnRain.Name = "btnRain";
            btnRain.Size = new Size(152, 29);
            btnRain.TabIndex = 1;
            btnRain.Text = "Дождь";
            btnRain.UseVisualStyleBackColor = true;
            // 
            // btnRestart
            // 
            btnRestart.Location = new Point(759, 400);
            btnRestart.Name = "btnRestart";
            btnRestart.Size = new Size(152, 29);
            btnRestart.TabIndex = 2;
            btnRestart.Text = "Начать заново";
            btnRestart.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(923, 443);
            Controls.Add(btnRestart);
            Controls.Add(btnRain);
            Controls.Add(picDisplay);
            Name = "Form1";
            Text = "Поливай лужайки!";
            ((System.ComponentModel.ISupportInitialize)picDisplay).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picDisplay;
        private System.Windows.Forms.Timer timer1;
        private Button btnRain;
        private Button btnRestart;
    }
}
