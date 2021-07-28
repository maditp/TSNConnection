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
using System.Data.SqlClient;
using System.Data.Sql;
using System.IO;

namespace TSNConnection
{
    public partial class FrmConnectionNew : Form
    {
        string connSQL,connString,sTrustConn="No";
        string sProfileID, sProfileName, sProfileDesc, sProfileServer, sProfileLoginID, sProfilePass, sProfileDBName, sProfileENtity="", _ConnEntityEdit, _ConnDBNameEdit;
        private int _Seq,_SeqEdit;
        private string _EncPass, _DecPass;

        public static string DIR_REG_CONNECTION = "HKEY_CURRENT_USER\\SOFTWARE\\TSNConnection\\";

        DataTable dt;

        public int pSeq
        {
            get { return _Seq; }
            set { _Seq = value; }
        }

        public FrmConnectionNew()
        {
            InitializeComponent();
        }

        private void FrmConnectionNew_Load(object sender, EventArgs e)
        {
            if (Program.modeTrx == 1)
            {
                gbImport.Visible = false;
                gb1.BringToFront();
                ClearText();
                linkLabel1.Visible = true;
            }
            else if (Program.modeTrx == 2)
            {
                gbImport.Visible = false;
                gb1.BringToFront();
                _SeqEdit = pSeq;
                GetConn();
                linkLabel1.Visible = false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            gbImport.Visible = true;
            gbImport.BringToFront();
            txtFilename.Text = "";
        }
        private void cboEntity_Click(object sender, EventArgs e)
        {
            if (cboDBName.Items.Count > 0)
            {
                if (cboDBName.Text.Trim() != "")
                {
                    try
                    {
                        connString = "Data Source=" + txtServer.Text.Trim() + ";initial catalog=" + cboDBName.Text.Trim() + ";User Id=" + txtLoginID.Text.Trim() + ";password=" + txtPassword.Text.Trim() + "";
                        SqlConnection con = new SqlConnection(connString);
                        con.Open();
                        SqlDataAdapter sda = new SqlDataAdapter("select gec_entity_id,gec_entity from GS_ENTITY_CODES", con);
                        DataTable dt = new DataTable();

                        sda.Fill(dt);
                        cboEntity.DataSource = dt;
                        cboEntity.DisplayMember = "gec_entity";
                        cboEntity.ValueMember = "gec_entity_id";

                        if (cboEntity.Text.Trim() == "")
                        {
                            MessageBox.Show("Entity not exists !", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void cboDBName_SelectedValueChanged(object sender, EventArgs e)
        {
            cboEntity.DataSource = null;
        }

        private void cboDBName_Click(object sender, EventArgs e)
        {
            GetDatabase();
            if (Program.modeTrx == 2)
            {
                cboDBName.Text = _ConnDBNameEdit;
            }
        }

        private void btnImportCancel_Click(object sender, EventArgs e)
        {
            gbImport.Visible = false;
            gb1.BringToFront();
            ClearText();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            GetDatabase();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim() == "")
            {
                return;
            }
            if (txtDesc.Text.Trim() == "")
            {
                return;
            }
            if (txtServer.Text.Trim() == "")
            {
                return;
            }
            if (txtLoginID.Text.Trim() == "")
            {
                return;
            }
            if (txtPassword.Text.Trim() == "")
            {
                return;
            }
            if (cboDBName.Text.Trim() == "")
            {
                return;
            }
            if (cboEntity.Text.Trim() == "")
            {
                return;
            }

            if (Program.modeTrx == 1)
            {
                bool _Check = GetList();

                if (_Check == true)
                {
                    MessageBox.Show("Connection name already used !", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            const string message = "Save profile connection?";
            const string caption = "TSN Connection";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                SaveConnStr();
            }
            
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilename.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {

            if (txtFilename.Text.Trim() == "")
            {
                return;
            }

            string[] _Lines = File.ReadAllLines(txtFilename.Text);
            string[] _Values;

            if (_Lines.Length > 1)
            {
                MessageBox.Show("Invalid profile !", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _Values = _Lines[0].ToString().Split('|');

                sProfileID = _Values[0].ToString().Trim();
                sProfileName = _Values[1].ToString().Trim();
                sProfileDesc = _Values[2].ToString().Trim();
                sProfileServer = _Values[3].ToString().Trim();
                sProfileLoginID = _Values[4].ToString().Trim();
                sProfilePass = _Values[5].ToString().Trim();
                sProfileDBName = _Values[6].ToString().Trim();
                sProfileENtity = _Values[7].ToString().Trim();


                if (sProfileID != "TSNCONNECTIONPROFILE")
                {
                    MessageBox.Show("Invalid profile !", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _DecPass = ClsCryptoEngine.Decrypt(sProfilePass, "sblw-3hn8-sqoy19");

                bool checkDB = CheckDatabase(sProfileServer, sProfileLoginID, _DecPass);

                if (checkDB == true)
                {
                    ClearText();

                    txtName.Text = sProfileName;
                    txtName.ReadOnly = true;
                    txtDesc.Text = sProfileDesc;
                    txtDesc.ReadOnly = true;
                    txtServer.Text = sProfileServer;
                    txtServer.ReadOnly = true;
                    txtLoginID.Text = sProfileLoginID;
                    txtLoginID.ReadOnly = true;
                    txtPassword.Text = _DecPass;
                    txtPassword.ReadOnly = true;
                    cboDBName.Enabled = false;
                    cboDBName.Text = sProfileDBName;
                    btnConnect.Enabled = false;

                    connString="Data Source=" + sProfileServer.Trim() + ";initial catalog=" + sProfileDBName.Trim() + ";User Id=" + sProfileLoginID.Trim() + ";password=" + _DecPass.Trim() + "";
                    SqlConnection con = new SqlConnection(connString);
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("select gec_entity_id,gec_entity from GS_ENTITY_CODES where gec_entity_id=" + sProfileENtity.Trim(),con);
                    DataTable dt = new DataTable();

                    sda.Fill(dt);
                    cboEntity.DataSource = dt;
                    cboEntity.DisplayMember = "gec_entity";
                    cboEntity.ValueMember = "gec_entity_id";
                    cboEntity.Enabled = false;

                    if (cboEntity.Text.Trim() == "")
                    {
                        MessageBox.Show("Entity not exists !", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearText();
                        return;
                    }

                    gbImport.Visible = false;
                    gb1.BringToFront();
                }
                else
                {
                    MessageBox.Show("Invalid Getting Database", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void GetDatabase()
        {
            if (txtLoginID.Text.Trim() == "" ||txtServer.Text.Trim()=="" ||txtPassword.Text.Trim()=="" )
            {
                return;
            }

            try
            {
                DataTable dbltables;

                connSQL = "Data Source=" + txtServer.Text.Trim() + ";User Id=" + txtLoginID.Text.Trim() + ";Password=" + txtPassword.Text.Trim() + ";Trusted_Connection=" + sTrustConn + "";
                SqlConnection con = new SqlConnection(connSQL);

                con.Open();
                dbltables = con.GetSchema("Databases");
                dbltables = dbltables.Select("dbid > 4").CopyToDataTable();

                con.Close();
                cboDBName.Items.Clear();
                cboDBName.LoadingType = MTGCComboBox.CaricamentoCombo.ComboBoxItem;
                cboDBName.ColumnNum = 1;
                cboDBName.ColumnWidth = "100";
                cboDBName.SourceDataString = new string[] { "database_name" };
                cboDBName.SourceDataTable = dbltables;
                cboDBName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Application Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboDBName.Items.Clear();
                cboDBName.Text = "";
            }
        }

        private bool CheckDatabase(string _Server,string _LoginID,string _Pass)
        {

            try
            {
                DataTable dbltables;

                connSQL = "Data Source=" + _Server + ";User Id=" + _LoginID + ";Password=" + _Pass + ";Trusted_Connection=" + sTrustConn + "";
                SqlConnection con = new SqlConnection(connSQL);

                con.Open();
                dbltables = con.GetSchema("Databases");
                dbltables = dbltables.Select("dbid > 4").CopyToDataTable();

                con.Close();
                cboDBName.Items.Clear();
                cboDBName.LoadingType = MTGCComboBox.CaricamentoCombo.ComboBoxItem;
                cboDBName.ColumnNum = 1;
                cboDBName.ColumnWidth = "100";
                cboDBName.SourceDataString = new string[] { "database_name" };
                cboDBName.SourceDataTable = dbltables;
                cboDBName.SelectedIndex = 0;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Application Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboDBName.Items.Clear();
                return false;
            }
        }
        private void ClearText()
        {
            txtName.Text = "";
            txtName.ReadOnly = false;
            txtDesc.Text = "";
            txtDesc.ReadOnly = false;
            txtServer.Text = "";
            txtServer.ReadOnly = false;
            txtLoginID.Text = "";
            txtLoginID.ReadOnly = false;
            txtPassword.Text = "";
            txtPassword.ReadOnly = false;
            cboDBName.Items.Clear();
            cboDBName.Enabled = true;
            cboEntity.Items.Clear();
            cboEntity.Enabled = true;
            btnConnect.Enabled = true;
        }

        private void SaveConnStr()
        {
            try
            {
                int _countConn = 0;
                string connReg = "", sEnt;

                if ((txtDesc.Text.Trim() == "") || (txtServer.Text.Trim() == "") || (cboDBName.Text.Trim() == ""))
                {
                    MessageBox.Show("Please check your connection string.", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    if (sProfileENtity.ToString().Trim() == "")
                    {
                        sEnt = cboEntity.SelectedValue.ToString().Trim();
                    }
                    else
                    {
                        sEnt = sProfileENtity.ToString().Trim();
                    }
                    if (Program.modeTrx == 1) //new
                    {
                        //cek jumlah koneksi                    
                        _countConn = Convert.ToInt32(Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\TSNConnection\Setting\", "ConnCount", "NULL").ToString());
                        _countConn = _countConn + 1;

                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\TSNConnection\Setting", "ConnCount", _countConn);

                        connReg = "HKEY_CURRENT_USER\\SOFTWARE\\TSNConnection\\ListConn\\TSNConn" + _countConn + "\\Conn";

                        Registry.SetValue(@"" + connReg + "", "ConnSeq", _countConn);

                    }
                    else if (Program.modeTrx == 2) //edit
                    {
                        connReg = "HKEY_CURRENT_USER\\SOFTWARE\\TSNConnection\\ListConn\\TSNConn" + _SeqEdit + "\\Conn";

                    }

                    _EncPass = ClsCryptoEngine.Encrypt(txtPassword.Text, "sblw-3hn8-sqoy19");

                    Registry.SetValue(@"" + connReg + "", "ConnName", txtName.Text.Trim());
                    Registry.SetValue(@"" + connReg + "", "ConnDesc", txtDesc.Text.Trim());
                    Registry.SetValue(@"" + connReg + "", "ConnServer", txtServer.Text.Trim());
                    Registry.SetValue(@"" + connReg + "", "ConnUserID", txtLoginID.Text.Trim());
                    Registry.SetValue(@"" + connReg + "", "ConnPass", _EncPass);
                    Registry.SetValue(@"" + connReg + "", "ConnDBName", cboDBName.Text.Trim());
                    Registry.SetValue(@"" + connReg + "", "ConnEntity", sEnt);

                    MessageBox.Show("Connection profile successfully save", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Boolean GetList()
        {
            dt = new DataTable();
            dt.Columns.Add("ConnName");

            dt.Rows.Clear();

            string _ConnName;
            string _ConnReg, _CheckConnReg;

            int _countConn = 0,_CountCheck=0;

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TSNConnection\Setting\"))
            {
                if (Key != null)
                {
                    _countConn = Convert.ToInt32(Registry.GetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnCount", "NULL").ToString());
                }
                else
                {
                    Registry.SetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnCount", _countConn);

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

                            if (_ConnName != "")
                            {
                                DataRow row = dt.NewRow();
                                row["ConnName"] = _ConnName;

                                dt.Rows.Add(row);
                            }
                        }
                }

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i <= dt.Rows.Count - 1; i++)
                    {
                        if(dt.Rows[i]["ConnName"].ToString()== txtName.Text.Trim())
                        {
                            _CountCheck = _CountCheck + 1;
                        }
                    }
                }

                if (_CountCheck > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void GetConn()
        {
            dt = new DataTable();
            dt.Columns.Add("ConnName");
            dt.Columns.Add("ConnDesc");
            dt.Columns.Add("ConnServer");
            dt.Columns.Add("ConnUserID");
            dt.Columns.Add("ConnPass");
            dt.Columns.Add("ConnDBName");
            dt.Columns.Add("ConnEntity");
            dt.Columns.Add("ConnKeyReg");
            dt.Columns.Add("ConnSeq");

            dt.Rows.Clear();

            string _ConnName, _ConnDesc, _ConnServer, _ConnUserID, _ConnPass, _ConnDBName, _ConnEntity, _ConnKeyReg, _ConnSeq;
            string _ConnReg, _CheckConnReg;

            int _countConn = 0;

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TSNConnection\Setting\"))
            {
                if (Key != null)
                {
                    _countConn = Convert.ToInt32(Registry.GetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnCount", "NULL").ToString());
                }
                else
                {
                    Registry.SetValue(@"" + DIR_REG_CONNECTION + "" + "Setting\\", "ConnCount", _countConn);
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
                            _ConnServer = Registry.GetValue(@"" + _ConnReg + "", "ConnServer", "NULL").ToString();
                            _ConnUserID = Registry.GetValue(@"" + _ConnReg + "", "ConnUserID", "NULL").ToString();
                            _ConnPass = Registry.GetValue(@"" + _ConnReg + "", "ConnPass", "NULL").ToString();
                            _ConnDBName = Registry.GetValue(@"" + _ConnReg + "", "ConnDBName", "NULL").ToString();
                            _ConnEntity = Registry.GetValue(@"" + _ConnReg + "", "ConnEntity", "NULL").ToString();
                            _ConnSeq = Registry.GetValue(@"" + _ConnReg + "", "ConnSeq", "NULL").ToString();
                            _ConnKeyReg = "TSNConn" + _ConnSeq;

                            if (_ConnName != "")
                            {
                                _DecPass = ClsCryptoEngine.Decrypt(_ConnPass, "sblw-3hn8-sqoy19");

                                DataRow row = dt.NewRow();
                                row["ConnName"] = _ConnName;
                                row["ConnDesc"] = _ConnDesc;
                                row["ConnServer"] = _ConnServer;
                                row["ConnUserID"] = _ConnUserID;
                                row["ConnPass"] = _DecPass;
                                row["ConnDBName"] = _ConnDBName;
                                row["ConnSeq"] = _ConnSeq;
                                row["ConnEntity"] = _ConnEntity;
                                dt.Rows.Add(row);
                            }
                        }
                }

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i <= dt.Rows.Count - 1; i++)
                    {
                        if (dt.Rows[i]["ConnSeq"].ToString() == _SeqEdit.ToString())
                        {
                            txtName.Text = dt.Rows[i]["ConnName"].ToString();
                            txtName.ReadOnly = false;
                            txtDesc.Text = dt.Rows[i]["ConnDesc"].ToString();
                            txtDesc.ReadOnly = false;
                            txtServer.Text = dt.Rows[i]["ConnServer"].ToString();
                            txtServer.ReadOnly = false;
                            txtLoginID.Text = dt.Rows[i]["ConnUserID"].ToString();
                            txtLoginID.ReadOnly = false;
                            txtPassword.Text = dt.Rows[i]["ConnPass"].ToString();
                            txtPassword.ReadOnly = false;
                            cboDBName.Text = dt.Rows[i]["ConnDBName"].ToString();
                            cboDBName.Enabled = true;
                            btnConnect.Enabled = true;
                            _ConnEntityEdit= dt.Rows[i]["ConnEntity"].ToString();
                            _ConnDBNameEdit = dt.Rows[i]["ConnDBName"].ToString();
                        }
                    }

                    connString = "Data Source=" + txtServer.Text.Trim() + ";initial catalog=" + cboDBName.Text.Trim() + ";User Id=" + txtLoginID.Text.Trim() + ";password=" + txtPassword.Text.Trim() + "";
                    SqlConnection con = new SqlConnection(connString);
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("select gec_entity_id,gec_entity from GS_ENTITY_CODES where gec_entity_id=" + _ConnEntityEdit.ToString().Trim(), con);
                    DataTable dte = new DataTable();

                    sda.Fill(dte);
                    cboEntity.DataSource = dte;
                    cboEntity.DisplayMember = "gec_entity";
                    cboEntity.ValueMember = "gec_entity_id";

                    if (cboEntity.Text.Trim() == "")
                    {
                        MessageBox.Show("Entity not exists !", "TSN Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearText();
                        return;
                    }
                }
            }
        }
    }
}
