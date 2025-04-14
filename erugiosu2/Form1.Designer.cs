namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new System.Windows.Forms.PictureBox();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            outputTextBox = new System.Windows.Forms.TextBox();
            copyButton = new System.Windows.Forms.Button();
            OutputLabel = new System.Windows.Forms.Label();
            InputTimer = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            DebugTextBox = new System.Windows.Forms.TextBox();
            DebugLabel = new System.Windows.Forms.Label();
            showDebugCheckBox = new System.Windows.Forms.CheckBox();
            OutputVisible = new System.Windows.Forms.CheckBox();
            TableOnlyVisible = new System.Windows.Forms.CheckBox();
            TimeVisible = new System.Windows.Forms.CheckBox();
            ConsoleButton = new System.Windows.Forms.Button();
            AutoLiveSplitEnabled = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new System.Drawing.Point(14, 15);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(280, 225);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new System.Drawing.Point(14, 248);
            dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowTemplate.Height = 21;
            dataGridView1.Size = new System.Drawing.Size(653, 232);
            dataGridView1.TabIndex = 10;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // outputTextBox
            // 
            outputTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            outputTextBox.Location = new System.Drawing.Point(438, 42);
            outputTextBox.Margin = new System.Windows.Forms.Padding(4);
            outputTextBox.Multiline = true;
            outputTextBox.Name = "outputTextBox";
            outputTextBox.ReadOnly = true;
            outputTextBox.Size = new System.Drawing.Size(254, 80);
            outputTextBox.TabIndex = 13;
            // 
            // copyButton
            // 
            copyButton.Location = new System.Drawing.Point(344, 61);
            copyButton.Margin = new System.Windows.Forms.Padding(4);
            copyButton.Name = "copyButton";
            copyButton.Size = new System.Drawing.Size(88, 40);
            copyButton.TabIndex = 14;
            copyButton.Text = "OutputCopy";
            copyButton.UseVisualStyleBackColor = true;
            copyButton.Click += copyButton_Click_1;
            // 
            // OutputLabel
            // 
            OutputLabel.AutoSize = true;
            OutputLabel.Location = new System.Drawing.Point(438, 22);
            OutputLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            OutputLabel.Name = "OutputLabel";
            OutputLabel.Size = new System.Drawing.Size(45, 15);
            OutputLabel.TabIndex = 15;
            OutputLabel.Text = "Output";
            OutputLabel.Click += label1_Click;
            // 
            // InputTimer
            // 
            InputTimer.BackColor = System.Drawing.SystemColors.ControlLightLight;
            InputTimer.Location = new System.Drawing.Point(299, 30);
            InputTimer.Margin = new System.Windows.Forms.Padding(4);
            InputTimer.Name = "InputTimer";
            InputTimer.Size = new System.Drawing.Size(131, 23);
            InputTimer.TabIndex = 16;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(299, 9);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(72, 15);
            label2.TabIndex = 17;
            label2.Text = "Timer(h m s)";
            label2.Click += label2_Click;
            // 
            // DebugTextBox
            // 
            DebugTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            DebugTextBox.Location = new System.Drawing.Point(440, 145);
            DebugTextBox.Margin = new System.Windows.Forms.Padding(4);
            DebugTextBox.Multiline = true;
            DebugTextBox.Name = "DebugTextBox";
            DebugTextBox.ReadOnly = true;
            DebugTextBox.Size = new System.Drawing.Size(254, 94);
            DebugTextBox.TabIndex = 18;
            // 
            // DebugLabel
            // 
            DebugLabel.AutoSize = true;
            DebugLabel.Location = new System.Drawing.Point(438, 127);
            DebugLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            DebugLabel.Name = "DebugLabel";
            DebugLabel.Size = new System.Drawing.Size(42, 15);
            DebugLabel.TabIndex = 19;
            DebugLabel.Text = "Debug";
            DebugLabel.Click += label3_Click;
            // 
            // showDebugCheckBox
            // 
            showDebugCheckBox.AutoSize = true;
            showDebugCheckBox.Location = new System.Drawing.Point(301, 221);
            showDebugCheckBox.Margin = new System.Windows.Forms.Padding(4);
            showDebugCheckBox.Name = "showDebugCheckBox";
            showDebugCheckBox.Size = new System.Drawing.Size(95, 19);
            showDebugCheckBox.TabIndex = 20;
            showDebugCheckBox.Text = "DebugVisible";
            showDebugCheckBox.UseVisualStyleBackColor = true;
            showDebugCheckBox.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // OutputVisible
            // 
            OutputVisible.AutoSize = true;
            OutputVisible.Location = new System.Drawing.Point(302, 166);
            OutputVisible.Margin = new System.Windows.Forms.Padding(4);
            OutputVisible.Name = "OutputVisible";
            OutputVisible.Size = new System.Drawing.Size(98, 19);
            OutputVisible.TabIndex = 21;
            OutputVisible.Text = "OutputVisible";
            OutputVisible.UseVisualStyleBackColor = true;
            OutputVisible.CheckedChanged += OutputVisible_CheckedChanged;
            // 
            // TableOnlyVisible
            // 
            TableOnlyVisible.AutoSize = true;
            TableOnlyVisible.Location = new System.Drawing.Point(491, 14);
            TableOnlyVisible.Margin = new System.Windows.Forms.Padding(4);
            TableOnlyVisible.Name = "TableOnlyVisible";
            TableOnlyVisible.Size = new System.Drawing.Size(78, 19);
            TableOnlyVisible.TabIndex = 22;
            TableOnlyVisible.Text = "TableOnly";
            TableOnlyVisible.UseVisualStyleBackColor = true;
            TableOnlyVisible.CheckedChanged += TimerVisible_CheckedChanged;
            // 
            // TimeVisible
            // 
            TimeVisible.AutoSize = true;
            TimeVisible.Location = new System.Drawing.Point(301, 193);
            TimeVisible.Margin = new System.Windows.Forms.Padding(4);
            TimeVisible.Name = "TimeVisible";
            TimeVisible.Size = new System.Drawing.Size(85, 19);
            TimeVisible.TabIndex = 23;
            TimeVisible.Text = "TimeVisible";
            TimeVisible.UseVisualStyleBackColor = true;
            TimeVisible.CheckedChanged += checkBox1_CheckedChanged_1;
            // 
            // ConsoleButton
            // 
            ConsoleButton.Location = new System.Drawing.Point(344, 110);
            ConsoleButton.Margin = new System.Windows.Forms.Padding(4);
            ConsoleButton.Name = "ConsoleButton";
            ConsoleButton.Size = new System.Drawing.Size(88, 48);
            ConsoleButton.TabIndex = 24;
            ConsoleButton.Text = "Console";
            ConsoleButton.UseVisualStyleBackColor = true;
            ConsoleButton.Click += button1_Click;
            // 
            // AutoLiveSplitEnabled
            // 
            AutoLiveSplitEnabled.AutoSize = true;
            AutoLiveSplitEnabled.Checked = true;
            AutoLiveSplitEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            AutoLiveSplitEnabled.Location = new System.Drawing.Point(576, 14);
            AutoLiveSplitEnabled.Name = "AutoLiveSplitEnabled";
            AutoLiveSplitEnabled.Size = new System.Drawing.Size(96, 19);
            AutoLiveSplitEnabled.TabIndex = 25;
            AutoLiveSplitEnabled.Text = "AutoLiveSplit";
            AutoLiveSplitEnabled.UseVisualStyleBackColor = true;
            AutoLiveSplitEnabled.CheckedChanged += checkBox1_CheckedChanged_2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(716, 491);
            Controls.Add(AutoLiveSplitEnabled);
            Controls.Add(ConsoleButton);
            Controls.Add(TimeVisible);
            Controls.Add(TableOnlyVisible);
            Controls.Add(OutputVisible);
            Controls.Add(showDebugCheckBox);
            Controls.Add(DebugLabel);
            Controls.Add(DebugTextBox);
            Controls.Add(label2);
            Controls.Add(InputTimer);
            Controls.Add(OutputLabel);
            Controls.Add(copyButton);
            Controls.Add(outputTextBox);
            Controls.Add(dataGridView1);
            Controls.Add(pictureBox1);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Label OutputLabel;
        private System.Windows.Forms.TextBox InputTimer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DebugTextBox;
        private System.Windows.Forms.Label DebugLabel;
        private System.Windows.Forms.CheckBox showDebugCheckBox;
        private System.Windows.Forms.CheckBox OutputVisible;
        private System.Windows.Forms.CheckBox TableOnlyVisible;
        private System.Windows.Forms.CheckBox TimeVisible;
        private System.Windows.Forms.Button ConsoleButton;
        private System.Windows.Forms.CheckBox AutoLiveSplitEnabled;
    }
}

