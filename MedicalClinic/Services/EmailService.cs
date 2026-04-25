using MimeKit;
using MailKit.Net.Smtp;

namespace MedicalClinic.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _configuration["Email:Host"],
                int.Parse(_configuration["Email:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendAppointmentConfirmationAsync(string toEmail,
            string patientName, string doctorName, DateTime appointmentTime)
        {
            var subject = "Confirmare Programare — MedicalClinic";
            var body = $@"
                <h2>Confirmare Programare</h2>
                <p>Bună ziua, <strong>{patientName}</strong>!</p>
                <p>Programarea ta a fost confirmată cu succes.</p>
                <table>
                    <tr><td><strong>Doctor:</strong></td><td>{doctorName}</td></tr>
                    <tr><td><strong>Data:</strong></td><td>{appointmentTime:dd/MM/yyyy HH:mm}</td></tr>
                </table>
                <p>Vă rugăm să anulați cu cel puțin 24 ore înainte dacă nu puteți ajunge.</p>
                <br/>
                <p>MedicalClinic</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAppointmentReminderAsync(string toEmail,
            string patientName, string doctorName, DateTime appointmentTime)
        {
            var subject = "Reminder Programare — MedicalClinic";
            var body = $@"
                <h2>Reminder Programare</h2>
                <p>Bună ziua, <strong>{patientName}</strong>!</p>
                <p>Vă reamintim că mâine aveți o programare.</p>
                <table>
                    <tr><td><strong>Doctor:</strong></td><td>{doctorName}</td></tr>
                    <tr><td><strong>Data:</strong></td><td>{appointmentTime:dd/MM/yyyy HH:mm}</td></tr>
                </table>
                <p>MedicalClinic</p>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}