using MesaCore.Data;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace MesaCore.Services
{
    public class EmailServices
    {
        private readonly EmailSettings _emailSettings;

        public EmailServices(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string bodyHtml)
        {
            using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password);
                smtpClient.EnableSsl = true;
        
                var mailMessage = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
        
                mailMessage.To.Add(to);
        
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (SmtpException ex)
                {
                    throw new Exception($"Error al enviar el correo: {ex.Message}");
                }
            }
        }      
    }
}
