using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Untility
{
    public static class ErrorCode
    {
        /// <summary>
        /// 发送数据共5位。
        /// 第一位8表示屏幕状态，
        /// 第二三位分别表示屏幕的行列，
        /// 第四五位表示屏幕具体状态。
        /// 80000，屏幕正常。表示背光灯开，有信号，信号源是DVI.
        /// 80010,没有返回数据。
        /// 80020，背光灯未开。
        /// 80030，无信号源。
        /// 80041，有信号，信号是VGA.
        /// 80042，有信号，信号是HDMI.
        /// 80043，有信号，信号是DVI.
        /// 80044，有信号，信号是VIDEO01. 
        /// 80045，有信号，信号是VIDEO02. 
        /// 80046，有信号，信号是VIDEO03. 
        /// 80047，有信号，信号是VIDEO04. 
        /// 80048，有信号，信号是SV.       
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetErrorInfo(string pcname, string cityName, string lineName, string station, string position, string code, string time, out string flag)
        {
            try
            {


                string info = "";
                string screenState = "";
                string screenRow = "";
                string screenColumn = "";
                string screenDetailState = "";
                string content = "";
                if (code.Length == 5)
                {
                    screenState = code[0].ToString();
                    screenRow = code[1].ToString();
                    screenColumn = code[2].ToString();
                    screenDetailState = code[3].ToString() + code[4].ToString();

                    //屏幕详细状态
                    switch (screenDetailState)
                    {
                        case "10":
                            content = "没有返回数据";
                            break;
                        case "20":
                            content = "背光灯未开";
                            break;
                        case "30":
                            content = "无信号源";
                            break;
                        case "41":
                            content = "信号源错误,信号源应为DVI,实际为VGA";
                            break;
                        case "42":
                            content = "信号源错误,信号源应为DVI,实际为HDMI";
                            break;
                        //case "43":
                        //    info = "有信号，信号是DVI";
                        //    break;
                        case "44":
                            content = "信号源错误,信号源应为DVI,实际为VIDEO01";
                            break;
                        case "45":
                            content = "信号源错误,信号源应为DVI,实际为VIDEO02";
                            break;
                        case "46":
                            content = "信号源错误,信号源应为DVI,实际为VIDEO03";
                            break;
                        case "47":
                            content = "信号源错误,信号源应为DVI,实际为VIDEO04";
                            break;
                        case "48":
                            content = "信号源错误,信号源应为DVI,实际为SV";
                            break;
                        default:
                            content = "未定义的状态码";
                            break;
                    }
                }
                else
                {
                    info = "屏幕状态码有误";
                    WriteLog.WriteErrorLogToFile(string.Format("屏幕状态码解析错误，错误原因：【状态码有误（位数不够）】时间：【{0}】", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    flag = "0";
                }

                if (code == "80000" || code == "80043")
                {
                    info = "屏幕正常";
                    flag = "0";
                }
                else
                {
                    flag = "1";
                    string tips = "--智元汇\n       【注意】本邮件为系统自动发送的邮件，请不要回复本邮件。";
                    info += string.Format("    城市:【{0}】\n" +
                                            "    线路:【{1}】\n" +
                                            "    站点:【{2}】\n" +
                                            "    点位:【{3}】\n" +
                                            "    计算机名:【{4}】\n" +
                                            "    屏幕位置:【{5}行{6}列】\n" +
                                            "    故障信息:【{7}】\n" +
                                            "    发生时间:【{8}】\n \n \n {9}", cityName, lineName, station, position, pcname, screenRow, screenColumn, content, time, tips);
                }
                return info;
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("解析错误码异常-异常信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),true);
                throw;
            }


        }
    }
}
