using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Untility
{
    public static class WriteLog
    {
        public static string errorLog = ConfigurationManager.AppSettings["errorLogPath"];
        public static string playLog = ConfigurationManager.AppSettings["playLogPath"];

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="isWritePcName">是否写计算机名</param>
        public static void WriteErrorLogToFile(string log, bool isWritePcName)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");
                string path = "";
                if (!Directory.Exists(errorLog))
                {
                    Directory.CreateDirectory(errorLog);

                }
                if (isWritePcName)
                {
                    path = errorLog + "voicePlayErrorLog" + date + "" + System.Environment.MachineName + ".txt";
                }
                else
                {
                    path = errorLog + "voicePlayErrorLog" + date + ".txt";
                }

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    sw.WriteLine(log);
                    sw.WriteLine('\n');
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("日志文件写入异常-异常信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                throw;
            }
        }

        /// <summary>
        /// 写播放日志
        /// </summary>
        /// <param name="log"></param>
        ///  <param name="isWritePcName"></param>
        public static void WritePlayLogToFile(string log, bool isWritePcName)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");
                string path = "";

                if (!Directory.Exists(playLog))
                {
                    Directory.CreateDirectory(playLog);
                }

                if (isWritePcName)
                {
                    path = playLog + "voicePlayLog" + date + "" + System.Environment.MachineName + ".txt";
                }
                else
                {
                    path = playLog + "voicePlayLog" + date + ".txt";
                }

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    sw.WriteLine(log);
                    sw.WriteLine('\n');
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("日志文件写入异常-异常信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                throw;
            }
        }


        public static void WriteErrorLogToFile(string log)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");
                string path = "";
                if (!Directory.Exists(errorLog))
                {
                    Directory.CreateDirectory(errorLog);

                }

                path = errorLog + "写日志错误" + date + ".txt";

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    sw.WriteLine(log);
                    sw.WriteLine('\n');
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("日志文件写入异常-异常信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                throw;
            }
        }
    }
}
