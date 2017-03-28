using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace Untility
{
    /// <summary>
    /// E-mailHelper
    /// </summary>
    public static class EmailHelper
    {
        public static void SendEmailToReportBroken(string contentStr)
        {
            try
            {
                var emailAcount = XmlParseHelper.GetSingNode("Sender", "Account").Trim(); ;
                var emailPassword = XmlParseHelper.GetSingNode("Sender", "Password").Trim();
                MailMessage message = new MailMessage();

                //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
                MailAddress fromAddr = new MailAddress(emailAcount);
                message.From = fromAddr;

                //设置收件人,可添加多个,添加方法与下面的一样
                foreach (var item in XmlParseHelper.GetNodeList("receiver", "Account"))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        message.To.Add(item.Trim());
                    }
                    
                }

                //设置抄送人
                foreach (var item in XmlParseHelper.GetNodeList("CCList", "Account"))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        message.CC.Add(item.Trim());
                    }
                }

                //设置邮件标题
                message.Subject = XmlParseHelper.GetSingNode("Title").Trim();

                //设置邮件内容
                message.Body = contentStr;

                //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
                var smtp = XmlParseHelper.GetSingNode("SmtpClient", "Host").Trim();
                var host = Convert.ToInt32(XmlParseHelper.GetSingNode("SmtpClient", "Port").Trim());
                SmtpClient client = new SmtpClient(smtp, host);

                //设置发送人的邮箱账号和密码
                client.Credentials = new NetworkCredential(emailAcount, emailPassword);

                //启用ssl,也就是安全发送 -服务器不支持安全
                client.EnableSsl = false;

                //发送邮件
                client.Send(message);
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("发送邮件异常-异常信息：{0},{1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),true);
                throw ex;
            }

        }
    }
}
