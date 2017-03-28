using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;
using System.Xml.Linq;
using Untility;

namespace PlayerClient
{
    public partial class Form1 : Form
    {
        //播放时间间隔
        float interval = 10.0F;  

        //主题
        string recordTitle = "";

        float Ad_Volume_level = 0F;


        //获取xml路径
        public static string xmlPath = System.Configuration.ConfigurationManager.AppSettings["xmlRoute"];

        //计数
        int i = 0;

        //播放路径
        string url = XmlParseHelper.GetSingNode("VoiceFile");


        public Form1()
        {
            InitializeComponent();
        }

        delegate void PlayInvoke();
        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

            StartUp("1");
            bool flag = false;
            bool Deadline = false;

            //判断是否已过截止日期
            Deadline = isDeadline();
            if (Deadline)
            {
                System.Environment.Exit(0);
            }


            while (!flag)
            {
                flag = isStartAd();

                //广告程序还未启动 程序休眠10秒钟
                if (!flag)
                {
                    System.Threading.Thread.Sleep(10000);
                }
            }

            //获取播放主题
            GetTitle();


            //广告程序已经启动 开始
            if (flag)
            {
                SetAdVolume();
                PlayVoice();
            }
        }

        /// <summary>
        /// 开机自动启动
        /// </summary>
        /// <param name="flag"></param>
        private void StartUp(string flag)
        {
            try
            {
                string path = Application.StartupPath;
                string keyName = "VoicePlayer";
                Microsoft.Win32.RegistryKey Rkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (flag.Equals("1"))
                {
                    if (Rkey == null)
                    {
                        Rkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                    }
                    if (Rkey.GetValue(keyName) == null)
                    {
                        Rkey.SetValue(keyName, path + @"\PlayerClient.exe");
                    }
                }
                else
                {
                    if (Rkey != null)
                    {
                        Rkey.DeleteValue(keyName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("设置开机启动程序-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 获取主题名
        /// </summary>
        public void GetTitle()
        {
            try
            {
                recordTitle = XmlParseHelper.GetSingNode("PlayTitle");
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("获取播放主题错误-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 判断广告程序是否已经启动
        /// </summary>
        /// <returns></returns>
        public bool isStartAd()
        {
            try
            {
                bool isStart = false;
                //定义并从xml文件中加载节点（根节点）
                XElement rootNode = XElement.Load(xmlPath);

                //查询语句：获取根节点下子节点(此时子节点可以跨层次：孙节点、重孙节点)
                IEnumerable<XElement> targetNodes = from target in rootNode.Descendants("VoicePlayStart").Descendants("volume")
                                                    select target;

                //判断广告进程是否已经启动
                foreach (XElement node in targetNodes)
                {
                    if (node.Attribute("type").Value == "ad")
                    {
                        isStart = Controller.IsExistProcessName(node.Attribute("name").Value);
                    }
                }

                return isStart;
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("判断广告程序是否已经启动错误-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 判断播放是否已到期
        /// </summary>
        /// <returns></returns>
        public bool isDeadline()
        {
            try
            {
                //截止日期还未到
                bool isOverDue = false;
                DateTime start = DateTime.Now;
                DateTime end;

                List<string> stopDate = XmlParseHelper.GetNodeList("PlayTime", "StopDate", "deadline");
                List<string> startDate = XmlParseHelper.GetNodeList("PlayTime", "StartDate");

                //开始时间 
                if (startDate != null && startDate.Count > 0)
                {
                    start = Convert.ToDateTime(startDate.First());
                }

                //结束时间
                if (stopDate != null && stopDate.Count > 0 && stopDate.First() != "")
                {
                    end = Convert.ToDateTime(stopDate.First());
                }
                else
                {
                    string unit = "";
                    int unitConverToDays = 0;
                    int num = 0;

                    //单位
                    List<string> stopUnit = XmlParseHelper.GetNodeList("PlayTime", "StopDate", "unit");

                    //数量
                    List<string> stopNum = XmlParseHelper.GetNodeList("PlayTime", "StopDate", "num");

                    if (stopUnit != null && stopUnit.Count > 0)
                    {
                        unit = stopUnit.First();
                    }

                    if (stopNum != null && stopNum.Count > 0)
                    {
                        num = Convert.ToInt32(stopNum.First());
                    }
                    switch (unit)
                    {
                        case "week":
                            unitConverToDays = 7;
                            break;
                        case "day":
                            unitConverToDays = 1;
                            break;
                        case "year":
                            unitConverToDays = 365;
                            break;
                        case "month":
                            unitConverToDays = 30;
                            break;
                        default:
                            unitConverToDays = 0;
                            break;
                    }

                    int sumDays = num * unitConverToDays;
                    end = start.AddDays(sumDays);
                }

                DateTime now = DateTime.Now;

                //开始时间未到 strat>now 截止日期已过 now>end     start<=now<=end

                //小于开始时间 开始时间大于现在的时间，过期
                if (DateTime.Compare(start, now) > 0)
                {
                    isOverDue = true;
                }
                else
                {
                    //大于开始时间，如果还大于了截止日期 也是过期
                    if (DateTime.Compare(end, now) <= 0)
                    {
                        isOverDue = true;
                    }
                }

                return isOverDue;
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("判断播放语音是否已到截止日期错误-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }

        }

        /// <summary>
        /// 播放语音
        /// </summary>
        public void PlayVoice()
        {
            try
            {
                //获取播放时间间隔
                IsDurationSetInterval();

                //播放计数
                ++i;

                //记录播放日志
                WriteLog.WritePlayLogToFile(string.Format("计算机【{0}】主题【{1}】播放第【{2}】次,【{3}】", System.Environment.MachineName, recordTitle, i, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);

                //获取路径
                Player.URL = url;
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("播放语音异常-异常信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 设置广告声音
        /// </summary>
        private void SetAdVolume()
        {
            try
            {
                //定义并从xml文件中加载节点（根节点）
                XElement rootNode = XElement.Load(xmlPath);

                //查询语句：获取根节点下子节点(此时子节点可以跨层次：孙节点、重孙节点)
                //获取xml配置文件的初始化声音
                IEnumerable<XElement> targetNodes = from target in rootNode.Descendants("InitializeVoice")
                                                    select target;

                foreach (XElement node in targetNodes)
                {
                    Ad_Volume_level = (Convert.ToInt32(node.Attribute("level").Value)) / (float)100;
                    Controller.SetVolume(node.Attribute("name").Value, Ad_Volume_level);
                    //Controller.SetVolume(node.Attribute("type").Value, node.Attribute("name").Value, (Convert.ToInt32(node.Attribute("level").Value)) / (float)100, ref Ad_Volume_level);
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("设置广告声音错误-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 播放状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //正在播放
            if (Player.status.Contains("正在播放"))
            {

                DownAdVolume();
                this.Hide();
            }
            //播放已完成
            else if (Player.status.Contains("已完成"))
            {
                UpAdVolume();

            }

                //播放停止
            else if (Player.status.Contains("停止"))
            {
                //获取音频时长 向上取整
                int duration = (int)Math.Ceiling(Player.currentMedia.duration);

                //休眠 播放间隔-音频时长
                int sleep = Convert.ToInt32((interval * 60 - duration) * 1000);

                System.Threading.Thread.Sleep(sleep);


                //重新播放时判断广告程序是否已经启动
                bool flag = false;


                while (!flag)
                {

                    flag = isStartAd();

                    if (flag)
                    {
                        //获取播放时间间隔
                        IsDurationSetInterval();

                        //播放计数
                        ++i;
                        //记录播放日志
                        WriteLog.WritePlayLogToFile(string.Format("计算机【{0}】主题【{1}】播放第【{2}】次,【{3}】", System.Environment.MachineName, recordTitle, i, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);

                        //重新播放
                        Player.Ctlcontrols.play();
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10000);
                    }
                }
            }
        }

        /// <summary>
        /// 窗体关闭 广告音量恢复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpAdVolume();
        }

        /// <summary>
        /// 获取语音播放开始后音量设置
        /// </summary>
        public void DownAdVolume()
        {
            try
            {
                //定义并从xml文件中加载节点（根节点）
                XElement rootNode = XElement.Load(xmlPath);

                //查询语句：获取根节点下子节点(此时子节点可以跨层次：孙节点、重孙节点)
                IEnumerable<XElement> targetNodes = from target in rootNode.Descendants("VoicePlayStart").Descendants("volume")
                                                    select target;
                foreach (XElement node in targetNodes)
                {
                    Controller.SetVolume(node.Attribute("name").Value, (Convert.ToInt32(node.Attribute("level").Value)) / (float)100);

                    //Controller.SetVolume(node.Attribute("type").Value, node.Attribute("name").Value, (Convert.ToInt32(node.Attribute("level").Value)) / (float)100, ref Ad_Volume_level);
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("获取语音播放开始后,音量设置错误-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }


        /// <summary>
        /// 获取语音播放停止后音量设置
        /// </summary>
        public void UpAdVolume()
        {
            try
            {
                //定义并从xml文件中加载节点（根节点）
                XElement rootNode = XElement.Load(xmlPath);

                //查询语句：获取根节点下子节点(此时子节点可以跨层次：孙节点、重孙节点)
                IEnumerable<XElement> targetNodes = from target in rootNode.Descendants("VoicePlayStop").Descendants("volume")
                                                    select target;
                foreach (XElement node in targetNodes)
                {
                    if (node.Attribute("type").Value == "ad")
                    {
                        Controller.SetVolume(node.Attribute("name").Value, Ad_Volume_level);
                    }
                    else if (node.Attribute("type").Value == "voice")
                    {
                        Controller.SetVolume(node.Attribute("name").Value, (Convert.ToInt32(node.Attribute("level").Value)) / (float)100);
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("获取语音播放停止后,音量设置数据-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 判断当前时间是否处于一段连续时间之中并获取播放间隔
        /// </summary>
        /// <returns></returns>
        public void IsDurationSetInterval()
        {
            //初始化0  1-平谷期 2-高峰期
            //int flag = 0;

            try
            {
                //定义并从xml文件中加载节点（根节点）
                XElement rootNode = XElement.Load(xmlPath);

                //查询语句：获取根节点下子节点(此时子节点可以跨层次：孙节点、重孙节点) 高峰
                IEnumerable<XElement> fastigiumTimeNodes = from target in rootNode.Descendants("Fastigium").Descendants("param").Descendants("time")
                                                           select target;
                foreach (XElement node in fastigiumTimeNodes)
                {
                    DateTime startTime = DateTime.Parse(node.Attribute("start").Value.ToString());
                    DateTime endTime = DateTime.Parse(node.Attribute("end").Value.ToString());
                    DateTime nowTime = DateTime.Now;
                    if (nowTime > startTime && nowTime < endTime)
                    {
                        //flag = 2;
                        var strInterval = from target in rootNode.Descendants("Normal").Descendants("param")
                                          select target;
                        interval = Convert.ToSingle(strInterval.Last().Attribute("value").Value);
                        break;
                    }

                }

                //查询语句：获取根节点下子节点(此时子节点可以跨层次：孙节点、重孙节点) 平谷
                IEnumerable<XElement> normalTimeTimeNodes = from target in rootNode.Descendants("Normal").Descendants("param").Descendants("time")
                                                            select target;
                foreach (XElement node in normalTimeTimeNodes)
                {
                    DateTime startTime = DateTime.Parse(node.Attribute("start").Value.ToString());
                    DateTime endTime = DateTime.Parse(node.Attribute("end").Value.ToString());
                    DateTime nowTime = DateTime.Now;
                    if (nowTime > startTime && nowTime < endTime)
                    {
                        //flag = 1;
                        var strInterval = from target in rootNode.Descendants("Normal").Descendants("param")
                                          select target;
                        interval = Convert.ToSingle(strInterval.Last().Attribute("value").Value);
                        break;
                    }
                }
                //既不处于高峰期 又不处于平谷期 
                //if (flag==0)
                //{
                //    System.Environment.Exit(0);
                //}
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("判断是否是高峰期或者平谷期,并设置播放时间间隔-错误信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true));
                throw ex;
            }
        }

        /// <summary>
        /// 托盘双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        ///  窗体尺寸变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.Hide();
                this.ShowInTaskbar = false;
            }
            else
            {
                notifyIcon1.Visible = false;
            }
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    //Thread thread = new Thread(new ThreadStart(PlayVoice));
        //    //thread.Start();
        //}



        /// <summary>
        /// 降低广告声音
        /// 提升语音声音
        /// </summary>
        //public void DownAdVolume()
        //{
        //    string setLowProcessName = "cloudmusic";
        //    int setLowVolumeLevel = 0;
        //    float DownLevel = (float)setLowVolumeLevel / (float)100;

        //    string setHighProcessName = "playerclient";
        //    int setHighVolumeLevel = 50;
        //    float UpLevel = (float)setHighVolumeLevel / (float)100;
        //    //1.降低ttplayer的声
        //    Controller.SetVolume(setLowProcessName, DownLevel);

        //    //2.设置本程序声音
        //    Controller.SetVolume(setHighProcessName, UpLevel);


        //}

        /// <summary>
        /// 恢复广告音量
        /// 降低语音音量
        /// </summary>
        //public void UpAdVolume()
        //{
        //    string setLowProcessName = "playerclient";
        //    int setLowVolumeLevel = 0;
        //    float DownLevel = (float)setLowVolumeLevel / (float)100;

        //    string setHighProcessName = "cloudmusic";
        //    int setHighVolumeLevel = 50;
        //    float UpLevel = (float)setHighVolumeLevel / (float)100;
        //    //1.恢复ttplayer的声
        //    Controller.SetVolume(setHighProcessName, UpLevel);

        //    //2.设置本程序声音
        //    Controller.SetVolume(setLowProcessName, DownLevel);    
        //}


    }
}
