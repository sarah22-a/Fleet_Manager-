using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FleetManager.Services
{
    /// <summary>
    /// Interface pour le service d'envoi d'emails
    /// </summary>
    public interface IEmailService
    {
        Task<(bool success, string message)> SendEmailAsync(string to, string subject, string body);
        Task<(bool success, string message)> SendReportAsync(string to, string reportFilePath, string reportName);
    }

    /// <summary>
    /// Service d'envoi d'emails
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _senderEmail = "fleet.manager.noreply@gmail.com";
        private readonly string _senderPassword = ""; // À configurer dans appsettings.json

        public EmailService()
        {
            // Charger depuis configuration si disponible
            try
            {
                _senderEmail = System.Configuration.ConfigurationManager.AppSettings["EmailService:SenderEmail"] ?? _senderEmail;
                _senderPassword = System.Configuration.ConfigurationManager.AppSettings["EmailService:SenderPassword"] ?? _senderPassword;
            }
            catch
            {
                // Utiliser les valeurs par défaut
            }
        }

        /// <summary>
        /// Envoyer un email simple
        /// </summary>
        public async Task<(bool, string)> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(_senderPassword))
                {
                    return (false, "Service d'email non configuré (mot de passe absent)");
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var mailMessage = new MailMessage(_senderEmail, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    };

                    await client.SendMailAsync(mailMessage);
                }

                return (true, $"Email envoyé avec succès à {to}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur d'envoi d'email: {ex.Message}");
                return (false, $"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Envoyer un rapport en pièce jointe
        /// </summary>
        public async Task<(bool, string)> SendReportAsync(string to, string reportFilePath, string reportName)
        {
            try
            {
                if (string.IsNullOrEmpty(_senderPassword))
                {
                    return (false, "Service d'email non configuré (mot de passe absent)");
                }

                if (!System.IO.File.Exists(reportFilePath))
                {
                    return (false, "Fichier rapport non trouvé");
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var mailMessage = new MailMessage(_senderEmail, to)
                    {
                        Subject = $"Rapport Fleet Manager - {reportName}",
                        Body = $"Veuillez trouver ci-joint le rapport de gestion de parc: {reportName}\n\n" +
                               $"Généré le: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n" +
                               "Cordialement,\nFleet Manager",
                        IsBodyHtml = false
                    };

                    // Ajouter la pièce jointe
                    mailMessage.Attachments.Add(new Attachment(reportFilePath));

                    await client.SendMailAsync(mailMessage);
                }

                return (true, $"Rapport envoyé avec succès à {to}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur d'envoi de rapport: {ex.Message}");
                return (false, $"Erreur: {ex.Message}");
            }
        }
    }
}
