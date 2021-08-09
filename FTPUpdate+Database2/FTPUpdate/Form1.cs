using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using Microsoft.Win32;

namespace FTPUpdate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        struct FtpSetting
        {
            public string Server { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string FileName { get; set;  }
            public string FullName { get; set; }
        
        }

        FtpSetting _inputParameter;

       

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string controlFTP = txtServer.Text.Substring(0, 6);
            if (controlFTP!= "ftp://")
            {
                MessageBox.Show("Hatalı Format Girişi", "Bilgilendirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else
            {


                string fileName = ((FtpSetting)e.Argument).FileName;
                string fullName = ((FtpSetting)e.Argument).FullName;
                string userName = ((FtpSetting)e.Argument).UserName;
                string password = ((FtpSetting)e.Argument).Password;
                string server = ((FtpSetting)e.Argument).Server;


                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", server, fileName)));
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(userName, password);
                    request.GetResponse();

                    Stream ftpStream = request.GetRequestStream();
                    FileStream fs = File.OpenRead(fullName);
                    byte[] buffer = new byte[1024];
                    double total = (double)fs.Length;
                    int byteRead = 0;
                    double read = 0;
                    do
                    {
                        if (!backgroundWorker.CancellationPending)
                        {
                            byteRead = fs.Read(buffer, 0, 1024);
                            ftpStream.Write(buffer, 0, byteRead);
                            read += (double)byteRead;
                            double percentage = read / total * 100;
                            backgroundWorker.ReportProgress((int)percentage);
                        }
                    }
                    while (byteRead != 0);
                    fs.Close();
                    ftpStream.Close();

                    string baglantiyol = "Data source = FTPdatabase.db";
                    SQLiteConnection baglanti = new SQLiteConnection(baglantiyol);
                    baglanti.Open();

                    string sql = "insert into successTable(serverName,userName,password,date) values (@serverName,@userName,@password,@date)";

                    SQLiteCommand komutIslet = new SQLiteCommand(sql, baglanti);
                    komutIslet.Parameters.AddWithValue("@serverName", txtServer.Text);
                    komutIslet.Parameters.AddWithValue("@userName", txtUserName.Text);
                    komutIslet.Parameters.AddWithValue("@password", txtPassword.Text);
                    komutIslet.Parameters.AddWithValue("@date", dateTimePicker1.Value.ToString());

                    komutIslet.ExecuteNonQuery();
                    baglanti.Dispose();
                    komutIslet.Dispose();
                    MessageBox.Show("Transfer işlemi başarılı olan Server bilgileri kaydedildi.");

                }
                catch (WebException ex)
                {

                    string baglantiyol2 = "Data source = FTPdatabase.db";
                    SQLiteConnection baglanti = new SQLiteConnection(baglantiyol2);
                    baglanti.Open();

                    string sql = "insert into errorTable(serverName,userName,password,date) values (@serverName,@userName,@password,@date)";

                    SQLiteCommand komutIslet = new SQLiteCommand(sql, baglanti);
                    komutIslet.Parameters.AddWithValue("@serverName", txtServer.Text);
                    komutIslet.Parameters.AddWithValue("@userName", txtUserName.Text);
                    komutIslet.Parameters.AddWithValue("@password", txtPassword.Text);
                    komutIslet.Parameters.AddWithValue("@date", dateTimePicker1.Value.ToString());

                    komutIslet.ExecuteNonQuery();
                    baglanti.Dispose();
                    komutIslet.Dispose();
                    MessageBox.Show("Bağlanılmak istenilen Server Bilgileri Hatalı!");

                    Application.Exit();

                }
            }   
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
            lblStatus.Text = $"Uploaded {e.ProgressPercentage} %";
            progressBar.Value = e.ProgressPercentage;
            progressBar.Update();

        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            lblStatus.Text = "Upload complete !";

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd=new OpenFileDialog() {Multiselect = false, ValidateNames= true, Filter="All files|*.*"})
            {
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                   
                    FileInfo fi = new FileInfo(ofd.FileName);
                    _inputParameter.UserName = txtUserName.Text;
                    _inputParameter.Password = txtPassword.Text;
                    _inputParameter.Server = txtServer.Text;
                    _inputParameter.FileName = fi.Name;
                    _inputParameter.FullName = fi.FullName;
                    backgroundWorker.RunWorkerAsync(_inputParameter);

                }

            }

            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("Oto Baslat", Application.ExecutablePath.ToString());
            MessageBox.Show("Program bilgisayar açıldığında otomatik başlatmaya ayarlanmıştır.", "Bilgilendirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
