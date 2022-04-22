using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace ElectronicProtocolStartLine.Class
{
    public static class Email
    {
        public static void SendEmailFuji(string subject, string Content)
        {
            var view = AlternateView.CreateAlternateViewFromString(Content, Encoding.UTF8, MediaTypeNames.Text.Html);

            var listemails = new FASEntities().EP_Email.Where(c => c.Type == "Фуджи").Select(c => c.Email);

            using (var client = new SmtpClient("mail.technopolis.gs", 25)) // Set properties as needed or use config file
            using (MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,

            })

            {
                message.AlternateViews.Add(view);
                message.From = new MailAddress("reporter@dtvs.ru", "ROBOT");
                //message.CC.Add("a.volodin@dtvs.ru");
                foreach (var item in listemails) message.CC.Add(item);
                //message.CC.Add("Лишик Станислав Александрович <lishik@dtvs.ru>");
                //message.CC.Add("Мелехин Константин Данилович <melekhin@dtvs.ru>");
                //message.CC.Add("Овчинников Дмитрий Игоревич <ovchinnikov@dtvs.ru>");
                //message.CC.Add("Шишкин Игорь Алексеевич <i.shishkin@dtvs.ru>");
                client.Send(message);
            }


        }
        public static void SendEmailProtocol(string subject, string Content)
        {
            var view = AlternateView.CreateAlternateViewFromString(Content, Encoding.UTF8, MediaTypeNames.Text.Html);

            var listemails = new FASEntities().EP_Email.Where(c=>c.Type == "ПДУ").Select(c=>c.Email);

            using (var client = new SmtpClient("mail.technopolis.gs", 25)) // Set properties as needed or use config file
            using (MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,

            })

            {
                message.AlternateViews.Add(view);
                message.From = new MailAddress("reporter@dtvs.ru", "ROBOT");
                foreach (var item in listemails)  message.CC.Add(item);                
                
                client.Send(message);
            }


        }
    }
}