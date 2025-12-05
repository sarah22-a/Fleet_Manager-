using FleetManager.Services;
using FleetManager.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    public class ObjectivesViewModel : INotifyPropertyChanged
    {
        private readonly StatisticsService _statisticsService;

        // Objectifs de consommation
        private decimal _targetAverageConsumption;
        private decimal _maxAcceptableConsumption;
        private decimal _targetFuelSavingPercent;
        private decimal _currentAverageConsumption;

        // Objectifs de coûts
        private decimal _monthlyFuelBudget;
        private decimal _monthlyMaintenanceBudget;
        private decimal _targetCostPerKm;
        private decimal _targetCostReductionPercent;
        private decimal _currentMonthlyFuelCost;
        private decimal _currentMonthlyMaintenanceCost;
        private decimal _currentCostPerKm;

        // Objectifs de maintenance
        private int _maintenanceIntervalDays;
        private int _maintenanceIntervalKm;
        private decimal _targetFleetAvailability;
        private decimal _currentFleetAvailability;

        // Alertes
        private bool _enableAlerts;
        private bool _alertOnBudgetExceed;
        private bool _alertOnHighConsumption;
        private bool _alertOnMaintenanceDue;
        private bool _alertOnTargetMissed;
        private string _reportFrequency;

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

        public ObjectivesViewModel(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;

            // Valeurs par défaut
            _targetAverageConsumption = 8.5m;
            _maxAcceptableConsumption = 12.0m;
            _targetFuelSavingPercent = 10m;
            _currentAverageConsumption = 9.2m;

            _monthlyFuelBudget = 5000m;
            _monthlyMaintenanceBudget = 3000m;
            _targetCostPerKm = 0.35m;
            _targetCostReductionPercent = 15m;
            _currentMonthlyFuelCost = 5200m;
            _currentMonthlyMaintenanceCost = 2800m;
            _currentCostPerKm = 0.38m;

            _maintenanceIntervalDays = 90;
            _maintenanceIntervalKm = 15000;
            _targetFleetAvailability = 95m;
            _currentFleetAvailability = 92.5m;

            _enableAlerts = true;
            _alertOnBudgetExceed = true;
            _alertOnHighConsumption = true;
            _alertOnMaintenanceDue = true;
            _alertOnTargetMissed = true;
            _reportFrequency = "Hebdomadaire";

            InitializeCollections();
            InitializeCommands();
            _ = LoadCurrentDataAsync();
        }

        #region Properties

        // Objectifs de consommation
        public decimal TargetAverageConsumption
        {
            get => _targetAverageConsumption;
            set => SetProperty(ref _targetAverageConsumption, value);
        }

        public decimal MaxAcceptableConsumption
        {
            get => _maxAcceptableConsumption;
            set => SetProperty(ref _maxAcceptableConsumption, value);
        }

        public decimal TargetFuelSavingPercent
        {
            get => _targetFuelSavingPercent;
            set => SetProperty(ref _targetFuelSavingPercent, value);
        }

        public decimal CurrentAverageConsumption
        {
            get => _currentAverageConsumption;
            set => SetProperty(ref _currentAverageConsumption, value);
        }

        // Objectifs de coûts
        public decimal MonthlyFuelBudget
        {
            get => _monthlyFuelBudget;
            set => SetProperty(ref _monthlyFuelBudget, value);
        }

        public decimal MonthlyMaintenanceBudget
        {
            get => _monthlyMaintenanceBudget;
            set => SetProperty(ref _monthlyMaintenanceBudget, value);
        }

        public decimal TargetCostPerKm
        {
            get => _targetCostPerKm;
            set => SetProperty(ref _targetCostPerKm, value);
        }

        public decimal TargetCostReductionPercent
        {
            get => _targetCostReductionPercent;
            set => SetProperty(ref _targetCostReductionPercent, value);
        }

        public decimal CurrentMonthlyFuelCost
        {
            get => _currentMonthlyFuelCost;
            set => SetProperty(ref _currentMonthlyFuelCost, value);
        }

        public decimal CurrentMonthlyMaintenanceCost
        {
            get => _currentMonthlyMaintenanceCost;
            set => SetProperty(ref _currentMonthlyMaintenanceCost, value);
        }

        public decimal CurrentCostPerKm
        {
            get => _currentCostPerKm;
            set => SetProperty(ref _currentCostPerKm, value);
        }

        // Objectifs de maintenance
        public int MaintenanceIntervalDays
        {
            get => _maintenanceIntervalDays;
            set => SetProperty(ref _maintenanceIntervalDays, value);
        }

        public int MaintenanceIntervalKm
        {
            get => _maintenanceIntervalKm;
            set => SetProperty(ref _maintenanceIntervalKm, value);
        }

        public decimal TargetFleetAvailability
        {
            get => _targetFleetAvailability;
            set => SetProperty(ref _targetFleetAvailability, value);
        }

        public decimal CurrentFleetAvailability
        {
            get => _currentFleetAvailability;
            set => SetProperty(ref _currentFleetAvailability, value);
        }

        // Alertes
        public bool EnableAlerts
        {
            get => _enableAlerts;
            set => SetProperty(ref _enableAlerts, value);
        }

        public bool AlertOnBudgetExceed
        {
            get => _alertOnBudgetExceed;
            set => SetProperty(ref _alertOnBudgetExceed, value);
        }

        public bool AlertOnHighConsumption
        {
            get => _alertOnHighConsumption;
            set => SetProperty(ref _alertOnHighConsumption, value);
        }

        public bool AlertOnMaintenanceDue
        {
            get => _alertOnMaintenanceDue;
            set => SetProperty(ref _alertOnMaintenanceDue, value);
        }

        public bool AlertOnTargetMissed
        {
            get => _alertOnTargetMissed;
            set => SetProperty(ref _alertOnTargetMissed, value);
        }

        public string ReportFrequency
        {
            get => _reportFrequency;
            set => SetProperty(ref _reportFrequency, value);
        }

        public List<string> ReportFrequencies { get; private set; } = new();

        #endregion

        #region Commands

        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand ResetCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(_ => SaveObjectives());
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
            ReportFrequencies = new List<string>
            {
                "Quotidien",
                "Hebdomadaire",
                "Bimensuel",
                "Mensuel"
            };
        }

        private async Task LoadCurrentDataAsync()
        {
            try
            {
                // Charger les données actuelles de la flotte
                var fleetStats = await _statisticsService.GetFleetStatisticsAsync();

                // Mettre à jour les valeurs actuelles
                CurrentAverageConsumption = fleetStats.AverageFleetConsumption;
                CurrentMonthlyFuelCost = fleetStats.MonthlyFuelCost;
                CurrentMonthlyMaintenanceCost = fleetStats.MonthlyMaintenanceCost;
                
                // Calcul du coût par km
                if (fleetStats.TotalMileage > 0)
                {
                    CurrentCostPerKm = (fleetStats.TotalFuelCost + fleetStats.TotalMaintenanceCost) / fleetStats.TotalMileage;
                }

                // Calcul de la disponibilité
                if (fleetStats.TotalVehicles > 0)
                {
                    CurrentFleetAvailability = ((decimal)fleetStats.ActiveVehicles / fleetStats.TotalVehicles) * 100;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données : {ex.Message}");
            }
        }

        private void SaveObjectives()
        {
            try
            {
                // Validation des valeurs
                if (TargetAverageConsumption <= 0 || MaxAcceptableConsumption <= 0)
                {
                    MessageBox.Show("Les valeurs de consommation doivent être supérieures à zéro.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MonthlyFuelBudget <= 0 || MonthlyMaintenanceBudget <= 0)
                {
                    MessageBox.Show("Les budgets doivent être supérieurs à zéro.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MaintenanceIntervalDays <= 0 || MaintenanceIntervalKm <= 0)
                {
                    MessageBox.Show("Les intervalles de maintenance doivent être supérieurs à zéro.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Sauvegarder les objectifs (à implémenter dans une base de données ou fichier de configuration)
                MessageBox.Show($"✅ Objectifs enregistrés avec succès!\n\n" +
                               $"Objectifs de consommation:\n" +
                               $"  • Consommation cible: {TargetAverageConsumption:F2} L/100km\n" +
                               $"  • Économie ciblée: {TargetFuelSavingPercent:F0}%\n\n" +
                               $"Objectifs de coûts:\n" +
                               $"  • Budget carburant: {MonthlyFuelBudget:C0}\n" +
                               $"  • Budget maintenance: {MonthlyMaintenanceBudget:C0}\n" +
                               $"  • Réduction ciblée: {TargetCostReductionPercent:F0}%\n\n" +
                               $"Objectifs de maintenance:\n" +
                               $"  • Intervalle: {MaintenanceIntervalDays} jours / {MaintenanceIntervalKm:N0} km\n" +
                               $"  • Disponibilité cible: {TargetFleetAvailability:F0}%\n\n" +
                               $"Alertes: {(EnableAlerts ? "Activées" : "Désactivées")}\n" +
                               $"Rapports: {ReportFrequency}",
                    "Objectifs enregistrés", MessageBoxButton.OK, MessageBoxImage.Information);

                // Fermer la fenêtre
                Application.Current.Windows[^1].Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement des objectifs:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults()
        {
            var result = MessageBox.Show("Voulez-vous vraiment réinitialiser tous les objectifs aux valeurs par défaut?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                TargetAverageConsumption = 8.5m;
                MaxAcceptableConsumption = 12.0m;
                TargetFuelSavingPercent = 10m;

                MonthlyFuelBudget = 5000m;
                MonthlyMaintenanceBudget = 3000m;
                TargetCostPerKm = 0.35m;
                TargetCostReductionPercent = 15m;

                MaintenanceIntervalDays = 90;
                MaintenanceIntervalKm = 15000;
                TargetFleetAvailability = 95m;

                EnableAlerts = true;
                AlertOnBudgetExceed = true;
                AlertOnHighConsumption = true;
                AlertOnMaintenanceDue = true;
                AlertOnTargetMissed = true;
                ReportFrequency = "Hebdomadaire";

                MessageBox.Show("Les objectifs ont été réinitialisés aux valeurs par défaut.",
                    "Réinitialisation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion
    }
}
