namespace 视频番剧爬取器
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
            AddButton = new Button();
            AddressText = new TextBox();
            videoP = new ProgressBar();
            allP = new ProgressBar();
            Down = new Button();
            videoList = new ListBox();
            downP = new ProgressBar();
            timer1 = new System.Windows.Forms.Timer(components);
            downSl = new Label();
            downPl = new Label();
            videoPl = new Label();
            allPl = new Label();
            cinmel = new CheckBox();
            CheckVideo = new Button();
            LogBox = new ListBox();
            CompleteCheck = new CheckBox();
            SuspendLayout();
            // 
            // AddButton
            // 
            AddButton.Location = new Point(600, 1);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(74, 35);
            AddButton.TabIndex = 0;
            AddButton.Text = "添加";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // AddressText
            // 
            AddressText.Location = new Point(3, 5);
            AddressText.Name = "AddressText";
            AddressText.Size = new Size(434, 27);
            AddressText.TabIndex = 1;
            // 
            // videoP
            // 
            videoP.Location = new Point(109, 432);
            videoP.Name = "videoP";
            videoP.Size = new Size(565, 27);
            videoP.TabIndex = 3;
            // 
            // allP
            // 
            allP.Location = new Point(109, 462);
            allP.Name = "allP";
            allP.Size = new Size(565, 31);
            allP.TabIndex = 4;
            // 
            // Down
            // 
            Down.Location = new Point(3, 285);
            Down.Name = "Down";
            Down.Size = new Size(671, 42);
            Down.TabIndex = 5;
            Down.Text = "下载";
            Down.UseVisualStyleBackColor = true;
            Down.Click += Down_Click;
            // 
            // videoList
            // 
            videoList.FormattingEnabled = true;
            videoList.ItemHeight = 20;
            videoList.Location = new Point(3, 38);
            videoList.Name = "videoList";
            videoList.Size = new Size(671, 244);
            videoList.TabIndex = 9;
            // 
            // downP
            // 
            downP.Location = new Point(3, 399);
            downP.Name = "downP";
            downP.Size = new Size(671, 27);
            downP.TabIndex = 3;
            // 
            // downSl
            // 
            downSl.AutoSize = true;
            downSl.Location = new Point(3, 376);
            downSl.Name = "downSl";
            downSl.Size = new Size(0, 20);
            downSl.TabIndex = 10;
            // 
            // downPl
            // 
            downPl.AutoSize = true;
            downPl.Location = new Point(286, 376);
            downPl.Name = "downPl";
            downPl.Size = new Size(0, 20);
            downPl.TabIndex = 11;
            downPl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // videoPl
            // 
            videoPl.AutoSize = true;
            videoPl.Location = new Point(3, 439);
            videoPl.Name = "videoPl";
            videoPl.Size = new Size(0, 20);
            videoPl.TabIndex = 12;
            videoPl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // allPl
            // 
            allPl.AutoSize = true;
            allPl.Location = new Point(3, 473);
            allPl.Name = "allPl";
            allPl.Size = new Size(0, 20);
            allPl.TabIndex = 13;
            allPl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cinmel
            // 
            cinmel.AutoSize = true;
            cinmel.Location = new Point(443, 8);
            cinmel.Name = "cinmel";
            cinmel.Size = new Size(151, 24);
            cinmel.TabIndex = 14;
            cinmel.Text = "剧场版或者单文件";
            cinmel.UseVisualStyleBackColor = true;
            // 
            // CheckVideo
            // 
            CheckVideo.Location = new Point(443, 330);
            CheckVideo.Name = "CheckVideo";
            CheckVideo.Size = new Size(231, 44);
            CheckVideo.TabIndex = 16;
            CheckVideo.Text = "检查更新";
            CheckVideo.UseVisualStyleBackColor = true;
            CheckVideo.Click += CheckVideo_Click;
            // 
            // LogBox
            // 
            LogBox.FormattingEnabled = true;
            LogBox.ItemHeight = 20;
            LogBox.Location = new Point(3, 330);
            LogBox.Name = "LogBox";
            LogBox.Size = new Size(438, 44);
            LogBox.TabIndex = 17;
            // 
            // CompleteCheck
            // 
            CompleteCheck.AutoSize = true;
            CompleteCheck.Font = new Font("Microsoft YaHei UI", 6.60000038F, FontStyle.Regular, GraphicsUnit.Point);
            CompleteCheck.Location = new Point(583, 372);
            CompleteCheck.Name = "CompleteCheck";
            CompleteCheck.Size = new Size(78, 21);
            CompleteCheck.TabIndex = 18;
            CompleteCheck.Text = "已经完结";
            CompleteCheck.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 495);
            Controls.Add(CompleteCheck);
            Controls.Add(LogBox);
            Controls.Add(CheckVideo);
            Controls.Add(cinmel);
            Controls.Add(allPl);
            Controls.Add(videoPl);
            Controls.Add(downPl);
            Controls.Add(downSl);
            Controls.Add(videoList);
            Controls.Add(Down);
            Controls.Add(allP);
            Controls.Add(downP);
            Controls.Add(videoP);
            Controls.Add(AddressText);
            Controls.Add(AddButton);
            Name = "Form1";
            Text = "番剧下载器";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button AddButton;
        private TextBox AddressText;
        private Button Down;
        private ListBox videoList;
        private ProgressBar downP;
        private System.Windows.Forms.Timer timer1;
        private ProgressBar videoP;
        private ProgressBar allP;
        private Label downSl;
        private Label downPl;
        private Label videoPl;
        private Label allPl;
        private CheckBox cinmel;
        private Button CheckVideo;
        private ListBox LogBox;
        private CheckBox CompleteCheck;
    }
}
