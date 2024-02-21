using PopupApp.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PopupApp
{
    public partial class NotificationPopup : Form
    {
        public NotificationPopup()
        {
            InitializeComponent();
            //MaximizeBox = false;
        }

        private string connectionString = @"Data Source=BLR-3ZY31F3\SQLEXPRESS;Initial Catalog=SignalRdb;Integrated Security=True";
        private string msgId = string.Empty;
        private string msgTitle = string.Empty;
        private string msgMessage = string.Empty;
        private string oldMsgId = "";
        private bool stopCn = false;
        private void NotificationPopup_Load(object sender, EventArgs e)
        {
            SqlClientPermission sqlClientPermission = new SqlClientPermission(System.Security.Permissions.PermissionState.Unrestricted);
            sqlClientPermission.Demand();
            startBtn.Enabled = false;
            stopBtn.Enabled = true;
            snoozeBtn.Enabled = true;
            comboBox2.Enabled = true;
            LoadData();

        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.btnHide_Click(null, null);
        }
        private void hide()
        {
            this.Visible = false;
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = new Icon(@"C:\Users\Prakash.K.CORPBRISTLECONE\Downloads\notification.ico");
            trayIcon.Visible = true;
            trayIcon.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            var thisIcon = (NotifyIcon)sender;
            thisIcon.Visible = false;
            thisIcon.Dispose();
        }
        void LoadData()
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }
                if (stopCn == true)
                {
                    cn.Close();
                    SqlDependency.Stop(connectionString);
                }
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM POPUPAPP ORDER BY MESSAGEDATE DESC", cn);
                cmd.Notification = null;
                SqlDependency.Start(connectionString);
                SqlDependency sqlDependency = new SqlDependency(cmd);
                sqlDependency.OnChange += new OnChangeEventHandler(OnDependencyChange);
                DataTable dt = new DataTable("MessageTable");
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    msgId = dt.Rows[0]["MESSAGEID"].ToString();
                    msgTitle = dt.Rows[0]["TITLE"].ToString();
                    msgMessage = dt.Rows[0]["MESSAGE"].ToString();
                }
                if (msgId != oldMsgId && oldMsgId != "")
                {
                    string message = msgMessage;
                    string title = msgTitle;
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                oldMsgId = msgId;
            }
        }
        delegate void UpdateData();
        public void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency sqlDependency = sender as SqlDependency;
            sqlDependency.OnChange -= OnDependencyChange;
            UpdateData updateData = new UpdateData(LoadData);
            this.Invoke(updateData, null);
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            stopCn = true;
            startBtn.Enabled = true;
            stopBtn.Enabled = false;
            snoozeBtn.Enabled = false;
            comboBox2.Enabled = false;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            hide();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            stopCn = false;
            startBtn.Enabled = false;
            stopBtn.Enabled = true;
            snoozeBtn.Enabled = true;
            comboBox2.Enabled = true;
            LoadData();
        }

        private async void snoozeBtn_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt16(comboBox2.SelectedItem);
            int hourDelay = val * 60 * 60000;
            stopCn = true;
            startBtn.Enabled = true;
            stopBtn.Enabled = false;
            snoozeBtn.Enabled = false;
            comboBox2.Enabled = false;
            hide();
            //await Task.Delay(10000);
            await PutTaskDelay(hourDelay);
            startBtn_Click(null, null);
        }
        async Task PutTaskDelay(int snoozeDelay)
        {
            await Task.Delay(snoozeDelay);
        }
    }
}
