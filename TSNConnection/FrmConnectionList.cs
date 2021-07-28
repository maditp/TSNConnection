using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace TSNConnection
{
    public partial class FrmConnectionList : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        public static string DIR_REG_CONNECTION = "HKEY_CURRENT_USER\\SOFTWARE\\TSNConnection\\";

        public static string AppExe = "MADITP2.0.exe";

        DataTable dt;
        string sConnName = "";
        string sConnDesc = "";
        string sConnEntity = "";
        string sKeyReg = "";
        string sKeySeq = "";

        public FrmConnectionList()
        {
            InitializeComponent();
        }

        private void FrmConnectionList_Load(object sender, EventArgs e)
        {
            //dt = new DataTable();
            //dt.Columns.Add("ConnName");
            //dt.Columns.Add("ConnDesc");
            //dt.Columns.Add("ConnEntity");
            //dt.Columns.Add("ConnKeyReg");

            GetList();
            BtnOpen.Select();
            
        }
        public void GetList()
        {
            dt = new DataTable();
            dt.Columns.Add("ConnName");
            dt.Columns.Add("ConnDesc");
            dt.Columns.Add("ConnEntity");
            dt.Columns.Add("ConnKeyReg");
            dt.Columns.Add("ConnSeq");

            dt.Rows.Clear();

            sConnName = "";
            sKeyReg = "";

            string _ConnName, _ConnDesc, _ConnEntity;
            string _ConnReg, _CheckConnReg, _KeyReg, _KeySeq;

            int _countConn = 0;

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TSNConnection\Setting\"))
            {
                if (Key != null)
                {
                    _countConn = Convert.ToInt32(Registry.GetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnCount","NULL").ToString());
                }
                else
                {
                    //setting count connection
                    Registry.SetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnCount", _countConn);

                    //setting connection desc
                    //Registry.SetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ApplyConnDesc", "");
                }
            }

            if (_countConn > 0)
            {
                for (int i = 1; i <= _countConn; i++)
                {
                    _CheckConnReg = "SOFTWARE\\TSNConnection\\ListConn\\TSNConn" + i + "\\Conn\\";

                    using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"" + _CheckConnReg + ""))
                        if (Key != null)
                        {
                            _ConnReg = DIR_REG_CONNECTION + "ListConn\\TSNConn" + i + "\\Conn\\";

                            _ConnName = Registry.GetValue(@"" + _ConnReg + "", "ConnName", "NULL").ToString();
                            _ConnDesc = Registry.GetValue(@"" + _ConnReg + "", "ConnDesc", "NULL").ToString();
                            _ConnEntity = Registry.GetValue(@"" + _ConnReg + "", "ConnEntity", "NULL").ToString();
                            _KeySeq = Registry.GetValue(@"" + _ConnReg + "", "ConnSeq", "NULL").ToString();

                            //accReg = DIR_REG_CONNECTION + "TiraNet" + i + "\\Account\\";
                            //entity = Registry.GetValue(@"" + accReg + "", "ExistEntity", "NULL").ToString();
                            //branch = Registry.GetValue(@"" + accReg + "", "ExistBranch", "NULL").ToString();
                            //division = Registry.GetValue(@"" + accReg + "", "ExistDivision", "NULL").ToString();

                            //appReg = DIR_REG_CONNECTION + "TiraNet" + i + "\\Application\\";
                            //applicationName = Registry.GetValue(@"" + appReg + "", "ApplicationName", "NULL").ToString();

                            _KeyReg = "TSNConn" + i;

                            if (_ConnName != "")
                            {
                                DataRow row = dt.NewRow();
                                row["ConnName"] = _ConnName;
                                row["ConnDesc"] = _ConnDesc;
                                row["ConnEntity"] = _ConnEntity;
                                row["ConnKeyReg"] = _KeyReg;
                                row["ConnSeq"] = _KeySeq;

                                dt.Rows.Add(row);
                            }
                        }
                }
            }
            dgList.DataSource = dt;

            //if (dgList.Rows.Count > 0)
            //{
            //    DataGridViewCellEventArgs DataGridViewCellEventArgs = new DataGridViewCellEventArgs(dgvList.Columns["ConnDesc"].Index, dgvList.CurrentRow.Index);
            //    this.dgvList_CellClick(dgvList.CurrentRow.Index, DataGridViewCellEventArgs);
            //}

        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Program.modeTrx = 1;
            FrmConnectionNew frm = new FrmConnectionNew();
            frm.ShowDialog();
            GetList();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgList.Rows.Count == 0)
            {
                MessageBox.Show("List no exists.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (sConnName == "")
                {
                    MessageBox.Show("Please select connection.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DialogResult dr;

                    dr = MessageBox.Show("Are you sure to delete this connection ?", "TSN Connection", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dr == DialogResult.OK)
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TSNConnection\ListConn", true))
                        {
                            if (key != null)
                            {
                                key.DeleteSubKeyTree(sKeyReg);
                                GetList();
                            }
                        }
                    }
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            Program.modeTrx = 2;

            if (dgList.Rows.Count == 0)
            {
                MessageBox.Show("List no exists.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (sConnName == "")
                {
                    MessageBox.Show("Please select connection.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Program.modeTrx = 2;
                    FrmConnectionNew frm = new FrmConnectionNew();
                    frm.pSeq = Convert.ToInt32(sKeySeq);
                    frm.ShowDialog();
                    GetList();
                }
            }
        }

        private void dgList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                sConnName = dgList.Rows[e.RowIndex].Cells["ConnName"].Value.ToString();
                sConnDesc = dgList.Rows[e.RowIndex].Cells["ConnDesc"].Value.ToString();
                sConnEntity = dgList.Rows[e.RowIndex].Cells["ConnEntity"].Value.ToString();
                sKeyReg = dgList.Rows[e.RowIndex].Cells["ConnKeyReg"].Value.ToString();
                sKeySeq = dgList.Rows[e.RowIndex].Cells["ConnSeq"].Value.ToString();
            }
        }

        public Boolean CheckName(string _Name)
        {
            int _CountRow = 0;

            if (dgList.RowCount > 0)
            {
                for (int i = 0; i <= dgList.RowCount - 1; i++)
                {
                    if (dgList.Rows[i].Cells["ConnName"].Value.ToString().Trim() == _Name.Trim())
                    {
                        _CountRow = _CountRow + 1;
                    }
                }
            }
            if (_CountRow > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //private bool FileExists(string pathFile)
        //{
        //    if (Microsoft.VisualBasic.FileIO.FileSystem.FileExists(pathFile))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        private void StartApplication()
        {
            try
            {
                if (dgList.Rows.Count == 0)
                {
                    MessageBox.Show("List no exists.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (sConnDesc == "")
                    {
                        MessageBox.Show("Please select connection.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        //if (FileExists(AppDomain.CurrentDomain.BaseDirectory + AppExe))
                        //{
                            //setting connnecion string
                            Registry.SetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnKey", sKeySeq);

                            Process.Start(AppExe);
                            Application.Exit();
                        //}
                        //else
                        //{
                        //    MessageBox.Show("Application is not exists.", "Application Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Application Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            StartApplication();
        }

        private void dgList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            StartApplication();
        }
    }
}
