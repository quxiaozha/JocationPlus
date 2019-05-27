using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Plist;
using iMobileDevice.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocationCleaned
{
    public class LocationService
    {
        List<DeviceModel> Devices = new List<DeviceModel>();
        IiDeviceApi iDevice = LibiMobileDevice.Instance.iDevice;
        ILockdownApi lockdown = LibiMobileDevice.Instance.Lockdown;
        IServiceApi service = LibiMobileDevice.Instance.Service;
        private static LocationService _instance;

        public Action<string> PrintMessageEvent = null;
        private LocationService() { }
        public static LocationService GetInstance() => _instance ?? (_instance = new LocationService());

        public void ListeningDevice()
        {
            var num = 0;
            var deviceError = iDevice.idevice_get_device_list(out var devices, ref num);
            if (deviceError != iDeviceError.Success)
            {
                PrintMessage("无法继续.可能本工具权限不足, 或者未正确安装iTunes工具.");
                return;
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    deviceError = iDevice.idevice_get_device_list(out devices, ref num);
                    if (devices.Count > 0)
                    {
                        var lst = Devices.Select(s => s.UDID).ToList().Except(devices).ToList();

                        var dst = devices.Except(Devices.Select(s => s.UDID)).ToList();

                        foreach (string udid in dst)
                        {
                            iDeviceHandle iDeviceHandle;
                            iDevice.idevice_new(out iDeviceHandle, udid).ThrowOnError();
                            LockdownClientHandle lockdownClientHandle;

                            lockdown.lockdownd_client_new_with_handshake(iDeviceHandle, out lockdownClientHandle, "Quamotion").ThrowOnError("无法读取设备Quamotion");

                            lockdown.lockdownd_get_device_name(lockdownClientHandle, out var deviceName).ThrowOnError("获取设备名称失败.");

                            lockdown.lockdownd_client_new_with_handshake(iDeviceHandle, out lockdownClientHandle, "waua").ThrowOnError("无法读取设备waua");

                            lockdown.lockdownd_get_value(lockdownClientHandle, null, "ProductVersion", out var node).ThrowOnError("获取设备系统版本失败.");

                            LibiMobileDevice.Instance.Plist.plist_get_string_val(node, out var version);

                            iDeviceHandle.Dispose();
                            lockdownClientHandle.Dispose();
                            var device = new DeviceModel
                            {
                                UDID = udid,
                                Name = deviceName,
                                Version = version
                            };

                            PrintMessage($"发现设备: {deviceName}  {version}");
                            LoadDevelopmentTool(device);
                            Devices.Add(device);
                        }

                    }
                    else
                    {
                        Devices.ForEach(itm => PrintMessage($"设备 {itm.Name} {itm.Version} 已断开连接."));
                        Devices.Clear();
                    }
                    Thread.Sleep(1000);
                }
            });
        }
        public bool GetDevice()
        {
            Devices.Clear();
            var num = 0;
            iDeviceError iDeviceError = iDevice.idevice_get_device_list(out var readOnlyCollection, ref num);
            if (iDeviceError == iDeviceError.NoDevice)
            {
                return false;
            }
            iDeviceError.ThrowOnError();
            foreach (string udid in readOnlyCollection)
            {
                //iDeviceHandle iDeviceHandle;
                iDevice.idevice_new(out var iDeviceHandle, udid).ThrowOnError();
                //LockdownClientHandle lockdownClientHandle;
                lockdown.lockdownd_client_new_with_handshake(iDeviceHandle, out var lockdownClientHandle, "Quamotion").ThrowOnError();
                //string deviceName;
                lockdown.lockdownd_get_device_name(lockdownClientHandle, out var deviceName).ThrowOnError();
                string version = "";
                PlistHandle node;
                if (lockdown.lockdownd_client_new_with_handshake(iDeviceHandle, out lockdownClientHandle, "waua") == LockdownError.Success && lockdown.lockdownd_get_value(lockdownClientHandle, null, "ProductVersion", out node) == LockdownError.Success)
                {
                    LibiMobileDevice.Instance.Plist.plist_get_string_val(node, out version);
                }
                iDeviceHandle.Dispose();
                lockdownClientHandle.Dispose();
                var device = new DeviceModel
                {
                    UDID = udid,
                    Name = deviceName,
                    Version = version
                };

                PrintMessage($"发现设备: {deviceName}  {version}  {udid}");
                LoadDevelopmentTool(device);
                Devices.Add(device);
            }
            return true;
        }
        /// <summary>
        /// 加载开发者工具
        /// </summary>
        /// <param name="device"></param>
        public void LoadDevelopmentTool(DeviceModel device)
        {
            var shortVersion = string.Join(".", device.Version.Split('.').Take(2));
            PrintMessage($"为设备 {device.Name} 加载驱动版本 {shortVersion} .");

            var basePath = AppDomain.CurrentDomain.BaseDirectory + "/drivers/";

            if (!File.Exists($"{basePath}{shortVersion}/inject.dmg"))
            {
                PrintMessage($"未找到 {shortVersion} 驱动版本,请前往下载驱动后重新加载设备 .");
                System.Windows.Forms.MessageBox.Show($"未找到 {shortVersion} 驱动版本,请前往下载驱动后重新加载设备 .");
                Process.Start("https://github.com/quxiaozha/JocationPlus/tree/master/drivers");
                return;
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = "injecttool",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true,
                //Arguments = ".\\drivers\\" + shortVersion + "\\inject.dmg"
                Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\drivers\\" + shortVersion + "\\inject.dmg"
            })
            .WaitForExit();
        }
        /// <summary>
        /// 修改定位
        /// </summary>
        /// <param name="location"></param>
        public void UpdateLocation(Location location)
        {
            if (Devices.Count == 0)
            {
                PrintMessage($"修改失败! 未发现任何设备.");
                return;
            }

            iDevice.idevice_set_debug_level(1);

            PrintMessage($"发起位置修改.");
            PrintMessage($"经纬度: {location.Longitude},{location.Latitude}");

            //location = bd09_To_Gcj02(location.Latitude, location.Longitude);
            //PrintMessage($"gcj02经度: {location.Longitude}");
            //PrintMessage($"gcj02纬度: {location.Latitude}");

            //location = gcj_To_Gps84(location.Latitude, location.Longitude);
            //PrintMessage($"gps84经度: {location.Longitude}");
            //PrintMessage($"gps84纬度: {location.Latitude}");

            var Longitude = location.Longitude.ToString();
            var Latitude = location.Latitude.ToString();

            var size = BitConverter.GetBytes(0u);
            Array.Reverse(size);
            Devices.ForEach(itm =>
            {
                PrintMessage($"开始修改设备 {itm.Name} {itm.Version}");

                var num = 0u;
                iDevice.idevice_new(out var device, itm.UDID);
                lockdown.lockdownd_client_new_with_handshake(device, out var client, "com.alpha.jailout").ThrowOnError();//com.alpha.jailout
                lockdown.lockdownd_start_service(client, "com.apple.dt.simulatelocation", out var service2).ThrowOnError();//com.apple.dt.simulatelocation
                var se = service.service_client_new(device, service2, out var client2);
                // 先置空
                se = service.service_send(client2, size, 4u, ref num);

                num = 0u;
                var bytesLocation = Encoding.ASCII.GetBytes(Latitude);
                size = BitConverter.GetBytes((uint)Latitude.Length);
                Array.Reverse(size);
                se = service.service_send(client2, size, 4u, ref num);
                se = service.service_send(client2, bytesLocation, (uint)bytesLocation.Length, ref num);


                bytesLocation = Encoding.ASCII.GetBytes(Longitude);
                size = BitConverter.GetBytes((uint)Longitude.Length);
                Array.Reverse(size);
                se = service.service_send(client2, size, 4u, ref num);
                se = service.service_send(client2, bytesLocation, (uint)bytesLocation.Length, ref num);

                //device.Dispose();
                //client.Dispose();
                PrintMessage($"设备 {itm.Name} {itm.Version} 修改完成.");
            });
        }

        public void ClearLocation()
        {
            if (Devices.Count == 0)
            {
                PrintMessage($"修改失败! 未发现任何设备.");
                return;
            }

            iDevice.idevice_set_debug_level(1);

            PrintMessage($"发起还原位置.");

            Devices.ForEach(itm =>
            {
                PrintMessage($"开始还原设备 {itm.Name} {itm.Version}");
                var num = 0u;
                iDevice.idevice_new(out var device, itm.UDID);
                var lockdowndError = lockdown.lockdownd_client_new_with_handshake(device, out LockdownClientHandle client, "com.alpha.jailout");//com.alpha.jailout
                lockdowndError = lockdown.lockdownd_start_service(client, "com.apple.dt.simulatelocation", out var service2);//com.apple.dt.simulatelocation
                var se = service.service_client_new(device, service2, out var client2);

                se = service.service_send(client2, new byte[4] { 0, 0, 0, 0 }, 4, ref num);
                se = service.service_send(client2, new byte[4] { 0, 0, 0, 1 }, 4, ref num);

                device.Dispose();
                client.Dispose();
                PrintMessage($"设备 {itm.Name} {itm.Version} 还原成功.");
            });
        }

        // 函数功能：若坐标位于中国 , 则返回 TRUE,否则返回 false
        public static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }
        // 函数功能：纬度转换
        public static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
            + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }
        // 函数功能：经度转换
        public static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
            * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0* pi)) * 2.0 / 3.0;
            return ret;
        }
        // 函数功能：经纬度转换
        public static Location transform(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new Location(lon, lat);
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat - dLat;
            double mgLon = lon - dLon;
            return new Location(mgLon, mgLat);
        }

        public static double pi = 3.1415926535897932384626;

        public static double a = 6378245.0;

        public static double ee = 0.00669342162296594323;

        public static double x_pi = 3.14159265358979324 * 3000.0 / 180.0;

        /**将 BD-09(百度地图坐标)转换成GCJ-02(谷歌地图坐标或火星坐标) * * @param */
        public static Location bd09_To_Gcj02(double bd_lat, double bd_lon)
        {
            double x = bd_lon - 0.0065, y = bd_lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double gg_lon = z * Math.Cos(theta);
            double gg_lat = z * Math.Sin(theta);
            return new Location(gg_lon, gg_lat);
        }

        /** 火星坐标系或谷歌地图坐标 (GCJ-02)转换为WGS84（谷歌地球坐标） */
        public static Location gcj_To_Gps84(double lat, double lon) {
            Location gps = transform(lat, lon);
            //double lontitude = lon * 2 - gps.Longitude;
            //double latitude = lat * 2 - gps.Latitude;
            //return new Location(lontitude, latitude);
            return gps;
        }

        /// <summary>
        /// 输出日志消息
        /// </summary>
        /// <param name="msg"></param>
        public void PrintMessage(string msg)
        {
            PrintMessageEvent?.Invoke(msg);
        }
    }

    public class DeviceModel
    {
        public string UDID { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
    }

    public class Location
    {
        public Location()
        {

        }
        public Location(double lo, double la)
        {
            Longitude = lo; Latitude = la;
        }
        public Location(string location)
        {
            var arry = location.Split(',');
            Longitude = double.Parse(arry[0]);
            Latitude = double.Parse(arry[1]);
        }
        /// <summary>
        /// 经度
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double Latitude { get; set; }
    }
}
