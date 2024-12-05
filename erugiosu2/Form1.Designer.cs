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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.copyButton = new System.Windows.Forms.Button();
            this.OutputLabel = new System.Windows.Forms.Label();
            this.InputTimer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.DebugTextBox = new System.Windows.Forms.TextBox();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.showDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.OutputVisible = new System.Windows.Forms.CheckBox();
            this.TableOnlyVisible = new System.Windows.Forms.CheckBox();
            this.TimeVisible = new System.Windows.Forms.CheckBox();
            this.ConsoleButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(240, 180);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 198);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(560, 186);
            this.dataGridView1.TabIndex = 10;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // outputTextBox
            // 
            this.outputTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.outputTextBox.Location = new System.Drawing.Point(377, 33);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.Size = new System.Drawing.Size(218, 65);
            this.outputTextBox.TabIndex = 13;
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(296, 50);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(75, 32);
            this.copyButton.TabIndex = 14;
            this.copyButton.Text = "OutputCopy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click_1);
            // 
            // OutputLabel
            // 
            this.OutputLabel.AutoSize = true;
            this.OutputLabel.Location = new System.Drawing.Point(375, 18);
            this.OutputLabel.Name = "OutputLabel";
            this.OutputLabel.Size = new System.Drawing.Size(39, 12);
            this.OutputLabel.TabIndex = 15;
            this.OutputLabel.Text = "Output";
            this.OutputLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // InputTimer
            // 
            this.InputTimer.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.InputTimer.Location = new System.Drawing.Point(258, 24);
            this.InputTimer.Name = "InputTimer";
            this.InputTimer.Size = new System.Drawing.Size(113, 19);
            this.InputTimer.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(258, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 17;
            this.label2.Text = "Timer(h m s)";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // DebugTextBox
            // 
            this.DebugTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.DebugTextBox.Location = new System.Drawing.Point(377, 116);
            this.DebugTextBox.Multiline = true;
            this.DebugTextBox.Name = "DebugTextBox";
            this.DebugTextBox.ReadOnly = true;
            this.DebugTextBox.Size = new System.Drawing.Size(218, 76);
            this.DebugTextBox.TabIndex = 18;
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(375, 101);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(37, 12);
            this.DebugLabel.TabIndex = 19;
            this.DebugLabel.Text = "Debug";
            this.DebugLabel.Click += new System.EventHandler(this.label3_Click);
            // 
            // showDebugCheckBox
            // 
            this.showDebugCheckBox.AutoSize = true;
            this.showDebugCheckBox.Location = new System.Drawing.Point(260, 176);
            this.showDebugCheckBox.Name = "showDebugCheckBox";
            this.showDebugCheckBox.Size = new System.Drawing.Size(91, 16);
            this.showDebugCheckBox.TabIndex = 20;
            this.showDebugCheckBox.Text = "DebugVisible";
            this.showDebugCheckBox.UseVisualStyleBackColor = true;
            this.showDebugCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // OutputVisible
            // 
            this.OutputVisible.AutoSize = true;
            this.OutputVisible.Location = new System.Drawing.Point(260, 132);
            this.OutputVisible.Name = "OutputVisible";
            this.OutputVisible.Size = new System.Drawing.Size(93, 16);
            this.OutputVisible.TabIndex = 21;
            this.OutputVisible.Text = "OutputVisible";
            this.OutputVisible.UseVisualStyleBackColor = true;
            this.OutputVisible.CheckedChanged += new System.EventHandler(this.OutputVisible_CheckedChanged);
            // 
            // TableOnlyVisible
            // 
            this.TableOnlyVisible.AutoSize = true;
            this.TableOnlyVisible.Location = new System.Drawing.Point(420, 12);
            this.TableOnlyVisible.Name = "TableOnlyVisible";
            this.TableOnlyVisible.Size = new System.Drawing.Size(75, 16);
            this.TableOnlyVisible.TabIndex = 22;
            this.TableOnlyVisible.Text = "TableOnly";
            this.TableOnlyVisible.UseVisualStyleBackColor = true;
            this.TableOnlyVisible.CheckedChanged += new System.EventHandler(this.TimerVisible_CheckedChanged);
            // 
            // TimeVisible
            // 
            this.TimeVisible.AutoSize = true;
            this.TimeVisible.Location = new System.Drawing.Point(260, 154);
            this.TimeVisible.Name = "TimeVisible";
            this.TimeVisible.Size = new System.Drawing.Size(84, 16);
            this.TimeVisible.TabIndex = 23;
            this.TimeVisible.Text = "TimeVisible";
            this.TimeVisible.UseVisualStyleBackColor = true;
            this.TimeVisible.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // ConsoleButton
            // 
            this.ConsoleButton.Location = new System.Drawing.Point(296, 88);
            this.ConsoleButton.Name = "ConsoleButton";
            this.ConsoleButton.Size = new System.Drawing.Size(75, 38);
            this.ConsoleButton.TabIndex = 24;
            this.ConsoleButton.Text = "Console";
            this.ConsoleButton.UseVisualStyleBackColor = true;
            this.ConsoleButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 393);
            this.Controls.Add(this.ConsoleButton);
            this.Controls.Add(this.TimeVisible);
            this.Controls.Add(this.TableOnlyVisible);
            this.Controls.Add(this.OutputVisible);
            this.Controls.Add(this.showDebugCheckBox);
            this.Controls.Add(this.DebugLabel);
            this.Controls.Add(this.DebugTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.InputTimer);
            this.Controls.Add(this.OutputLabel);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}

