using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using FleetManager.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using FleetManager.Models;
using FleetManager.Services;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FleetManager.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly VehicleService _vehicleService;
        private readonly FuelService _fuelService;
        private readonly StatisticsService _statisticsService;
        private readonly ExportService _exportService;

        // Propriétés pour les indicateurs clés
        private int _totalVehicles;
        private int _activeVehicles;
        private int _vehiclesInMaintenance;
        private int _totalFuelRecords;
        private decimal _totalFuelCost;
        private decimal _averageFuelConsumption;
        private decimal _monthlyFuelCost;
        private decimal _totalMaintenanceCost;
        private decimal _monthlyMaintenanceCost;

        // Collections pour les données
        private ObservableCollection<VehicleStatistics> _topVehiclesByConsumption = new();
        private ObservableCollection<VehicleStatistics> _topVehiclesByCost = new();
        private ObservableCollection<RecentMovement> _recentMovements = new();
        private ObservableCollection<DashboardAlert> _alerts = new();
        private ObservableCollection<MonthlyStatistics> _monthlyTrends = new();
        private ObservableCollection<VehicleTypeStatistics> _vehicleTypeStats = new();
        private ObservableCollection<FuelTypeStatistics> _fuelTypeStats = new();

        // Propriétés pour les graphiques
        private ObservableCollection<TimeSeriesData> _consumptionTrend = new();
        private ObservableCollection<TimeSeriesData> _costTrend = new();

        // LiveCharts series & labels pour le dashboard (initialiser pour éviter NullReference)
        private IEnumerable<ISeries> _consumptionSeries = new List<ISeries>
        {
            new LineSeries<double> { Values = new double[] { }, Name = "Consommation" }
        };
        private IEnumerable<ISeries> _costSeries = new List<ISeries>
        {
            new ColumnSeries<double> { Values = new double[] { }, Name = "Coûts" }
        };
        private IEnumerable<ISeries> _monthlyTrendsSeries = new List<ISeries>
        {
            new ColumnSeries<double> { Values = new double[] { }, Name = "Carburant" },
            new ColumnSeries<double> { Values = new double[] { }, Name = "Maintenance" }
        };
        private string[] _trendLabels = Array.Empty<string>();

        // Propriétés pour les statistiques annuelles
        private decimal _yearlyFuelTotal;
        private decimal _yearlyMaintenanceTotal;
        private decimal _yearlyOperatingCost;
        private decimal _monthlyAverageCost;

        // État de chargement
        private bool _isLoading;
        private string _lastUpdated = string.Empty;

        public DashboardViewModel(
            VehicleService vehicleService,
            FuelService fuelService,
            StatisticsService statisticsService,
            ExportService exportService)
        {
            _vehicleService = vehicleService;
            _fuelService = fuelService;
            _statisticsService = statisticsService;
            _exportService = exportService;

            LoadDataCommand = new AsyncRelayCommand(_ => LoadDataAsync(null));
            RefreshCommand = new AsyncRelayCommand(_ => RefreshDataAsync(null));
            ViewDetailedStatisticsCommand = new RelayCommand(_ => ViewDetailedStatistics());
            GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
            ExportDataCommand = new AsyncRelayCommand(ExportDataAsync);
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // Charger les données au démarrage
            _ = LoadDataAsync(null);
        }

        #region Propriétés des indicateurs clés

        public int TotalVehicles
        {
            get => _totalVehicles;
            set => SetProperty(ref _totalVehicles, value);
        }

        public int ActiveVehicles
        {
            get => _activeVehicles;
            set => SetProperty(ref _activeVehicles, value);
        }

        public int VehiclesInMaintenance
        {
            get => _vehiclesInMaintenance;
            set => SetProperty(ref _vehiclesInMaintenance, value);
        }

        public int TotalFuelRecords
        {
            get => _totalFuelRecords;
            set => SetProperty(ref _totalFuelRecords, value);
        }

        public decimal TotalFuelCost
        {
            get => _totalFuelCost;
            set => SetProperty(ref _totalFuelCost, value);
        }

        public decimal AverageFuelConsumption
        {
            get => _averageFuelConsumption;
            set => SetProperty(ref _averageFuelConsumption, value);
        }

        public decimal MonthlyFuelCost
        {
            get => _monthlyFuelCost;
            set => SetProperty(ref _monthlyFuelCost, value);
        }

        public decimal TotalMaintenanceCost
        {
            get => _totalMaintenanceCost;
            set => SetProperty(ref _totalMaintenanceCost, value);
        }

        public decimal MonthlyMaintenanceCost
        {
            get => _monthlyMaintenanceCost;
            set => SetProperty(ref _monthlyMaintenanceCost, value);
        }

        #endregion

        #region Collections pour les données

        public ObservableCollection<VehicleStatistics> TopVehiclesByConsumption
        {
            get => _topVehiclesByConsumption;
            set => SetProperty(ref _topVehiclesByConsumption, value);
        }

        public ObservableCollection<VehicleStatistics> TopVehiclesByCost
        {
            get => _topVehiclesByCost;
            set => SetProperty(ref _topVehiclesByCost, value);
        }

        public ObservableCollection<RecentMovement> RecentMovements
        {
            get => _recentMovements;
            set => SetProperty(ref _recentMovements, value);
        }

        public ObservableCollection<DashboardAlert> Alerts
        {
            get => _alerts;
            set => SetProperty(ref _alerts, value);
        }

        public ObservableCollection<MonthlyStatistics> MonthlyTrends
        {
            get => _monthlyTrends;
            set => SetProperty(ref _monthlyTrends, value);
        }

        public ObservableCollection<VehicleTypeStatistics> VehicleTypeStats
        {
            get => _vehicleTypeStats;
            set => SetProperty(ref _vehicleTypeStats, value);
        }

        public ObservableCollection<FuelTypeStatistics> FuelTypeStats
        {
            get => _fuelTypeStats;
            set => SetProperty(ref _fuelTypeStats, value);
        }

        public ObservableCollection<TimeSeriesData> ConsumptionTrend
        {
            get => _consumptionTrend;
            set => SetProperty(ref _consumptionTrend, value);
        }

        public ObservableCollection<TimeSeriesData> CostTrend
        {
            get => _costTrend;
            set => SetProperty(ref _costTrend, value);
        }

        // Séries exposées pour LiveCharts
        public IEnumerable<ISeries> ConsumptionSeries
        {
            get => _consumptionSeries;
            set => SetProperty(ref _consumptionSeries, value);
        }

        public IEnumerable<ISeries> CostSeries
        {
            get => _costSeries;
            set => SetProperty(ref _costSeries, value);
        }

        public IEnumerable<ISeries> MonthlyTrendsSeries
        {
            get => _monthlyTrendsSeries;
            set => SetProperty(ref _monthlyTrendsSeries, value);
        }

        public string[] TrendLabels
        {
            get => _trendLabels;
            set => SetProperty(ref _trendLabels, value);
        }

        // Propriétés pour les statistiques annuelles
        public decimal YearlyFuelTotal
        {
            get => _yearlyFuelTotal;
            set => SetProperty(ref _yearlyFuelTotal, value);
        }

        public decimal YearlyMaintenanceTotal
        {
            get => _yearlyMaintenanceTotal;
            set => SetProperty(ref _yearlyMaintenanceTotal, value);
        }

        public decimal YearlyOperatingCost
        {
            get => _yearlyOperatingCost;
            set => SetProperty(ref _yearlyOperatingCost, value);
        }

        public decimal MonthlyAverageCost
        {
            get => _monthlyAverageCost;
            set => SetProperty(ref _monthlyAverageCost, value);
        }

        #endregion

        #region Propriétés d'état

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public int AlertCount => Alerts?.Count ?? 0;
        public int CriticalAlerts => Alerts?.Count(a => a.Priority == AlertPriority.Critical) ?? 0;
        public int HighPriorityAlerts => Alerts?.Count(a => a.Priority == AlertPriority.High) ?? 0;

        // Propriétés calculées pour les pourcentages et ratios
        public decimal ActiveVehiclePercentage => TotalVehicles > 0 ? (decimal)ActiveVehicles / TotalVehicles * 100 : 0;
        public decimal MaintenanceVehiclePercentage => TotalVehicles > 0 ? (decimal)VehiclesInMaintenance / TotalVehicles * 100 : 0;
        public decimal TotalMonthlyCost => MonthlyFuelCost + MonthlyMaintenanceCost;
        public decimal FuelToMaintenanceRatio => TotalMaintenanceCost > 0 ? TotalFuelCost / TotalMaintenanceCost : 0;

        #endregion

        #region Commandes

        public ICommand LoadDataCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewDetailedStatisticsCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        #endregion

        #region Méthodes de chargement des données

        private async Task LoadDataAsync(object? parameter)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("Chargement des données du dashboard...");

                // Obtenir toutes les données du dashboard en une seule fois
                var dashboardData = await _statisticsService.GetDashboardDataAsync();

                // Mettre à jour les indicateurs clés
                UpdateFleetStatistics(dashboardData.FleetStats);

                // Mettre à jour les collections
                TopVehiclesByConsumption = new ObservableCollection<VehicleStatistics>(dashboardData.TopVehiclesByConsumption);
                TopVehiclesByCost = new ObservableCollection<VehicleStatistics>(dashboardData.TopVehiclesByCost);
                MonthlyTrends = new ObservableCollection<MonthlyStatistics>(dashboardData.MonthlyTrends);
                VehicleTypeStats = new ObservableCollection<VehicleTypeStatistics>(dashboardData.TypeBreakdown);
                FuelTypeStats = new ObservableCollection<FuelTypeStatistics>(dashboardData.FuelBreakdown);
                Alerts = new ObservableCollection<DashboardAlert>(dashboardData.Alerts);
                ConsumptionTrend = new ObservableCollection<TimeSeriesData>(dashboardData.ConsumptionTrend);
                CostTrend = new ObservableCollection<TimeSeriesData>(dashboardData.CostTrend);

                // Préparer les séries et labels pour LiveCharts (pour affichage rapide)
                try
                {
                    TrendLabels = ConsumptionTrend.Select(t => t.Date.ToString("dd/MM")).ToArray();

                    var consumptionValues = ConsumptionTrend.Select(t => (double)t.Value).ToArray();
                    ConsumptionSeries = new ISeries[]
                    {
                        new LineSeries<double>
                        {
                            Values = consumptionValues,
                            Name = "Consommation",
                            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 2),
                            GeometrySize = 4,
                            Fill = null,
                            YToolTipLabelFormatter = point => 
                            {
                                var label = TrendLabels.ElementAtOrDefault((int)point.Index) ?? "N/A";
                                return $"{label}: {point.Coordinate.PrimaryValue:F2} L/100km";
                            }
                        }
                    };

                    var costValues = CostTrend.Select(t => (double)t.Value).ToArray();
                    CostSeries = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Values = costValues,
                            Name = "Coûts",
                            Fill = new SolidColorPaint(SKColors.Orange),
                            Stroke = new SolidColorPaint(SKColors.DarkOrange, 1),
                            YToolTipLabelFormatter = point => 
                            {
                                var label = TrendLabels.ElementAtOrDefault((int)point.Index) ?? "N/A";
                                return $"{label}: {point.Coordinate.PrimaryValue:C0}";
                            }
                        }
                    };

                    // Créer les séries pour l'évolution mensuelle (12 mois)
                    if (MonthlyTrends != null && MonthlyTrends.Any())
                    {
                        var fuelCostValues = MonthlyTrends.Select(m => (double)m.FuelCost).ToArray();
                        var maintenanceCostValues = MonthlyTrends.Select(m => (double)m.MaintenanceCost).ToArray();
                        var monthLabels = MonthlyTrends.Select(m => new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy")).ToArray();

                        MonthlyTrendsSeries = new ISeries[]
                        {
                            new ColumnSeries<double>
                            {
                                Values = fuelCostValues,
                                Name = "Carburant",
                                Fill = new SolidColorPaint(new SKColor(76, 175, 80)),
                                Stroke = new SolidColorPaint(new SKColor(56, 142, 60), 1),
                                YToolTipLabelFormatter = point => 
                                {
                                    var label = monthLabels.ElementAtOrDefault((int)point.Index) ?? "N/A";
                                    return $"{label}: {point.Coordinate.PrimaryValue:C0}";
                                }
                            },
                            new ColumnSeries<double>
                            {
                                Values = maintenanceCostValues,
                                Name = "Maintenance",
                                Fill = new SolidColorPaint(new SKColor(255, 152, 0)),
                                Stroke = new SolidColorPaint(new SKColor(245, 124, 0), 1),
                                YToolTipLabelFormatter = point => 
                                {
                                    var label = monthLabels.ElementAtOrDefault((int)point.Index) ?? "N/A";
                                    return $"{label}: {point.Coordinate.PrimaryValue:C0}";
                                }
                            }
                        };

                        // Calculer les totaux annuels
                        YearlyFuelTotal = MonthlyTrends.Sum(m => m.FuelCost);
                        YearlyMaintenanceTotal = MonthlyTrends.Sum(m => m.MaintenanceCost);
                        YearlyOperatingCost = YearlyFuelTotal + YearlyMaintenanceTotal;
                        MonthlyAverageCost = MonthlyTrends.Count > 0 ? YearlyOperatingCost / MonthlyTrends.Count : 0;
                    }
                    else
                    {
                        MonthlyTrendsSeries = new ISeries[]
                        {
                            new ColumnSeries<double> { Values = new double[] { }, Name = "Carburant" },
                            new ColumnSeries<double> { Values = new double[] { }, Name = "Maintenance" }
                        };
                        YearlyFuelTotal = 0;
                        YearlyMaintenanceTotal = 0;
                        YearlyOperatingCost = 0;
                        MonthlyAverageCost = 0;
                    }
                }
                catch
                {
                    TrendLabels = Array.Empty<string>();
                    ConsumptionSeries = Array.Empty<ISeries>();
                    CostSeries = Array.Empty<ISeries>();
                    MonthlyTrendsSeries = Array.Empty<ISeries>();
                    YearlyFuelTotal = 0;
                    YearlyMaintenanceTotal = 0;
                    YearlyOperatingCost = 0;
                    MonthlyAverageCost = 0;
                }

                // Charger les mouvements récents séparément
                var recentMovements = await _statisticsService.GetRecentMovementsAsync(10);
                RecentMovements = new ObservableCollection<RecentMovement>(recentMovements);

                // Calculer le nombre total de pleins (pour compatibilité avec l'ancienne interface)
                TotalFuelRecords = await GetTotalFuelRecordsAsync();

                // Mettre à jour l'horodatage
                LastUpdated = $"Dernière mise à jour: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Notifier les changements des propriétés calculées
                OnPropertyChanged(nameof(AlertCount));
                OnPropertyChanged(nameof(CriticalAlerts));
                OnPropertyChanged(nameof(HighPriorityAlerts));
                OnPropertyChanged(nameof(ActiveVehiclePercentage));
                OnPropertyChanged(nameof(MaintenanceVehiclePercentage));
                OnPropertyChanged(nameof(TotalMonthlyCost));
                OnPropertyChanged(nameof(FuelToMaintenanceRatio));

                System.Diagnostics.Debug.WriteLine($"Dashboard chargé: {TotalVehicles} véhicules, {ActiveVehicles} actifs, {AlertCount} alertes");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement du dashboard: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur lors du chargement des données du tableau de bord:\n\n{ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDataAsync(object? parameter)
        {
            await LoadDataAsync(parameter);
        }

        private void UpdateFleetStatistics(FleetStatistics fleetStats)
        {
            TotalVehicles = fleetStats.TotalVehicles;
            ActiveVehicles = fleetStats.ActiveVehicles;
            VehiclesInMaintenance = fleetStats.VehiclesInMaintenance;
            TotalFuelCost = fleetStats.TotalFuelCost;
            AverageFuelConsumption = fleetStats.AverageFleetConsumption;
            MonthlyFuelCost = fleetStats.MonthlyFuelCost;
            TotalMaintenanceCost = fleetStats.TotalMaintenanceCost;
            MonthlyMaintenanceCost = fleetStats.MonthlyMaintenanceCost;
        }

        private async Task<int> GetTotalFuelRecordsAsync()
        {
            try
            {
                var fuelRecords = await _fuelService.GetAllFuelRecordsAsync();
                return fuelRecords.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur calcul nombre de pleins: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Obtient la couleur d'une alerte selon sa priorité
        /// </summary>
        public string GetAlertColor(AlertPriority priority)
        {
            return priority switch
            {
                AlertPriority.Critical => "#F44336",
                AlertPriority.High => "#FF9800",
                AlertPriority.Medium => "#FFC107",
                AlertPriority.Low => "#4CAF50",
                _ => "#9E9E9E"
            };
        }

        /// <summary>
        /// Obtient l'icône d'une alerte selon son type
        /// </summary>
        public string GetAlertIcon(AlertType type)
        {
            return type switch
            {
                AlertType.MaintenanceDue => "🔧",
                AlertType.InspectionExpired => "📋",
                AlertType.InsuranceExpired => "🛡️",
                AlertType.HighConsumption => "⛽",
                AlertType.CostThreshold => "💰",
                AlertType.VehicleInactive => "⚠️",
                _ => "ℹ️"
            };
        }

        /// <summary>
        /// Formate un montant en euros
        /// </summary>
        public string FormatCurrency(decimal amount)
        {
            return amount.ToString("C2");
        }

        /// <summary>
        /// Formate une consommation en L/100km
        /// </summary>
        public string FormatConsumption(decimal consumption)
        {
            return $"{consumption:F1} L/100km";
        }

        #region Nouvelles commandes Dashboard

        private void ViewDetailedStatistics()
        {
            try
            {
                // Créer et ouvrir la fenêtre de statistiques détaillées
                var viewModel = new DetailedStatisticsViewModel(_statisticsService, _exportService);
                var window = new FleetManager.Views.DetailedStatisticsWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des statistiques détaillées:\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GenerateReportAsync(object? parameter)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers PDF (*.pdf)|*.pdf",
                    FileName = $"Dashboard_FleetManager_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var content = GenerateReportContent();
                    var (success, message) = _exportService.GeneratePdfReport(
                        "Rapport Tableau de Bord Fleet Manager",
                        content,
                        saveDialog.FileName);

                    if (success)
                    {
                        MessageBox.Show("Rapport PDF généré avec succès", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(message, "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportDataAsync(object? parameter)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers CSV (*.csv)|*.csv",
                    FileName = $"Dashboard_Export_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Exporter les statistiques simplifiées du top coûts (ou consommation)
                    var statsToExport = TopVehiclesByCost.Any() ? TopVehiclesByCost : TopVehiclesByConsumption;
                    // Utiliser directement la liste existante (les propriétés calculées sont en lecture seule)
                    var vehicleStatsList = statsToExport.ToList();

                    var exportResult = await _exportService.ExportStatisticsToCsvAsync(vehicleStatsList, saveDialog.FileName);
                    bool success = exportResult.Success;
                    string message = exportResult.Message;

                    if (success)
                    {
                        MessageBox.Show("Export CSV réussi", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(message, "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSettings()
        {
            try
            {
                // Créer et ouvrir la fenêtre de configuration
                var viewModel = new SettingsViewModel();
                var window = new FleetManager.Views.SettingsWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
                
                // Après fermeture des paramètres, recharger les données si nécessaire
                // LoadDataAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des paramètres:\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateReportContent()
        {
            var content = $"TABLEAU DE BORD FLEET MANAGER\n";
            content += $"Généré le: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";
            content += $"Véhicules: {TotalVehicles}\n";
            content += $"Actifs: {ActiveVehicles}\n";
            content += $"En maintenance: {VehiclesInMaintenance}\n";
            content += $"Coût carburant: {TotalFuelCost:C}\n";
            content += $"Maintenance: {TotalMaintenanceCost:C}\n";
            return content;
        }

        #endregion

        #endregion
    }
}
