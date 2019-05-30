# JocationPlus IOS虚拟定位修改器 

#### 修改自[JonneyDong的Jocation](https://github.com/JonneyDong/JocationRelease)，感谢原作者的开源~

#### 版本记录

- v1.3  增加**速度**和**方向**按钮，支持**键盘上下左右**操作，支持**手工输入**经纬度
- v1.4 增加**匀速**移动功能
- v1.5 增加地点**保存**、**删除**功能、支持保存**当前坐标** 
- v1.6 修复百度地图经纬度偏移，当通过**自带的地图**选取坐标后，会将**BD-09**坐标转换为**WGS-84**坐标，但是不对转换手工输入的坐标，因为我不知道你输入的是什么坐标系~
- v1.7 新增**四个方向**，支持**数字键盘**，调整UI，感谢钟小懒同学的UI设计，有小彩蛋哦~



#### TIPS：

- 如果遇到``` An Lockdown error occurred. The error code was InvalidService。```之类的错误时，请参考[这个](https://github.com/quxiaozha/JocationPlus/issues/2)
- 建议直接通过程序自带的地图选取位置，自动转换后比较精确
- 输入坐标请使用**WGS-84**坐标，可以通过[不同坐标系经纬度查询](http://www.gpsspg.com/maps.htm)或者[百度地图坐标拾取](http://api.map.baidu.com/lbsapi/getpoint/index.html)+[坐标转换](https://tool.lu/coordinate/)等网址自行转换



#### 已知bugs：

- 定位还原不生效，可以重启还原定位
- 偶尔会出现模拟定位失效的情况

