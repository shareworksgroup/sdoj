using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            // no-reply@sdcb.in
            var from = AppSettings.EmailFrom;

            // 123456
            var password = AppSettings.EmailPassword;

            // stmp.ym.163.com
            var host = AppSettings.SmtpHost;

            // 25
            var port = AppSettings.SmtpPort;
            
            // DestinationΪ�ռ��䣬��397482054@qq.com
            var mail = new MailMessage(from, message.Destination)
            {
                Subject = message.Subject, // ����
                Body = message.Body, // ����
                IsBodyHtml = true, // �����Ƿ�ΪHTML��ʽ
            };

            using (var client = new SmtpClient(host, port))
            {
                // from����������ַ��password������
                client.Credentials = new NetworkCredential(from, password);
                client.EnableSsl = true;

                // ����
                await client.SendMailAsync(mail);
            }
        }
    }
}