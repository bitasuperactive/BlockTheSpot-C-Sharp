namespace BlockTheSpot
{
    partial class BlockTheSpot
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlockTheSpot));
            this.patchButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.spotifyPictureBox = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            this.outputLabel = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spotifyPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // patchButton
            // 
            this.patchButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.patchButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.patchButton.Location = new System.Drawing.Point(8, 59);
            this.patchButton.Name = "patchButton";
            this.patchButton.Size = new System.Drawing.Size(192, 54);
            this.patchButton.TabIndex = 12;
            this.patchButton.Text = "Bloquear anuncios";
            this.patchButton.UseVisualStyleBackColor = true;
            this.patchButton.Click += new System.EventHandler(this.PatchButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.resetButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetButton.Location = new System.Drawing.Point(8, 119);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(192, 54);
            this.resetButton.TabIndex = 13;
            this.resetButton.Text = "Restablecer Spotify";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // spotifyPictureBox
            // 
            this.spotifyPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.spotifyPictureBox.Enabled = false;
            this.spotifyPictureBox.ErrorImage = null;
            this.spotifyPictureBox.Image = global::BlockTheSpot.Properties.Resources.AddsOnImage;
            this.spotifyPictureBox.InitialImage = null;
            this.spotifyPictureBox.Location = new System.Drawing.Point(5, 5);
            this.spotifyPictureBox.Name = "spotifyPictureBox";
            this.spotifyPictureBox.Size = new System.Drawing.Size(199, 48);
            this.spotifyPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.spotifyPictureBox.TabIndex = 7;
            this.spotifyPictureBox.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(8, 196);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(170, 14);
            this.progressBar.TabIndex = 14;
            this.progressBar.Visible = false;
            // 
            // progressLabel
            // 
            this.progressLabel.Location = new System.Drawing.Point(178, 195);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(37, 14);
            this.progressLabel.TabIndex = 15;
            this.progressLabel.Text = "0%";
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.progressLabel.Visible = false;
            // 
            // outputLabel
            // 
            this.outputLabel.AccessibleDescription = "";
            this.outputLabel.AccessibleName = "";
            this.outputLabel.AutoSize = true;
            this.outputLabel.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputLabel.Location = new System.Drawing.Point(8, 176);
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.Size = new System.Drawing.Size(84, 15);
            this.outputLabel.TabIndex = 16;
            this.outputLabel.Tag = "";
            this.outputLabel.Text = "<outputLabel>";
            this.outputLabel.Visible = false;
            // 
            // BlockTheSpot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(230)))), ((int)(((byte)(140)))));
            this.ClientSize = new System.Drawing.Size(209, 225);
            this.Controls.Add(this.outputLabel);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.spotifyPictureBox);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.patchButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlockTheSpot";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BlockTheSpot";
            this.TopMost = true;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.BlockTheSpot_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BlockTheSpot_FormClosing);
            this.Load += new System.EventHandler(this.BlockTheSpot_Load);
            ((System.ComponentModel.ISupportInitialize)(this.spotifyPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox spotifyPictureBox;
        private System.Windows.Forms.Button patchButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.Label outputLabel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

