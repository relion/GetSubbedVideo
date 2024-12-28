namespace GetSubbedVideo
{
    partial class MainForm
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
            this.movie_or_Series_Episode_TextBox = new System.Windows.Forms.TextBox();
            this.run_Button = new System.Windows.Forms.Button();
            this.movie_or_Series_Episode_Label = new System.Windows.Forms.Label();
            this.outputRichTextBox = new System.Windows.Forms.RichTextBox();
            this.outputBestRichTextBox = new System.Windows.Forms.RichTextBox();
            this.pauseCheckBox = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.subtitlesLangLabel = new System.Windows.Forms.Label();
            this.lang_ComboBox = new System.Windows.Forms.ComboBox();
            this.skip_skytorrents_scraping_CheckBox = new System.Windows.Forms.CheckBox();
            this.resetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // movie_or_Series_Episode_TextBox
            // 
            this.movie_or_Series_Episode_TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.movie_or_Series_Episode_TextBox.Location = new System.Drawing.Point(197, 12);
            this.movie_or_Series_Episode_TextBox.Name = "movie_or_Series_Episode_TextBox";
            this.movie_or_Series_Episode_TextBox.Size = new System.Drawing.Size(333, 20);
            this.movie_or_Series_Episode_TextBox.TabIndex = 0;
            this.movie_or_Series_Episode_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.key_pressed_on_movie_or_Series_Episode_TextBox);
            // 
            // run_Button
            // 
            this.run_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.run_Button.Location = new System.Drawing.Point(536, 10);
            this.run_Button.Name = "run_Button";
            this.run_Button.Size = new System.Drawing.Size(42, 23);
            this.run_Button.TabIndex = 1;
            this.run_Button.Text = "Run";
            this.run_Button.UseVisualStyleBackColor = true;
            this.run_Button.Click += new System.EventHandler(this.run_handler);
            // 
            // movie_or_Series_Episode_Label
            // 
            this.movie_or_Series_Episode_Label.AutoSize = true;
            this.movie_or_Series_Episode_Label.Location = new System.Drawing.Point(12, 15);
            this.movie_or_Series_Episode_Label.Name = "movie_or_Series_Episode_Label";
            this.movie_or_Series_Episode_Label.Size = new System.Drawing.Size(179, 13);
            this.movie_or_Series_Episode_Label.TabIndex = 2;
            this.movie_or_Series_Episode_Label.Text = "Requested Movie or Series Episode:";
            // 
            // outputRichTextBox
            // 
            this.outputRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.outputRichTextBox.Name = "outputRichTextBox";
            this.outputRichTextBox.Size = new System.Drawing.Size(565, 95);
            this.outputRichTextBox.TabIndex = 4;
            this.outputRichTextBox.Text = "";
            // 
            // outputBestRichTextBox
            // 
            this.outputBestRichTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.outputBestRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.outputBestRichTextBox.Name = "outputBestRichTextBox";
            this.outputBestRichTextBox.Size = new System.Drawing.Size(130, 91);
            this.outputBestRichTextBox.TabIndex = 5;
            this.outputBestRichTextBox.Text = "";
            this.outputBestRichTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.handle_Link_Clicked);
            // 
            // pauseCheckBox
            // 
            this.pauseCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pauseCheckBox.AutoSize = true;
            this.pauseCheckBox.Location = new System.Drawing.Point(520, 39);
            this.pauseCheckBox.Name = "pauseCheckBox";
            this.pauseCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.pauseCheckBox.Size = new System.Drawing.Size(58, 17);
            this.pauseCheckBox.TabIndex = 6;
            this.pauseCheckBox.Text = ":pause";
            this.pauseCheckBox.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 69);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.outputRichTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser);
            this.splitContainer1.Panel2.Controls.Add(this.outputBestRichTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(565, 190);
            this.splitContainer1.SplitterDistance = 95;
            this.splitContainer1.TabIndex = 7;
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(136, 0);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(429, 91);
            this.webBrowser.TabIndex = 6;
            this.webBrowser.Url = new System.Uri("", System.UriKind.Relative);
            //this.webBrowser.Url = new System.Uri("http://10.0.0.26:4747/video", System.UriKind.Absolute);
            // 
            // subtitlesLangLabel
            // 
            this.subtitlesLangLabel.AutoSize = true;
            this.subtitlesLangLabel.Location = new System.Drawing.Point(94, 43);
            this.subtitlesLangLabel.Name = "subtitlesLangLabel";
            this.subtitlesLangLabel.Size = new System.Drawing.Size(97, 13);
            this.subtitlesLangLabel.TabIndex = 8;
            this.subtitlesLangLabel.Text = "Subtitles language:";
            // 
            // lang_ComboBox
            // 
            this.lang_ComboBox.FormattingEnabled = true;
            this.lang_ComboBox.Items.AddRange(new object[] {
            "heb",
            "eng",
            "rum"});
            this.lang_ComboBox.Location = new System.Drawing.Point(198, 39);
            this.lang_ComboBox.Name = "lang_ComboBox";
            this.lang_ComboBox.Size = new System.Drawing.Size(49, 21);
            this.lang_ComboBox.TabIndex = 9;
            // 
            // skip_skytorrents_scraping_CheckBox
            // 
            this.skip_skytorrents_scraping_CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skip_skytorrents_scraping_CheckBox.AutoSize = true;
            this.skip_skytorrents_scraping_CheckBox.Location = new System.Drawing.Point(345, 39);
            this.skip_skytorrents_scraping_CheckBox.Name = "skip_skytorrents_scraping_CheckBox";
            this.skip_skytorrents_scraping_CheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.skip_skytorrents_scraping_CheckBox.Size = new System.Drawing.Size(151, 17);
            this.skip_skytorrents_scraping_CheckBox.TabIndex = 10;
            this.skip_skytorrents_scraping_CheckBox.Text = ":skip_skytorrents_scraping";
            this.skip_skytorrents_scraping_CheckBox.UseVisualStyleBackColor = true;
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(15, 37);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(49, 23);
            this.resetButton.TabIndex = 11;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 262);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.skip_skytorrents_scraping_CheckBox);
            this.Controls.Add(this.lang_ComboBox);
            this.Controls.Add(this.subtitlesLangLabel);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pauseCheckBox);
            this.Controls.Add(this.movie_or_Series_Episode_Label);
            this.Controls.Add(this.run_Button);
            this.Controls.Add(this.movie_or_Series_Episode_TextBox);
            this.Name = "MainForm";
            this.Text = "Get Subbed Video";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox movie_or_Series_Episode_TextBox;
        private System.Windows.Forms.Button run_Button;
        private System.Windows.Forms.Label movie_or_Series_Episode_Label;
        private System.Windows.Forms.RichTextBox outputRichTextBox;
        private System.Windows.Forms.RichTextBox outputBestRichTextBox;
        private System.Windows.Forms.CheckBox pauseCheckBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label subtitlesLangLabel;
        private System.Windows.Forms.ComboBox lang_ComboBox;
        private System.Windows.Forms.CheckBox skip_skytorrents_scraping_CheckBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.WebBrowser webBrowser;
    }
}

