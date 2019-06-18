using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestSqlite.sq;

namespace LocationCleaned
{
    [ComVisible(true)]
    public partial class frmMap : Form
    {
        /// <summary>
        /// 经纬度坐标
        /// </summary>
        public new Location Location { get; set; } = new Location();
        public SqLiteHelper locationDB { get; set; } = new SqLiteHelper("locationDB.db");
        public new Location txtLocation { get; set; } = new Location();
        public frmMap()
        {
            //this.locationDB = locationDB;
            CreateLocationDB();
            InitializeComponent();
            ReadNameFromDB();
        }

        private void frmMap_Load(object sender, EventArgs e)
        {
            this.webBrowser1.ScriptErrorsSuppressed = true;
            var text = @"<html xmlns='http://www.w3.org/1999/xhtml\'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title>站点地图</title>
    <style type='text/css'>
        body, html, #allmap
        {
            width: 100%;
            height: 100%;
            overflow: hidden;
            margin: 0;
        }
        #l-map
        {
            height: 100%;
            width: 78%;
            float: left;
            border-right: 2px solid #bcbcbc;
        }
        #r-result
        {
            height: 100%;
            width: 20%;
            float: left;
        }
    </style>
    <script type='text/javascript' src='http://api.map.baidu.com/api?v=3.0&ak=qsgSGFbqwGGzeQZU1ahGqLN3xQaHGrHf'></script>
</head>
<body>
    <div id='allmap'>
    </div>
    <div style='float:right;position: absolute;
    top: 1px;
    left: 60px;background-color:;font-size: 12px;
    line-height: 23px;'>
        <div style='float:left'>请输入关键字搜索</div>
        <div id='r-result' style='float:left'><input type='text' id='suggestId' size='20' value=''  style='width:300px;' /></div>
        <div id='searchResultPanel' style='border:1px solid #C0C0C0;width:300px;height:auto; display:none;'></div>
    </div>
</body>
</html>
<script type='text/javascript'>
    document.oncontextmenu=new Function('event.returnValue=false;'); document.onselectstart=new Function('event.returnValue=false;'); 
    var marker;
    var map = new BMap.Map('allmap');               // 创建Map实例
    var point = new BMap.Point(116.331398,39.897445);    // 创建点坐标(经度,纬度)
    map.centerAndZoom(point, 13);                   // 初始化地图,设置中心点坐标和地图大小级别
    var lng = window.external.GetLongitude();
    var lat = window.external.GetLatitude();
    if(lng =! '0' && lat != '0'){
        point = new BMap.Point(window.external.GetLongitude(), window.external.GetLatitude());    // 创建点坐标(经度,纬度)
        map.centerAndZoom(point, 13);                   // 初始化地图,设置中心点坐标和地图大小级别
    }else{
        var myCity = new BMap.LocalCity();
        myCity.get(myFun); 
    }

    //map.addOverlay(new BMap.Marker(point));         // 给该坐标加一个红点标记
    map.addControl(new BMap.NavigationControl());   // 添加平移缩放控件
    map.addControl(new BMap.ScaleControl());        // 添加比例尺控件
    map.addControl(new BMap.OverviewMapControl());  //添加缩略地图控件
    map.addControl(new BMap.MapTypeControl());      //添加地图类型控件
    map.enableScrollWheelZoom();                    //启用滚轮放大缩小
    var geoc = new BMap.Geocoder();  
    map.addEventListener('click', function (e) {
        checkMaker();
        point = new BMap.Point(e.point.lng, e.point.lat);
        marker = new BMap.Marker(point);
        map.addOverlay(marker);
        var pt = e.point;
        geoc.getLocation(pt, function (rs) {
            var addComp = rs.addressComponents;
            var address = [];
            if (addComp.province.length > 0) {
                address.push(addComp.province);
            }
            if (addComp.city.length > 0) {
                address.push(addComp.city);
            }
            if (addComp.district.length > 0) {
                address.push(addComp.district);
            }
            if (addComp.street.length > 0) {
                address.push(addComp.street);
            }
            if (addComp.streetNumber.length > 0) {
                address.push(addComp.streetNumber);
            }
            window.external.position(e.point.lat, e.point.lng, address.join(','));
        });
    });

    function myFun(result){
	    var cityName = result.name;
	    map.setCenter(cityName);
        //map.centerAndZoom(cityName, 13);
    }
        function G(id) {
        return document.getElementById(id);
    }
    var ac = new BMap.Autocomplete(    //建立一个自动完成的对象
       {
           'input': 'suggestId'
          , 'location': map
       });
    ac.addEventListener('onhighlight', function (e) {  //鼠标放在下拉列表上的事件
        var str = '';
        var _value = e.fromitem.value;
        var value = '';
        if (e.fromitem.index > -1) {
            value = _value.province + _value.city + _value.district + _value.street + _value.business;
        }
        str = 'FromItem<br />index = ' + e.fromitem.index + '<br />value = ' + value;
        value = '';
        if (e.toitem.index > -1) {
            _value = e.toitem.value;
            value = _value.province + _value.city + _value.district + _value.street + _value.business;
        }
        str += '<br />ToItem<br />index = ' + e.toitem.index + '<br />value = ' + value;
        G('searchResultPanel').innerHTML = str;
    });
    var myValue;
    ac.addEventListener('onconfirm', function (e) {    //鼠标点击下拉列表后的事件
        var _value = e.item.value;
        myValue = _value.province + _value.city + _value.district + _value.street + _value.business;
        G('searchResultPanel').innerHTML = 'onconfirm<br />index = ' + e.item.index + '<br />myValue = ' + myValue;
        setPlace();
    });
    function setPlace() {
        map.clearOverlays();    //清除地图上所有覆盖物
        function myFun() {
            var pp = local.getResults().getPoi(0).point;    //获取第一个智能搜索的结果
            checkMaker();
            map.centerAndZoom(pp, 15);
            marker=new BMap.Marker(pp);
            map.addOverlay(marker);    //添加标注
         var pt = pp;
            geoc.getLocation(pt, function (rs) {
                var addComp = rs.addressComponents;
                var address = [];
                if (addComp.province.length > 0) {
                    address.push(addComp.province);
                }
                if (addComp.city.length > 0) {
                    address.push(addComp.city);
                }
                if (addComp.district.length > 0) {
                    address.push(addComp.district);
                }
                if (addComp.street.length > 0) {
                    address.push(addComp.street);
                }
                if (addComp.streetNumber.length > 0) {
                    address.push(addComp.streetNumber);
                }
                window.external.position(pp.lat, pp.lng, address.join(','));
            });
        }
        var local = new BMap.LocalSearch(map, { //智能搜索
            onSearchComplete: myFun
        });
        local.search(myValue);
    }
    function checkMaker() {
        if (marker != null)
            map.removeOverlay(marker);
    };
    function evaluatepoint(log,lat){
        checkMaker();
        point = new BMap.Point(log,lat);
        map.centerAndZoom(point, 15);
        marker = new BMap.Marker(point);
        map.addOverlay(marker);
        var pt = point;
        geoc.getLocation(pt, function (rs) {
            var addComp = rs.addressComponents;
            var address = [];
            if (addComp.province.length > 0) {
                address.push(addComp.province);
            }
            if (addComp.city.length > 0) {
                address.push(addComp.city);
            }
            if (addComp.district.length > 0) {
                address.push(addComp.district);
            }
            if (addComp.street.length > 0) {
                address.push(addComp.street);
            }
            if (addComp.streetNumber.length > 0) {
                address.push(addComp.streetNumber);
            }
            window.external.position(pt.lat,pt.lng, address.join(','));
        });
    };</script>";
            this.webBrowser1.DocumentText = text;
            this.webBrowser1.ObjectForScripting = this;
        }
        public void position(string a_0, string a_1, string b_0)
        {
            this.label3.Text = (double.Parse( a_1)).ToString();
            this.label4.Text = (double.Parse(a_0)).ToString();
            this.label5.Text = b_0;
            this.textBox1.Text = b_0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double lon = double.Parse(label3.Text);
            double lat = double.Parse(label4.Text);
            Location location = LocationService.bd09_To_Gcj02(lat, lon);
            location = LocationService.gcj_To_Gps84(location.Latitude, location.Longitude);
            this.Location = location;
            Close();
            
        }
        public void Alert(string msg)
        {
            MessageBox.Show(msg);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        public void button2_Click_1(object sender, EventArgs e)
        {
            if(this.textBox1.Text.Trim() != "" && this.label3.Text.Trim() != "" && this.label4.Text.Trim() != "")
            {
                string name = this.textBox1.Text.Trim();
                string position = this.label3.Text.Trim() + ":" + this.label4.Text.Trim();
                //MessageBox.Show(name+"\n"+position);
                this.InsertLocation(name, position);
                this.ReadNameFromDB();
            }
            else
            {
                MessageBox.Show("别名和经纬度不能为空哦!");
            }
            
        }

        private void CreateLocationDB()
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
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
        public string GetLatitude()
        {
            Console.WriteLine(Location.Latitude.ToString());
            Console.WriteLine(txtLocation.Latitude.ToString());
            if (Location.Latitude.ToString() == "0" && txtLocation.Latitude.ToString() == "0")
            {
                return "0";
            }
            return Location.Latitude.ToString()=="0"?txtLocation.Latitude.ToString(): Location.Latitude.ToString();
        }

        public string GetLongitude()
        {
            Console.WriteLine(Location.Longitude.ToString());
            Console.WriteLine(txtLocation.Longitude.ToString());
            if (Location.Longitude.ToString() == "0" && txtLocation.Longitude.ToString() == "0")
            {
                return "0";
            }
            return Location.Longitude.ToString() == "0" ? txtLocation.Longitude.ToString() : Location.Longitude.ToString();
        }

        public void ReadNameFromDB()
        {
            comboBox1.Items.Clear();
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            SQLiteDataReader reader = locationDB.ReadFullTable("location");
            try
            {
                //连接数据库
                //locationDB = new SqLiteHelper("locationDB.db");
                //读取整张表
                //SQLiteDataReader reader = locationDB.ReadFullTable("location");
                while (reader.Read())
                {
                    //读取NAME与POSITION                    
                    comboBox1.Items.Add(reader.GetString(reader.GetOrdinal("NAME")));
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

        private void InsertLocation(string name, string position)
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                //locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
                locationDB.InsertValues("location", new string[] {name, position});
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

        private void DeleteLocation(string name)
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                locationDB.DeleteValuesAND("location", new string[] {"NAME"}, new string[] { name }, new string[] {"="});
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

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string name = comboBox1.Text.Trim();
            DeleteLocation(name);
            ReadNameFromDB();
        }

        private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
