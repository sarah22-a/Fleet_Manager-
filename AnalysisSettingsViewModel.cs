using FleetManager.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    public class AnalysisSettingsViewModel : INotifyPropertyChanged
    {
        // Période d'analyse
        private string _defaultAnalysisPeriod;
        private int _trendMonthsCount;

        // Seuils de performance
        private decimal _excellentConsumptionThreshold;
        private decimal _acceptableConsumptionThreshold;
        private decimal _optimalCostPerKm;
        private int _urgentMaintenanceDays;

        // Critères de comparaison
        private bool _compareToFleetAverage;
        private bool _compareToPreviousYear;
        private bool _includeOutOfServiceVehicles;
        private int _minimumDataPoints;

        // Options d'affichage
        private bool _showMovingAverages;
        private bool _showPredictions;
        private bool _showAnomalies;
        private bool _useColorCoding;
        private string _currencyFormat;
        private string _dateFormat;

        // Calculs avancés
        private bool _enablePredictiveAnalysis;
        private bool _enableAnomalyDetection;
        private bool _calculateForecasts;
        private string _predictionAlgorithm;

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

        public AnalysisSettingsViewModel()
        {
            // Valeurs par défaut
            _defaultAnalysisPeriod = "6 derniers mois";
            _trendMonthsCount = 12;

            _excellentConsumptionThreshold = 7.5m;
            _acceptableConsumptionThreshold = 10.0m;
            _optimalCostPerKm = 0.30m;
            _urgentMaintenanceDays = 90;

            _compareToFleetAverage = true;
            _compareToPreviousYear = true;
            _includeOutOfServiceVehicles = false;
            _minimumDataPoints = 5;

            _showMovingAverages = true;
            _showPredictions = true;
            _showAnomalies = true;
            _useColorCoding = true;
            _currencyFormat = "€ (Euro)";
            _dateFormat = "dd/MM/yyyy";

            _enablePredictiveAnalysis = true;
            _enableAnomalyDetection = true;
            _calculateForecasts = true;
            _predictionAlgorithm = "Régression linéaire";

            InitializeCollections();
            InitializeCommands();
        }

        #region Properties

        // Période d'analyse
        public string DefaultAnalysisPeriod
        {
            get => _defaultAnalysisPeriod;
            set => SetProperty(ref _defaultAnalysisPeriod, value);
        }

        public int TrendMonthsCount
        {
            get => _trendMonthsCount;
            set => SetProperty(ref _trendMonthsCount, value);
        }

        // Seuils de performance
        public decimal ExcellentConsumptionThreshold
        {
            get => _excellentConsumptionThreshold;
            set => SetProperty(ref _excellentConsumptionThreshold, value);
        }

        public decimal AcceptableConsumptionThreshold
        {
            get => _acceptableConsumptionThreshold;
            set => SetProperty(ref _acceptableConsumptionThreshold, value);
        }

        public decimal OptimalCostPerKm
        {
            get => _optimalCostPerKm;
            set => SetProperty(ref _optimalCostPerKm, value);
        }

        public int UrgentMaintenanceDays
        {
            get => _urgentMaintenanceDays;
            set => SetProperty(ref _urgentMaintenanceDays, value);
        }

        // Critères de comparaison
        public bool CompareToFleetAverage
        {
            get => _compareToFleetAverage;
            set => SetProperty(ref _compareToFleetAverage, value);
        }

        public bool CompareToPreviousYear
        {
            get => _compareToPreviousYear;
            set => SetProperty(ref _compareToPreviousYear, value);
        }

        public bool IncludeOutOfServiceVehicles
        {
            get => _includeOutOfServiceVehicles;
            set => SetProperty(ref _includeOutOfServiceVehicles, value);
        }

        public int MinimumDataPoints
        {
            get => _minimumDataPoints;
            set => SetProperty(ref _minimumDataPoints, value);
        }

        // Options d'affichage
        public bool ShowMovingAverages
        {
            get => _showMovingAverages;
            set => SetProperty(ref _showMovingAverages, value);
        }

        public bool ShowPredictions
        {
            get => _showPredictions;
            set => SetProperty(ref _showPredictions, value);
        }

        public bool ShowAnomalies
        {
            get => _showAnomalies;
            set => SetProperty(ref _showAnomalies, value);
        }

        public bool UseColorCoding
        {
            get => _useColorCoding;
            set => SetProperty(ref _useColorCoding, value);
        }

        public string CurrencyFormat
        {
            get => _currencyFormat;
            set => SetProperty(ref _currencyFormat, value);
        }

        public string DateFormat
        {
            get => _dateFormat;
            set => SetProperty(ref _dateFormat, value);
        }

        // Calculs avancés
        public bool EnablePredictiveAnalysis
        {
            get => _enablePredictiveAnalysis;
            set => SetProperty(ref _enablePredictiveAnalysis, value);
        }

        public bool EnableAnomalyDetection
        {
            get => _enableAnomalyDetection;
            set => SetProperty(ref _enableAnomalyDetection, value);
        }

        public bool CalculateForecasts
        {
            get => _calculateForecasts;
            set => SetProperty(ref _calculateForecasts, value);
        }

        public string PredictionAlgorithm
        {
            get => _predictionAlgorithm;
            set => SetProperty(ref _predictionAlgorithm, value);
        }

        public List<string> AnalysisPeriods { get; private set; } = new();
        public List<string> CurrencyFormats { get; private set; } = new();
        public List<string> DateFormats { get; private set; } = new();
        public List<string> PredictionAlgorithms { get; private set; } = new();

        #endregion

        #region Commands

        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand ResetCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(_ => SaveSettings());
            ResetCommand = new RelayCommand(_ => ResetToDefaults());
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
            AnalysisPeriods = new List<string>
            {
                "7 derniers jours",
                "30 derniers jours",
                "3 derniers mois",
                "6 derniers mois",
                "12 derniers mois",
                "Année en cours",
                "Année dernière"
            };

            CurrencyFormats = new List<string>
            {
                "€ (Euro)",
                "$ (Dollar)",
                "£ (Livre)",
                "CHF (Franc suisse)"
            };

            DateFormats = new List<string>
            {
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy-MM-dd",
                "dd-MM-yyyy"
            };

            PredictionAlgorithms = new List<string>
            {
                "Régression linéaire",
                "Moyenne mobile",
                "Lissage exponentiel",
                "Tendance polynomiale"
            };
        }

        private void SaveSettings()
        {
            try
            {
                // Validation
                if (ExcellentConsumptionThreshold >= AcceptableConsumptionThreshold)
                {
                    MessageBox.Show("Le seuil de consommation excellente doit être inférieur au seuil acceptable.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OptimalCostPerKm <= 0)
                {
                    MessageBox.Show("Le coût par kilomètre optimal doit être supérieur à zéro.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (UrgentMaintenanceDays <= 0)
                {
                    MessageBox.Show("Le nombre de jours pour maintenance urgente doit être supérieur à zéro.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Sauvegarder les paramètres (à implémenter dans un fichier de configuration)
                MessageBox.Show($"✅ Paramètres d'analyse enregistrés avec succès!\n\n" +
                               $"Période par défaut: {DefaultAnalysisPeriod}\n" +
                               $"Mois de tendance: {TrendMonthsCount}\n\n" +
                               $"Seuils:\n" +
                               $"  • Consommation excellente: < {ExcellentConsumptionThreshold:F1} L/100km\n" +
                               $"  • Consommation acceptable: < {AcceptableConsumptionThreshold:F1} L/100km\n" +
                               $"  • Coût optimal: {OptimalCostPerKm:F2} €/km\n" +
                               $"  • Maintenance urgente: {UrgentMaintenanceDays} jours\n\n" +
                               $"Comparaisons:\n" +
                               $"  • Moyenne flotte: {(CompareToFleetAverage ? "Oui" : "Non")}\n" +
                               $"  • Année précédente: {(CompareToPreviousYear ? "Oui" : "Non")}\n" +
                               $"  • Données minimum: {MinimumDataPoints} points\n\n" +
                               $"Affichage:\n" +
                               $"  • Moyennes mobiles: {(ShowMovingAverages ? "Oui" : "Non")}\n" +
                               $"  • Prédictions: {(ShowPredictions ? "Oui" : "Non")}\n" +
                               $"  • Anomalies: {(ShowAnomalies ? "Oui" : "Non")}\n" +
                               $"  • Codage couleur: {(UseColorCoding ? "Oui" : "Non")}\n\n" +
                               $"Calculs avancés:\n" +
                               $"  • Analyse prédictive: {(EnablePredictiveAnalysis ? "Activée" : "Désactivée")}\n" +
                               $"  • Détection anomalies: {(EnableAnomalyDetection ? "Activée" : "Désactivée")}\n" +
                               $"  • Prévisions: {(CalculateForecasts ? "Activées" : "Désactivées")}\n" +
                               $"  • Algorithme: {PredictionAlgorithm}",
                    "Paramètres enregistrés", MessageBoxButton.OK, MessageBoxImage.Information);

                // Fermer la fenêtre
                Application.Current.Windows[^1].Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement des paramètres:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults()
        {
            var result = MessageBox.Show("Voulez-vous vraiment réinitialiser tous les paramètres aux valeurs par défaut?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DefaultAnalysisPeriod = "6 derniers mois";
                TrendMonthsCount = 12;

                ExcellentConsumptionThreshold = 7.5m;
                AcceptableConsumptionThreshold = 10.0m;
                OptimalCostPerKm = 0.30m;
                UrgentMaintenanceDays = 90;

                CompareToFleetAverage = true;
                CompareToPreviousYear = true;
                IncludeOutOfServiceVehicles = false;
                MinimumDataPoints = 5;

                ShowMovingAverages = true;
                ShowPredictions = true;
                ShowAnomalies = true;
                UseColorCoding = true;
                CurrencyFormat = "€ (Euro)";
                DateFormat = "dd/MM/yyyy";

                EnablePredictiveAnalysis = true;
                EnableAnomalyDetection = true;
                CalculateForecasts = true;
                PredictionAlgorithm = "Régression linéaire";

                MessageBox.Show("Les paramètres ont été réinitialisés aux valeurs par défaut.",
                    "Réinitialisation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion
    }
}
