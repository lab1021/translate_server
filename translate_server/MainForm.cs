using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace translate_server
{
    public partial class MainForm : Form
    {
        private static TransportOperation T;
        public static ConfigurationOperation ConfigOp;
        public MainForm()
        {
            InitializeComponent();
            SetServerip();
        }
        public void SetServerip()
        {
            IPAddress []ipaddr = Dns.GetHostAddresses(Dns.GetHostName());//取得本机IP
            foreach (var item in ipaddr)
            {
                if(item.AddressFamily == AddressFamily.InterNetwork)
                    this.comboBox_ip.Items.Add(item);
            }
            this.comboBox_ip.SelectedItem = this.comboBox_ip.Items[0];
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            string ipaddr;
            int port;
            int max_thread;
            try
            {
                if (((IPAddress)this.comboBox_ip.SelectedItem).AddressFamily != AddressFamily.InterNetwork)
                {
                    this.label_warn_ip.Text = "ip地址设置错误:***.***.***.***";
                    return;
                }
                ipaddr = this.comboBox_ip.SelectedItem.ToString();
                this.label_warn_ip.Text = "";
            }
            catch (Exception)
            {
                this.label_warn_ip.Text = "ip地址设置错误:***.***.***.***";
                return;
            }
            try
            {
                if (this.textBox_port.Text == "" || this.textBox_port == null)
                {
                    this.label_warn_port.Text = "端口号不能为空";
                    return;
                }
                port = Int32.Parse(this.textBox_port.Text.ToString());
                this.label_warn_port.Text = "";
            }
            catch (Exception)
            {
                this.label_warn_port.Text = "端口号必须为数字！";
                return;
            }

            max_thread = Decimal.ToInt32(this.numericUpDown_max_thread.Value);

            ConfigOp = new ConfigurationOperation(@"F:\projects\C#\projects\translate_server\translate_server\app_data\");

            ConfigOp.Load();

            T = new TransportOperation(ipaddr, port, 50, max_thread);
            this.comboBox_ip.Enabled = false;
            this.textBox_port.Enabled = false;
            this.numericUpDown_max_thread.Enabled = false;

            T.Run();

            this.button_start.Enabled = false;
            this.button_stop.Enabled = true;
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            T.Stop();
            this.comboBox_ip.Enabled = true;
            this.textBox_port.Enabled = true;
            this.numericUpDown_max_thread.Enabled = true;
            this.button_start.Enabled = true;
            this.button_stop.Enabled = false;
        }
    }
}
