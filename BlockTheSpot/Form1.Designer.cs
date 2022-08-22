﻿namespace BlockTheSpot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlockTheSpot));
            this.PatchButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.SpotifyPictureBox = new System.Windows.Forms.PictureBox();
            this.WorkingPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.SpotifyPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorkingPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // PatchButton
            // 
            this.PatchButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PatchButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PatchButton.Location = new System.Drawing.Point(16, 73);
            this.PatchButton.Margin = new System.Windows.Forms.Padding(4);
            this.PatchButton.Name = "PatchButton";
            this.PatchButton.Size = new System.Drawing.Size(256, 66);
            this.PatchButton.TabIndex = 12;
            this.PatchButton.Text = "Bloquear anuncios";
            this.PatchButton.UseVisualStyleBackColor = true;
            this.PatchButton.Click += new System.EventHandler(this.PatchButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResetButton.Location = new System.Drawing.Point(16, 146);
            this.ResetButton.Margin = new System.Windows.Forms.Padding(4);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(256, 66);
            this.ResetButton.TabIndex = 13;
            this.ResetButton.Text = "Restablecer Spotify";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // SpotifyPictureBox
            // 
            this.SpotifyPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.SpotifyPictureBox.Enabled = false;
            this.SpotifyPictureBox.ErrorImage = null;
            this.SpotifyPictureBox.Image = global::BlockTheSpot.Properties.Resources.AddsOnImage;
            this.SpotifyPictureBox.InitialImage = null;
            this.SpotifyPictureBox.Location = new System.Drawing.Point(7, 6);
            this.SpotifyPictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.SpotifyPictureBox.Name = "SpotifyPictureBox";
            this.SpotifyPictureBox.Size = new System.Drawing.Size(265, 59);
            this.SpotifyPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.SpotifyPictureBox.TabIndex = 7;
            this.SpotifyPictureBox.TabStop = false;
            // 
            // WorkingPictureBox
            // 
            this.WorkingPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.WorkingPictureBox.ErrorImage = null;
            this.WorkingPictureBox.Image = global::BlockTheSpot.Properties.Resources.WorkingImage;
            this.WorkingPictureBox.InitialImage = null;
            this.WorkingPictureBox.Location = new System.Drawing.Point(16, 73);
            this.WorkingPictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.WorkingPictureBox.Name = "WorkingPictureBox";
            this.WorkingPictureBox.Size = new System.Drawing.Size(256, 143);
            this.WorkingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.WorkingPictureBox.TabIndex = 12;
            this.WorkingPictureBox.TabStop = false;
            this.WorkingPictureBox.Visible = false;
            // 
            // BlockTheSpot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(230)))), ((int)(((byte)(140)))));
            this.ClientSize = new System.Drawing.Size(291, 230);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.PatchButton);
            this.Controls.Add(this.SpotifyPictureBox);
            this.Controls.Add(this.WorkingPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlockTheSpot";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BlockTheSpot";
            this.TopMost = true;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.BlockTheSpot_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BlockTheSpot_FormClosing);
            this.Load += new System.EventHandler(this.BlockTheSpot_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SpotifyPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorkingPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox SpotifyPictureBox;
        private System.Windows.Forms.PictureBox WorkingPictureBox;
        private System.Windows.Forms.Button PatchButton;
        private System.Windows.Forms.Button ResetButton;
    }
}

