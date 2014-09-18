namespace LocalAudioBroadcast {
    partial class MainForm {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.cbRenderers = new System.Windows.Forms.ComboBox();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.ilVolume = new System.Windows.Forms.ImageList(this.components);
            this.ilPlay = new System.Windows.Forms.ImageList(this.components);
            this.tbVolume = new System.Windows.Forms.TrackBar();
            this.btnVolume = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnRefreshRenderers = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.controlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // cbRenderers
            // 
            this.cbRenderers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRenderers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRenderers.FormattingEnabled = true;
            this.cbRenderers.Location = new System.Drawing.Point(12, 12);
            this.cbRenderers.Name = "cbRenderers";
            this.cbRenderers.Size = new System.Drawing.Size(253, 21);
            this.cbRenderers.TabIndex = 0;
            this.cbRenderers.SelectedIndexChanged += new System.EventHandler(this.cbRenderers_SelectedIndexChanged);
            this.cbRenderers.SelectionChangeCommitted += new System.EventHandler(this.cbRenderers_SelectionChangeCommitted);
            // 
            // controlPanel
            // 
            this.controlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.controlPanel.Controls.Add(this.btnVolume);
            this.controlPanel.Controls.Add(this.btnPlay);
            this.controlPanel.Controls.Add(this.tbVolume);
            this.controlPanel.Enabled = false;
            this.controlPanel.Location = new System.Drawing.Point(12, 47);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(243, 40);
            this.controlPanel.TabIndex = 7;
            // 
            // ilVolume
            // 
            this.ilVolume.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilVolume.ImageStream")));
            this.ilVolume.TransparentColor = System.Drawing.Color.Transparent;
            this.ilVolume.Images.SetKeyName(0, "audio-volume-muted.png");
            this.ilVolume.Images.SetKeyName(1, "audio-volume-low.png");
            this.ilVolume.Images.SetKeyName(2, "audio-volume-medium.png");
            this.ilVolume.Images.SetKeyName(3, "audio-volume-high.png");
            // 
            // ilPlay
            // 
            this.ilPlay.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilPlay.ImageStream")));
            this.ilPlay.TransparentColor = System.Drawing.Color.Transparent;
            this.ilPlay.Images.SetKeyName(0, "media-playback-start.png");
            this.ilPlay.Images.SetKeyName(1, "media-playback-stop.png");
            // 
            // tbVolume
            // 
            this.tbVolume.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVolume.Location = new System.Drawing.Point(82, -4);
            this.tbVolume.Maximum = 100;
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(161, 45);
            this.tbVolume.TabIndex = 7;
            this.tbVolume.TickFrequency = 5;
            this.tbVolume.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.tbVolume.Value = 100;
            this.tbVolume.ValueChanged += new System.EventHandler(this.tbVolume_ValueChanged);
            // 
            // btnVolume
            // 
            this.btnVolume.ImageIndex = 3;
            this.btnVolume.ImageList = this.ilVolume;
            this.btnVolume.Location = new System.Drawing.Point(41, 0);
            this.btnVolume.Name = "btnVolume";
            this.btnVolume.Size = new System.Drawing.Size(35, 32);
            this.btnVolume.TabIndex = 11;
            this.btnVolume.UseVisualStyleBackColor = true;
            this.btnVolume.Click += new System.EventHandler(this.btnVolume_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.ImageIndex = 0;
            this.btnPlay.ImageList = this.ilPlay;
            this.btnPlay.Location = new System.Drawing.Point(0, 0);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(35, 32);
            this.btnPlay.TabIndex = 8;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnRefreshRenderers
            // 
            this.btnRefreshRenderers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshRenderers.Image = global::LocalAudioBroadcast.Properties.Resources.view_refresh;
            this.btnRefreshRenderers.Location = new System.Drawing.Point(271, 11);
            this.btnRefreshRenderers.Name = "btnRefreshRenderers";
            this.btnRefreshRenderers.Size = new System.Drawing.Size(24, 23);
            this.btnRefreshRenderers.TabIndex = 1;
            this.btnRefreshRenderers.Click += new System.EventHandler(this.btnRefreshRenderers_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbout.Image = global::LocalAudioBroadcast.Properties.Resources.dialog_information;
            this.btnAbout.Location = new System.Drawing.Point(261, 47);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(35, 32);
            this.btnAbout.TabIndex = 13;
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 90);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.btnRefreshRenderers);
            this.Controls.Add(this.cbRenderers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Local Audio Broadcast";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbRenderers;
        private System.Windows.Forms.Button btnRefreshRenderers;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.TrackBar tbVolume;
        private System.Windows.Forms.ImageList ilVolume;
        private System.Windows.Forms.Button btnVolume;
        private System.Windows.Forms.ImageList ilPlay;
        private System.Windows.Forms.Button btnAbout;
    }
}

