using FleetManager.Services;
using FleetManager.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    public class SendReportViewModel : INotifyPropertyChanged
    {
        private readonly ExportService _exportService;
        private readonly StatisticsService _statisticsService;

        private string _recipientEmail;
        private string _recipientName;
        private string _subject;
        private string _message;
        private string _selectedReportType;
        private string _selectedFormat;
        private DateTime _startDate;
        private DateTime _endDate;
        private bool _includeGraphs;
        private bool _includeDetails;
        private bool _isSending;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public SendReportViewModel(ExportService exportService, StatisticsService statisticsService)
        {
            _exportService = exportService;
            _statisticsService = statisticsService;

            // Valeurs par défaut
            _recipientEmail = "";
            _recipientName = "";
            _subject = "Rapport Fleet Manager";
            _message = "Veuillez trouver ci-joint le rapport de gestion de flotte.";
            _selectedReportType = "Rapport mensuel";
            _selectedFormat = "PDF";
            _startDate = DateTime.Now.AddMonths(-1);
            _endDate = DateTime.Now;
            _includeGraphs = true;
            _includeDetails = true;
            _isSending = false;

            InitializeCollections();
            InitializeCommands();
        }

        #region Properties

        public string RecipientEmail
        {
            get => _recipientEmail;
            set => SetProperty(ref _recipientEmail, value);
        }

        public string RecipientName
        {
            get => _recipientName;
            set => SetProperty(ref _recipientName, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (SetProperty(ref _selectedReportType, value))
                    UpdateDateRangeForReportType(value);
            }
        }

        public string SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public bool IncludeGraphs
        {
            get => _includeGraphs;
            set => SetProperty(ref _includeGraphs, value);
        }

        public bool IncludeDetails
        {
            get => _includeDetails;
            set => SetProperty(ref _includeDetails, value);
        }

        public bool IsSending
        {
            get => _isSending;
            set => SetProperty(ref _isSending, value);
        }

        public List<string> ReportTypes { get; private set; } = new();
        public List<string> Formats { get; private set; } = new();

        #endregion

        #region Commands

        public ICommand SendCommand { get; private set; } = null!;
        public ICommand PreviewCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            SendCommand = new RelayCommand(_ => SendReport(), _ => CanSendReport());
            PreviewCommand = new RelayCommand(_ => PreviewReport());
            CancelCommand = new RelayCommand(param =>
            {
                if (param is Window window)
                    window.Close();
            });
        }

        #endregion

        #region Methods

        private void InitializeCollections()
        {
            ReportTypes = new List<string>
            {
                "Rapport journalier",
                "Rapport hebdomadaire",
                "Rapport mensuel",
                "Rapport trimestriel",
                "Rapport annuel",
                "Rapport personnalisé"
            };

            Formats = new List<string>
            {
                "PDF",
                "Excel",
                "CSV"
            };
        }

        private void UpdateDateRangeForReportType(string reportType)
        {
            var endDate = DateTime.Now;
            var startDate = reportType switch
            {
                "Rapport journalier" => endDate.AddDays(-1),
                "Rapport hebdomadaire" => endDate.AddDays(-7),
                "Rapport mensuel" => endDate.AddMonths(-1),
                "Rapport trimestriel" => endDate.AddMonths(-3),
                "Rapport annuel" => endDate.AddYears(-1),
                _ => endDate.AddMonths(-1)
            };

            StartDate = startDate;
            EndDate = endDate;
        }

        private bool CanSendReport()
        {
            return !string.IsNullOrWhiteSpace(RecipientEmail) &&
                   RecipientEmail.Contains("@") &&
                   !string.IsNullOrWhiteSpace(Subject) &&
                   !IsSending;
        }

        private void SendReport()
        {
            try
            {
                IsSending = true;

                // Validation
                if (!IsValidEmail(RecipientEmail))
                {
                    MessageBox.Show("Veuillez saisir une adresse email valide.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Simuler l'envoi d'email
                MessageBox.Show($"📧 Fonctionnalité en cours de développement\n\n" +
                               $"Le rapport sera envoyé à : {RecipientEmail}\n" +
                               $"Format : {SelectedFormat}\n" +
                               $"Type : {SelectedReportType}\n" +
                               $"Période : {StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}\n\n" +
                               $"Pour activer l'envoi par email, configurez un serveur SMTP dans les paramètres de l'application.",
                    "Envoi de rapport", MessageBoxButton.OK, MessageBoxImage.Information);

                // Fermer la fenêtre après succès
                Application.Current.Windows[^1].Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du rapport:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSending = false;
            }
        }

        private void PreviewReport()
        {
            try
            {
                MessageBox.Show($"Prévisualisation du rapport\n\n" +
                               $"Type : {SelectedReportType}\n" +
                               $"Format : {SelectedFormat}\n" +
                               $"Période : {StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}\n" +
                               $"Graphiques : {(IncludeGraphs ? "Oui" : "Non")}\n" +
                               $"Détails : {(IncludeDetails ? "Oui" : "Non")}\n\n" +
                               $"La prévisualisation complète sera disponible prochainement.",
                    "Prévisualisation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la prévisualisation:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
