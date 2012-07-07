namespace translate_server
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label_ip = new System.Windows.Forms.Label();
            this.label_port = new System.Windows.Forms.Label();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.label_max_thread = new System.Windows.Forms.Label();
            this.textBox_state = new System.Windows.Forms.TextBox();
            this.numericUpDown_max_thread = new System.Windows.Forms.NumericUpDown();
            this.comboBox_ip = new System.Windows.Forms.ComboBox();
            this.button_start = new System.Windows.Forms.Button();
            this.label_warn_ip = new System.Windows.Forms.Label();
            this.label_duration_port = new System.Windows.Forms.Label();
            this.label_warn_port = new System.Windows.Forms.Label();
            this.button_stop = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_max_thread)).BeginInit();
            this.SuspendLayout();
            // 
            // label_ip
            // 
            this.label_ip.AutoSize = true;
            this.label_ip.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_ip.Location = new System.Drawing.Point(44, 49);
            this.label_ip.Name = "label_ip";
            this.label_ip.Size = new System.Drawing.Size(63, 22);
            this.label_ip.TabIndex = 0;
            this.label_ip.Text = "本机IP:";
            // 
            // label_port
            // 
            this.label_port.AutoSize = true;
            this.label_port.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_port.Location = new System.Drawing.Point(60, 106);
            this.label_port.Name = "label_port";
            this.label_port.Size = new System.Drawing.Size(47, 22);
            this.label_port.TabIndex = 2;
            this.label_port.Text = "端口:";
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(119, 106);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(104, 21);
            this.textBox_port.TabIndex = 3;
            this.textBox_port.Text = "9050";
            // 
            // label_max_thread
            // 
            this.label_max_thread.AutoSize = true;
            this.label_max_thread.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_max_thread.Location = new System.Drawing.Point(12, 177);
            this.label_max_thread.Name = "label_max_thread";
            this.label_max_thread.Size = new System.Drawing.Size(95, 22);
            this.label_max_thread.TabIndex = 4;
            this.label_max_thread.Text = "最大线程数:";
            // 
            // textBox_state
            // 
            this.textBox_state.Location = new System.Drawing.Point(68, 244);
            this.textBox_state.Multiline = true;
            this.textBox_state.Name = "textBox_state";
            this.textBox_state.Size = new System.Drawing.Size(528, 170);
            this.textBox_state.TabIndex = 6;
            // 
            // numericUpDown_max_thread
            // 
            this.numericUpDown_max_thread.Location = new System.Drawing.Point(119, 177);
            this.numericUpDown_max_thread.Name = "numericUpDown_max_thread";
            this.numericUpDown_max_thread.Size = new System.Drawing.Size(104, 21);
            this.numericUpDown_max_thread.TabIndex = 7;
            this.numericUpDown_max_thread.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // comboBox_ip
            // 
            this.comboBox_ip.FormattingEnabled = true;
            this.comboBox_ip.Location = new System.Drawing.Point(119, 49);
            this.comboBox_ip.Name = "comboBox_ip";
            this.comboBox_ip.Size = new System.Drawing.Size(257, 20);
            this.comboBox_ip.TabIndex = 8;
            // 
            // button_start
            // 
            this.button_start.Location = new System.Drawing.Point(331, 175);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(75, 23);
            this.button_start.TabIndex = 9;
            this.button_start.Text = "启动";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // label_warn_ip
            // 
            this.label_warn_ip.AutoSize = true;
            this.label_warn_ip.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_warn_ip.ForeColor = System.Drawing.Color.Red;
            this.label_warn_ip.Location = new System.Drawing.Point(406, 49);
            this.label_warn_ip.Name = "label_warn_ip";
            this.label_warn_ip.Size = new System.Drawing.Size(0, 16);
            this.label_warn_ip.TabIndex = 10;
            // 
            // label_duration_port
            // 
            this.label_duration_port.AutoSize = true;
            this.label_duration_port.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_duration_port.Location = new System.Drawing.Point(116, 130);
            this.label_duration_port.Name = "label_duration_port";
            this.label_duration_port.Size = new System.Drawing.Size(113, 12);
            this.label_duration_port.TabIndex = 11;
            this.label_duration_port.Text = "(范围：1025-65535)";
            // 
            // label_warn_port
            // 
            this.label_warn_port.AutoSize = true;
            this.label_warn_port.ForeColor = System.Drawing.Color.Red;
            this.label_warn_port.Location = new System.Drawing.Point(255, 106);
            this.label_warn_port.Name = "label_warn_port";
            this.label_warn_port.Size = new System.Drawing.Size(0, 12);
            this.label_warn_port.TabIndex = 12;
            // 
            // button_stop
            // 
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(481, 177);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(75, 23);
            this.button_stop.TabIndex = 13;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 446);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.label_warn_port);
            this.Controls.Add(this.label_duration_port);
            this.Controls.Add(this.label_warn_ip);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.comboBox_ip);
            this.Controls.Add(this.numericUpDown_max_thread);
            this.Controls.Add(this.textBox_state);
            this.Controls.Add(this.label_max_thread);
            this.Controls.Add(this.textBox_port);
            this.Controls.Add(this.label_port);
            this.Controls.Add(this.label_ip);
            this.Name = "MainForm";
            this.Text = "translate_server";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_max_thread)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_ip;
        private System.Windows.Forms.Label label_port;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.Label label_max_thread;
        private System.Windows.Forms.TextBox textBox_state;
        private System.Windows.Forms.NumericUpDown numericUpDown_max_thread;
        private System.Windows.Forms.ComboBox comboBox_ip;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Label label_warn_ip;
        private System.Windows.Forms.Label label_duration_port;
        private System.Windows.Forms.Label label_warn_port;
        private System.Windows.Forms.Button button_stop;
    }
}

