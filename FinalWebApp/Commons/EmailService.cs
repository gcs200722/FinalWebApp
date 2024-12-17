using System.Net.Mail;
using System.Net;

namespace FinalWebApp.Commons
{

    public class EmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "thanhtung13032002@gmail.com";
        private readonly string _smtpPass = "anfo qxtq ovsk nkzx"; // Sử dụng App Password ở đây

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_smtpHost)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true // Đảm bảo sử dụng SSL
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Đảm bảo nội dung email là HTML
            };

            mailMessage.To.Add(toEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Xử lý lỗi nếu có (ví dụ: ghi log lỗi)
                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw;
            }
        }
    }

}
