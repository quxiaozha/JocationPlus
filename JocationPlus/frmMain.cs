using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Service;
using TestSqlite.sq;
using Microsoft.VisualBasic;

namespace LocationCleaned
{
    public partial class frmMain : Form
    {
        frmMap map = new frmMap();
        LocationService service;
        double speed = 0.0002;
        //public SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
        bool keepMoving = false;

        public frmMain()
        {
            CreateLocationDB();
            InitializeComponent();
            ReadLocationFromDB();
            PrintMessage("https://github.com/quxiaozha/JocationPlus");
            PrintMessage("开源软件，请勿用作非法用途^_^");
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            NativeLibraries.Load();
            service = LocationService.GetInstance();
            service.PrintMessageEvent = PrintMessage;
            service.ListeningDevice();
        }

        private void CreateLocationDB()
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            { 
                //创建名为table1的数据表
                map.locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
                //locationDB.CloseConnection();
            }
            catch (Exception ex)
            {
                //locationDB.CloseConnection();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //locationDB.CloseConnection();
            }
        }

        private void InsertLocation(string name, string position)
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                //locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
                map.locationDB.InsertValues("location", new string[] { name, position });
                //locationDB.CloseConnection();
            }
            catch (Exception ex)
            {
                //locationDB.CloseConnection();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //locationDB.CloseConnection();
            }
        }

        private void ReadLocationFromDB()
        {
            txtLocation.Items.Clear();
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            SQLiteDataReader reader = map.locationDB.ReadFullTable("location");
            try
            {
                //连接数据库
                //locationDB = new SqLiteHelper("locationDB.db");
                //读取整张表
                //SQLiteDataReader reader = locationDB.ReadFullTable("location");
                while (reader.Read())
                {
                    //读取NAME与POSITION                    
                    txtLocation.Items.Add(reader.GetString(reader.GetOrdinal("NAME")) +"["+ reader.GetString(reader.GetOrdinal("POSITION"))+"]");
                }
                reader.Close();
                //locationDB.CloseConnection();

            }
            catch (Exception ex)
            {
                reader.Close();
                //locationDB.CloseConnection();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                reader.Close();
                //locationDB.CloseConnection();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            map.ShowDialog();
            txtLocation.Text = $"{map.Location.Longitude}:{map.Location.Latitude}";
            txtLocation.Items.Clear();
            ReadLocationFromDB();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var location = new Location(txtLocationTest.Text);
            //service.UpdateLocation(location);
            string locStr = txtLocation.Text;
            if (locStr.Contains("[")&locStr.Contains("]"))
            {
                int start = locStr.LastIndexOf("[");
                int end = locStr.LastIndexOf("]");
                locStr = locStr.Substring(start+1, end-start-1);
            }else if (locStr.Contains(","))
            {
                locStr = locStr.Replace(",", ":");
            }
            string[] loc = locStr.Split(new char[] { ':' });
            if (loc.Length == 2)
            {
                map.Location.Longitude = System.Convert.ToDouble(loc[0].Trim());
                map.Location.Latitude = System.Convert.ToDouble(loc[1].Trim());
                service.UpdateLocation(map.Location);
            }
            else
            {
                PrintMessage($"位置格式为：经度:纬度 或 经度,纬度，请确认格式");
            }
            
        }

        public void PrintMessage(string msg)
        {
            if (rtbxLog.InvokeRequired)
            {
                this.Invoke(new Action<string>(PrintMessage), msg);
            }
            else
            {
                rtbxLog.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}： {msg}\r\n");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            service.ClearLocation();
        }

        private void txtLocationTest_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtbxLog_TextChanged(object sender, EventArgs e)
        {

        }

        //↑
        private void button5_Click(object sender, EventArgs e)
        {
            
            PrintMessage($"向上移动.");
            do
            {
                map.Location.Latitude += speed;
                //map.Location.Longitude += 0.0005;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        // ←
        private void button3_Click(object sender, EventArgs e)
        {
            PrintMessage($"向左移动.");
            do
            {
                map.Location.Longitude -= speed;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
            
        }

        //↓
        private void button4_Click(object sender, EventArgs e)
        {
            PrintMessage($"向下移动.");
            do
            {
                map.Location.Latitude -= speed;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
            
        }

        //→
        private void button6_Click(object sender, EventArgs e)
        {
            PrintMessage($"向右移动.");
            do
            {
                map.Location.Longitude += speed;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
            
        }

        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    button3.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Right:
                    button6.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Up:
                    button5.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Down:
                    button4.PerformClick();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            speed = System.Convert.ToDouble(textBox1.Text);
            PrintMessage($"速度修改为：{speed}");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtLocation_Click(object sender, EventArgs e)
        {

        }

        private void txtLocation_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void txtLocation_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtLocation_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.CheckState == CheckState.Checked)
            {
                PrintMessage("开启持续移动，关闭请取消勾选!");
                keepMoving = true;
            }
            else
            {
                PrintMessage("已取消持续移动!");
                keepMoving = false;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            String locName = Interaction.InputBox("", "输入坐标别名", "", -1, -1);
            if(locName != "")
            {
                //MessageBox.Show(locName);
                InsertLocation(locName, map.Location.Longitude + ":" + map.Location.Latitude);
                ReadLocationFromDB();
                map.ReadNameFromDB();
            }
        }
    }

}
