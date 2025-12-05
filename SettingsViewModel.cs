using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FleetManager.Helpers;
using Microsoft.Win32;
using System.IO;
using System.Configuration;

namespace FleetManager.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        // Propriétés Base de données
        private string _databaseServer = "localhost";
        private string _databaseName = "fleet_manager";
        private Brush _connectionStatusColor = Brushes.Gray;
        private string _connectionStatusText = "Non testé";

        // Propriétés Alertes
        private bool _enableMaintenanceAlerts = true;
        private int _maintenanceAlertDays = 30;
        private bool _enableInsuranceAlerts = true;
        private int _insuranceAlertDays = 30;
        private bool _enableHighConsumptionAlerts = true;
        private double _highConsumptionThreshold = 20;

        // Propriétés Rapports
        private string _defaultDateFormat = "dd/MM/yyyy";
        private string _csvSeparator = "Point-virgule (;)";
        private string _defaultExportFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private bool _includeChartsInPdf = true;
        private bool _autoOpenExportedFiles = false;

        // Propriétés Affichage
        private string _itemsPerPage = "25";
        private bool _showChartsOnStartup = true;
        private bool _autoRefreshDashboard = false;
        private int _refreshInterval = 5;

        // Propriétés À propos
        private string _applicationVersion = "1.0.0";

        public SettingsViewModel()
        {
            // Commandes
            TestConnectionCommand = new AsyncRelayCommand(_ => TestConnectionAsync());
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            SaveCommand = new RelayCommand(_ => SaveSettings());
            CancelCommand = new RelayCommand(_ => CancelSettings());
            ResetToDefaultCommand = new RelayCommand(_ => ResetToDefault());

            LoadSettings();
        }

        #region Propriétés Base de données

        public string DatabaseServer
        {
            get => _databaseServer;
            set => SetProperty(ref _databaseServer, value);
        }

        public string DatabaseName
        {
            get => _databaseName;
            set => SetProperty(ref _databaseName, value);
        }

        public Brush ConnectionStatusColor
        {
            get => _connectionStatusColor;
            set => SetProperty(ref _connectionStatusColor, value);
        }

        public string ConnectionStatusText
        {
            get => _connectionStatusText;
            set => SetProperty(ref _connectionStatusText, value);
        }

        #endregion

        #region Propriétés Alertes

        public bool EnableMaintenanceAlerts
        {
            get => _enableMaintenanceAlerts;
            set => SetProperty(ref _enableMaintenanceAlerts, value);
        }

        public int MaintenanceAlertDays
        {
            get => _maintenanceAlertDays;
            set => SetProperty(ref _maintenanceAlertDays, value);
        }

        public bool EnableInsuranceAlerts
        {
            get => _enableInsuranceAlerts;
            set => SetProperty(ref _enableInsuranceAlerts, value);
        }

        public int InsuranceAlertDays
        {
            get => _insuranceAlertDays;
            set => SetProperty(ref _insuranceAlertDays, value);
        }

        public bool EnableHighConsumptionAlerts
        {
            get => _enableHighConsumptionAlerts;
            set => SetProperty(ref _enableHighConsumptionAlerts, value);
        }

        public double HighConsumptionThreshold
        {
            get => _highConsumptionThreshold;
            set => SetProperty(ref _highConsumptionThreshold, value);
        }

        #endregion

        #region Propriétés Rapports

        public string DefaultDateFormat
        {
            get => _defaultDateFormat;
            set => SetProperty(ref _defaultDateFormat, value);
        }

        public string CsvSeparator
        {
            get => _csvSeparator;
            set => SetProperty(ref _csvSeparator, value);
        }

        public string DefaultExportFolder
        {
            get => _defaultExportFolder;
            set => SetProperty(ref _defaultExportFolder, value);
        }

        public bool IncludeChartsInPdf
        {
            get => _includeChartsInPdf;
            set => SetProperty(ref _includeChartsInPdf, value);
        }

        public bool AutoOpenExportedFiles
        {
            get => _autoOpenExportedFiles;
            set => SetProperty(ref _autoOpenExportedFiles, value);
        }

        #endregion

        #region Propriétés Affichage

        public string ItemsPerPage
        {
            get => _itemsPerPage;
            set => SetProperty(ref _itemsPerPage, value);
        }

        public bool ShowChartsOnStartup
        {
            get => _showChartsOnStartup;
            set => SetProperty(ref _showChartsOnStartup, value);
        }

        public bool AutoRefreshDashboard
        {
            get => _autoRefreshDashboard;
            set => SetProperty(ref _autoRefreshDashboard, value);
        }

        public int RefreshInterval
        {
            get => _refreshInterval;
            set => SetProperty(ref _refreshInterval, value);
        }

        #endregion

        #region Propriétés À propos

        public string ApplicationVersion
        {
            get => _applicationVersion;
            set => SetProperty(ref _applicationVersion, value);
        }

        #endregion

        #region Commandes

        public ICommand TestConnectionCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetToDefaultCommand { get; }

        #endregion

        #region Méthodes

        private async System.Threading.Tasks.Task TestConnectionAsync()
        {
            try
            {
                ConnectionStatusColor = Brushes.Orange;
                ConnectionStatusText = "Test en cours...";

                // Simuler un test de connexion
                await System.Threading.Tasks.Task.Delay(1000);

                ConnectionStatusColor = Brushes.Green;
                ConnectionStatusText = "Connexion réussie";

                MessageBox.Show("Connexion à la base de données réussie!", "Test de connexion",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ConnectionStatusColor = Brushes.Red;
                ConnectionStatusText = "Échec de connexion";

                MessageBox.Show($"Erreur de connexion:\n{ex.Message}", "Test de connexion",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseFolder()
        {
            try
            {
                // Utiliser SaveFileDialog comme workaround pour sélectionner un dossier
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Sélectionner le dossier d'export",
                    FileName = "Dossier sélectionné", // Nom de fichier par défaut
                    DefaultExt = ".folder",
                    Filter = "Dossier|*.folder",
                    CheckFileExists = false,
                    CheckPathExists = true
                };

                if (!string.IsNullOrEmpty(DefaultExportFolder) && Directory.Exists(DefaultExportFolder))
                {
                    dialog.InitialDirectory = DefaultExportFolder;
                }

                if (dialog.ShowDialog() == true)
                {
                    // Extraire le chemin du dossier depuis le fichier sélectionné
                    DefaultExportFolder = System.IO.Path.GetDirectoryName(dialog.FileName) ?? DefaultExportFolder;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sélection du dossier:\n{ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Enregistrer les paramètres (ici on pourrait utiliser un fichier de configuration)
                // Pour l'instant, on simule juste la sauvegarde

                MessageBox.Show("Paramètres enregistrés avec succès!", "Enregistrement",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement:\n{ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelSettings()
        {
            var result = MessageBox.Show("Voulez-vous annuler les modifications?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CloseWindow();
            }
        }

        private void ResetToDefault()
        {
            var result = MessageBox.Show("Voulez-vous restaurer les valeurs par défaut?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Alertes
                EnableMaintenanceAlerts = true;
                MaintenanceAlertDays = 30;
                EnableInsuranceAlerts = true;
                InsuranceAlertDays = 30;
                EnableHighConsumptionAlerts = true;
                HighConsumptionThreshold = 20;

                // Rapports
                DefaultDateFormat = "dd/MM/yyyy";
                CsvSeparator = "Point-virgule (;)";
                DefaultExportFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                IncludeChartsInPdf = true;
                AutoOpenExportedFiles = false;

                // Affichage
                ItemsPerPage = "25";
                ShowChartsOnStartup = true;
                AutoRefreshDashboard = false;
                RefreshInterval = 5;

                MessageBox.Show("Valeurs par défaut restaurées!", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadSettings()
        {
            // Charger les paramètres depuis un fichier de configuration ou la base de données
            // Pour l'instant, on utilise les valeurs par défaut
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        #endregion
    }
}
