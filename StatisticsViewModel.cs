using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using FleetManager.Models;
using FleetManager.Services;
using Microsoft.Win32;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.IO;

namespace FleetManager.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly VehicleService _vehicleService;
        private readonly FuelService _fuelService;
        private readonly StatisticsService _statisticsService;
        private readonly ExportService _exportService;
        private readonly IEmailService _emailService;

        // Collections principales
        private ObservableCollection<Vehicle> _vehicles = new();
        private ObservableCollection<VehicleStatistics> _vehicleStatistics = new();
        private ObservableCollection<MonthlyStatistics> _monthlyStatistics = new();
        private ObservableCollection<VehicleTypeStatistics> _typeStatistics = new();
        private ObservableCollection<FuelTypeStatistics> _fuelStatistics = new();

        // Sélections et filtres
        private Vehicle? _selectedVehicle;
        private VehicleStatistics? _selectedVehicleStats;
        private DateTime _startDate = DateTime.Now.AddYears(-1);
        private DateTime _endDate = DateTime.Now;
        private string _selectedPeriod = "Année";
        private string? _selectedVehicleType;
        private string? _selectedFuelType;

        // Statistiques globales
        private decimal _totalFuelCost;
        private decimal _totalMaintenanceCost;
        private decimal _averageConsumption;
        private decimal _totalMileage;
        private int _totalRefuels;
        private int _totalMaintenances;

        // Comparaisons et analyses
        private ObservableCollection<VehicleStatistics> _topPerformers = new();
        private ObservableCollection<VehicleStatistics> _bottomPerformers = new();
        private ObservableCollection<PerformanceComparison> _performanceComparisons = new();
        private ObservableCollection<PredictionData> _predictions = new();

        // Graphiques et tendances
        private ObservableCollection<TimeSeriesData> _consumptionTrend = new();
        private ObservableCollection<TimeSeriesData> _costTrend = new();
        private ObservableCollection<TimeSeriesData> _mileageTrend = new();

        // LiveCharts series & labels (initialiser avec série vide pour éviter NullReference XAML)
        private IEnumerable<ISeries> _monthlyCostSeries = new List<ISeries>
        {
            new ColumnSeries<double> { Values = new double[] { }, Name = "Carburant" },
            new ColumnSeries<double> { Values = new double[] { }, Name = "Maintenance" }
        };
        private IEnumerable<ISeries> _monthlyConsumptionSeries = new List<ISeries>
        {
            new ColumnSeries<double> { Values = new double[] { }, Name = "Consommation" }
        };
        private string[] _monthlyLabels = Array.Empty<string>();

        // État et contrôles
        private bool _isLoading;
        private string _loadingMessage = string.Empty;
        private bool _showAdvancedFilters;
        private string _searchText = string.Empty;

        public StatisticsViewModel(
            VehicleService vehicleService,
            FuelService fuelService,
            StatisticsService statisticsService,
            ExportService exportService,
            IEmailService emailService)
        {
            _vehicleService = vehicleService;
            _fuelService = fuelService;
            _statisticsService = statisticsService;
            _exportService = exportService;
            _emailService = emailService;

            InitializeCommands();
            InitializePeriods();

            // Charger les données au démarrage
            _ = LoadDataAsync();
        }

        #region Propriétés - Collections

        public ObservableCollection<Vehicle> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public ObservableCollection<VehicleStatistics> VehicleStatistics
        {
            get => _vehicleStatistics;
            set => SetProperty(ref _vehicleStatistics, value);
        }

        public ObservableCollection<MonthlyStatistics> MonthlyStatistics
        {
            get => _monthlyStatistics;
            set => SetProperty(ref _monthlyStatistics, value);
        }

        public ObservableCollection<VehicleTypeStatistics> TypeStatistics
        {
            get => _typeStatistics;
            set => SetProperty(ref _typeStatistics, value);
        }

        public ObservableCollection<FuelTypeStatistics> FuelStatistics
        {
            get => _fuelStatistics;
            set => SetProperty(ref _fuelStatistics, value);
        }

        public ObservableCollection<VehicleStatistics> TopPerformers
        {
            get => _topPerformers;
            set => SetProperty(ref _topPerformers, value);
        }

        public ObservableCollection<VehicleStatistics> BottomPerformers
        {
            get => _bottomPerformers;
            set => SetProperty(ref _bottomPerformers, value);
        }

        public ObservableCollection<PerformanceComparison> PerformanceComparisons
        {
            get => _performanceComparisons;
            set => SetProperty(ref _performanceComparisons, value);
        }

        public ObservableCollection<PredictionData> Predictions
        {
            get => _predictions;
            set => SetProperty(ref _predictions, value);
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

        public ObservableCollection<TimeSeriesData> MileageTrend
        {
            get => _mileageTrend;
            set => SetProperty(ref _mileageTrend, value);
        }

        // Properties exposées aux vues pour LiveCharts
        public IEnumerable<ISeries> MonthlyCostSeries
        {
            get => _monthlyCostSeries;
            set => SetProperty(ref _monthlyCostSeries, value);
        }

        public IEnumerable<ISeries> MonthlyConsumptionSeries
        {
            get => _monthlyConsumptionSeries;
            set => SetProperty(ref _monthlyConsumptionSeries, value);
        }

        public string[] MonthlyLabels
        {
            get => _monthlyLabels;
            set => SetProperty(ref _monthlyLabels, value);
        }

        #endregion

        #region Propriétés - Sélections et filtres

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (SetProperty(ref _selectedVehicle, value))
                {
                    _ = LoadVehicleStatisticsAsync();
                }
            }
        }

        public VehicleStatistics? SelectedVehicleStats
        {
            get => _selectedVehicleStats;
            set => SetProperty(ref _selectedVehicleStats, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    _ = RefreshStatisticsAsync();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    _ = RefreshStatisticsAsync();
                }
            }
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    UpdateDateRange();
                    _ = RefreshStatisticsAsync();
                }
            }
        }

        public string? SelectedVehicleType
        {
            get => _selectedVehicleType;
            set
            {
                if (SetProperty(ref _selectedVehicleType, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string? SelectedFuelType
        {
            get => _selectedFuelType;
            set
            {
                if (SetProperty(ref _selectedFuelType, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        #endregion

        #region Propriétés - Statistiques globales

        public decimal TotalFuelCost
        {
            get => _totalFuelCost;
            set => SetProperty(ref _totalFuelCost, value);
        }

        public decimal TotalMaintenanceCost
        {
            get => _totalMaintenanceCost;
            set => SetProperty(ref _totalMaintenanceCost, value);
        }

        public decimal AverageConsumption
        {
            get => _averageConsumption;
            set => SetProperty(ref _averageConsumption, value);
        }

        public decimal TotalMileage
        {
            get => _totalMileage;
            set => SetProperty(ref _totalMileage, value);
        }

        public int TotalRefuels
        {
            get => _totalRefuels;
            set => SetProperty(ref _totalRefuels, value);
        }

        public int TotalMaintenances
        {
            get => _totalMaintenances;
            set => SetProperty(ref _totalMaintenances, value);
        }

        // Propriétés calculées
        public decimal TotalCost => TotalFuelCost + TotalMaintenanceCost;
        public decimal CostPerKilometer => TotalMileage > 0 ? TotalCost / TotalMileage : 0;
        public decimal FuelToMaintenanceRatio => TotalMaintenanceCost > 0 ? TotalFuelCost / TotalMaintenanceCost : 0;
        public decimal AverageCostPerRefuel => TotalRefuels > 0 ? TotalFuelCost / TotalRefuels : 0;
        public decimal AverageCostPerMaintenance => TotalMaintenances > 0 ? TotalMaintenanceCost / TotalMaintenances : 0;

        #endregion

        #region Propriétés - État et contrôles

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        public bool ShowAdvancedFilters
        {
            get => _showAdvancedFilters;
            set => SetProperty(ref _showAdvancedFilters, value);
        }

        public ObservableCollection<string> AvailablePeriods { get; private set; } = new();
        public ObservableCollection<VehicleType> AvailableVehicleTypes { get; private set; } = new();
        public ObservableCollection<FuelType> AvailableFuelTypes { get; private set; } = new();

        #endregion

        #region Commandes

        public ICommand LoadDataCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand GenerateReportCommand { get; private set; }
        public ICommand ExportToCsvCommand { get; private set; }
        public ICommand ExportToExcelCommand { get; private set; }
        public ICommand ToggleAdvancedFiltersCommand { get; private set; }
        public ICommand ResetFiltersCommand { get; private set; }
        public ICommand CompareVehiclesCommand { get; private set; }
        public ICommand ShowVehicleDetailCommand { get; private set; }
        
        // Nouvelles commandes pour les 5 boutons manquants
        public ICommand ShowAdvancedChartsCommand { get; private set; }
        public ICommand ComparePeriodCommand { get; private set; }
        public ICommand SendReportCommand { get; private set; }
        public ICommand SetTargetsCommand { get; private set; }
        public ICommand AnalysisSettingsCommand { get; private set; }

        private void InitializeCommands()
        {
            LoadDataCommand = new AsyncRelayCommand(_ => LoadDataAsync());
            RefreshCommand = new AsyncRelayCommand(_ => RefreshStatisticsAsync());
            GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
            ExportToCsvCommand = new AsyncRelayCommand(ExportToCsvAsync);
            ExportToExcelCommand = new AsyncRelayCommand(ExportToExcelAsync);
            ToggleAdvancedFiltersCommand = new RelayCommand(_ => ToggleAdvancedFilters(null));
            ResetFiltersCommand = new AsyncRelayCommand(ResetFiltersAsync);
            CompareVehiclesCommand = new AsyncRelayCommand(CompareVehiclesAsync);
            ShowVehicleDetailCommand = new RelayCommand<VehicleStatistics>(ShowVehicleDetail);
            
            // Initialiser les 5 nouvelles commandes
            ShowAdvancedChartsCommand = new RelayCommand(_ => ShowAdvancedCharts());
            ComparePeriodCommand = new RelayCommand(_ => ComparePeriod());
            SendReportCommand = new AsyncRelayCommand(SendReportAsync);
            SetTargetsCommand = new RelayCommand(_ => SetTargets());
            AnalysisSettingsCommand = new RelayCommand(_ => OpenAnalysisSettings());
        }

        #endregion

        #region Méthodes de chargement des données

        private async Task LoadDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                LoadingMessage = "Chargement des données...";

                // Charger les véhicules
                LoadingMessage = "Chargement des véhicules...";
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                Vehicles = new ObservableCollection<Vehicle>(vehicles);

                // Charger les statistiques globales
                LoadingMessage = "Calcul des statistiques...";
                await LoadGlobalStatisticsAsync();

                // Charger les statistiques par véhicule
                LoadingMessage = "Analyse des véhicules...";
                await LoadVehicleStatisticsListAsync();

                // Charger les tendances mensuelles
                LoadingMessage = "Calcul des tendances...";
                var monthlyStats = await _statisticsService.GetMonthlyTrendsAsync(12);
                MonthlyStatistics = new ObservableCollection<MonthlyStatistics>(monthlyStats);
                
                // Préparer les séries et labels pour les graphiques LiveCharts
                try
                {
                    MonthlyLabels = MonthlyStatistics.Select(m => m.MonthName).ToArray();

                    var fuelValues = MonthlyStatistics.Select(m => (double)m.FuelCost).ToArray();
                    var maintenanceValues = MonthlyStatistics.Select(m => (double)m.MaintenanceCost).ToArray();

                    MonthlyCostSeries = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Values = fuelValues,
                            Name = "Carburant",
                            Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                            Stroke = new SolidColorPaint(SKColors.SeaGreen, 1),
                            YToolTipLabelFormatter = point => $"{MonthlyLabels.ElementAtOrDefault(point.Index) ?? "N/A"}: {point.Coordinate.PrimaryValue:C0}"
                        },
                        new ColumnSeries<double>
                        {
                            Values = maintenanceValues,
                            Name = "Maintenance",
                            Fill = new SolidColorPaint(SKColors.SteelBlue),
                            Stroke = new SolidColorPaint(SKColors.MidnightBlue, 1),
                            YToolTipLabelFormatter = point => $"{MonthlyLabels.ElementAtOrDefault(point.Index) ?? "N/A"}: {point.Coordinate.PrimaryValue:C0}"
                        }
                    };

                    var consumptionValues = MonthlyStatistics.Select(m => (double)m.AverageConsumption).ToArray();
                    MonthlyConsumptionSeries = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Values = consumptionValues,
                            Name = "Consommation",
                            Fill = new SolidColorPaint(SKColors.Goldenrod),
                            Stroke = new SolidColorPaint(SKColors.DarkGoldenrod, 1),
                            YToolTipLabelFormatter = point => $"{MonthlyLabels.ElementAtOrDefault(point.Index) ?? "N/A"}: {point.Coordinate.PrimaryValue:F2} L/100km"
                        }
                    };
                }
                catch
                {
                    // Si quelque chose échoue ici, on laisse les charts vides mais l'application continue
                    MonthlyLabels = Array.Empty<string>();
                    MonthlyCostSeries = Array.Empty<ISeries>();
                    MonthlyConsumptionSeries = Array.Empty<ISeries>();
                }

                // Charger les statistiques par type
                LoadingMessage = "Analyse par catégorie...";
                var typeStats = await _statisticsService.GetVehicleTypeStatisticsAsync();
                TypeStatistics = new ObservableCollection<VehicleTypeStatistics>(typeStats);

                var fuelStats = await _statisticsService.GetFuelTypeStatisticsAsync();
                FuelStatistics = new ObservableCollection<FuelTypeStatistics>(fuelStats);

                // Charger les analyses de performance
                LoadingMessage = "Analyse de performance...";
                await LoadPerformanceAnalysisAsync();

                // Charger les tendances pour les graphiques
                LoadingMessage = "Génération des graphiques...";
                await LoadTrendsAsync();

                // Charger les prédictions
                LoadingMessage = "Calcul des prédictions...";
                var predictions = await _statisticsService.GetPredictionsAsync();
                Predictions = new ObservableCollection<PredictionData>(predictions);

                // Notifier les propriétés calculées
                NotifyCalculatedProperties();

                LoadingMessage = "Données chargées avec succès";
            }
            catch (Exception ex)
            {
                LoadingMessage = $"Erreur: {ex.Message}";
                MessageBox.Show($"Erreur lors du chargement des statistiques:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = string.Empty;
            }
        }

        private async Task RefreshStatisticsAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadVehicleStatisticsAsync()
        {
            if (SelectedVehicle == null)
            {
                SelectedVehicleStats = null;
                return;
            }

            try
            {
                SelectedVehicleStats = await _statisticsService.GetVehicleStatisticsAsync(SelectedVehicle.VehicleId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des statistiques du véhicule:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadGlobalStatisticsAsync()
        {
            var fleetStats = await _statisticsService.GetFleetStatisticsAsync();

            TotalFuelCost = fleetStats.TotalFuelCost;
            TotalMaintenanceCost = fleetStats.TotalMaintenanceCost;
            AverageConsumption = fleetStats.AverageFleetConsumption;
            TotalMileage = fleetStats.TotalMileage;

            // Compter les pleins et maintenances
            var fuelRecords = await _fuelService.GetAllFuelRecordsAsync();
            TotalRefuels = fuelRecords.Count;

            // Note: Vous devrez ajouter une méthode GetAllMaintenanceRecordsAsync dans un service approprié
            // ou l'implémenter dans StatisticsService
            // TotalMaintenances = maintenanceRecords.Count;
        }

        private async Task LoadVehicleStatisticsListAsync()
        {
            var vehicleStatsList = new List<VehicleStatistics>();

            foreach (var vehicle in Vehicles)
            {
                try
                {
                    var stats = await _statisticsService.GetVehicleStatisticsAsync(vehicle.VehicleId);
                    vehicleStatsList.Add(stats);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur stats véhicule {vehicle.VehicleId}: {ex.Message}");
                }
            }

            VehicleStatistics = new ObservableCollection<VehicleStatistics>(vehicleStatsList);
        }

        private async Task LoadPerformanceAnalysisAsync()
        {
            var topByConsumption = await _statisticsService.GetTopVehiclesByConsumptionAsync(5);
            TopPerformers = new ObservableCollection<VehicleStatistics>(
                topByConsumption.OrderBy(v => v.AverageConsumption).Take(3));

            BottomPerformers = new ObservableCollection<VehicleStatistics>(
                topByConsumption.OrderByDescending(v => v.AverageConsumption).Take(3));

            // Générer les comparaisons de performance
            var comparisons = new List<PerformanceComparison>();
            var fleetAvgConsumption = VehicleStatistics.Where(v => v.AverageConsumption > 0)
                .DefaultIfEmpty()
                .Average(v => v?.AverageConsumption ?? 0);

            var fleetAvgCost = VehicleStatistics.DefaultIfEmpty()
                .Average(v => v?.TotalCost ?? 0);

            foreach (var vehicle in VehicleStatistics.Take(10))
            {
                var comparison = new PerformanceComparison
                {
                    VehicleRegistration = vehicle.RegistrationNumber,
                    ConsumptionVsFleet = fleetAvgConsumption > 0 ?
                        ((vehicle.AverageConsumption - fleetAvgConsumption) / fleetAvgConsumption) * 100 : 0,
                    CostVsFleet = fleetAvgCost > 0 ?
                        ((vehicle.TotalCost - fleetAvgCost) / fleetAvgCost) * 100 : 0,
                };

                // Calculer la note de performance
                var efficiencyScore = comparison.ConsumptionVsFleet <= -20 ? 5 :
                                    comparison.ConsumptionVsFleet <= -10 ? 4 :
                                    comparison.ConsumptionVsFleet <= 0 ? 3 :
                                    comparison.ConsumptionVsFleet <= 10 ? 2 : 1;

                var costScore = comparison.CostVsFleet <= -20 ? 5 :
                              comparison.CostVsFleet <= -10 ? 4 :
                              comparison.CostVsFleet <= 0 ? 3 :
                              comparison.CostVsFleet <= 10 ? 2 : 1;

                comparison.EfficiencyRating = (efficiencyScore + costScore) / 2.0m;
                comparison.PerformanceGrade = comparison.EfficiencyRating >= 4.5m ? "A" :
                                            comparison.EfficiencyRating >= 3.5m ? "B" :
                                            comparison.EfficiencyRating >= 2.5m ? "C" :
                                            comparison.EfficiencyRating >= 1.5m ? "D" : "E";

                // Générer des recommandations
                if (comparison.ConsumptionVsFleet > 15)
                    comparison.Recommendations.Add("Vérifier l'état du moteur et des filtres");
                if (comparison.CostVsFleet > 20)
                    comparison.Recommendations.Add("Analyser les coûts de maintenance");
                if (comparison.EfficiencyRating < 2.5m)
                    comparison.Recommendations.Add("Programmer une révision complète");

                comparisons.Add(comparison);
            }

            PerformanceComparisons = new ObservableCollection<PerformanceComparison>(comparisons);
        }

        private async Task LoadTrendsAsync()
        {
            var consumptionData = await _statisticsService.GetConsumptionTrendAsync(90);
            ConsumptionTrend = new ObservableCollection<TimeSeriesData>(consumptionData);

            var costData = await _statisticsService.GetCostTrendAsync(90);
            CostTrend = new ObservableCollection<TimeSeriesData>(costData);

            // Générer une tendance de kilométrage basée sur les données disponibles
            var mileageData = VehicleStatistics.Where(v => v.CurrentMileage > 0)
                .OrderBy(v => v.VehicleId)
                .Select((v, index) => new TimeSeriesData
                {
                    Date = DateTime.Now.AddDays(-index),
                    Value = v.CurrentMileage,
                    Label = v.RegistrationNumber,
                    Category = "Kilométrage"
                }).ToList();

            MileageTrend = new ObservableCollection<TimeSeriesData>(mileageData);
        }

        #endregion

        #region Méthodes de filtrage

        private async Task ApplyFiltersAsync()
        {
            if (IsLoading) return;

            try
            {
                var filteredVehicles = Vehicles.AsEnumerable();

                // Filtre par type de véhicule
                if (SelectedVehicleType != null)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.VehicleType == SelectedVehicleType);
                }

                // Filtre par type de carburant
                if (SelectedFuelType != null)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.FuelType == SelectedFuelType);
                }

                // Filtre par recherche textuelle
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    filteredVehicles = filteredVehicles.Where(v =>
                        v.RegistrationNumber.ToLower().Contains(searchLower) ||
                        v.Brand.ToLower().Contains(searchLower) ||
                        v.Model.ToLower().Contains(searchLower));
                }

                // Mettre à jour la collection filtrée
                var filteredList = filteredVehicles.ToList();

                // Recalculer les statistiques pour les véhicules filtrés
                var filteredStats = VehicleStatistics
                    .Where(vs => filteredList.Any(v => v.VehicleId == vs.VehicleId))
                    .ToList();

                VehicleStatistics = new ObservableCollection<VehicleStatistics>(filteredStats);

                // Mettre à jour les statistiques globales
                UpdateGlobalStatisticsFromFiltered(filteredStats);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'application des filtres:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateGlobalStatisticsFromFiltered(List<VehicleStatistics> filteredStats)
        {
            TotalFuelCost = filteredStats.Sum(s => s.TotalFuelCost);
            TotalMaintenanceCost = filteredStats.Sum(s => s.TotalMaintenanceCost);
            AverageConsumption = filteredStats.Where(s => s.AverageConsumption > 0)
                .DefaultIfEmpty()
                .Average(s => s?.AverageConsumption ?? 0);
            TotalMileage = filteredStats.Sum(s => s.CurrentMileage);
            TotalRefuels = filteredStats.Sum(s => s.TotalRefuels);
            TotalMaintenances = filteredStats.Sum(s => s.TotalMaintenances);

            NotifyCalculatedProperties();
        }

        #endregion

        #region Méthodes des commandes

        private async Task GenerateReportAsync(object? parameter)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichiers PDF (*.pdf)|*.pdf",
                    FileName = $"Statistiques_FleetManager_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var content = GenerateReportContent();
                    var (success, message) = _exportService.GeneratePdfReport(
                        "Rapport Statistiques Fleet Manager",
                        content,
                        saveDialog.FileName);

                    if (success)
                    {
                        MessageBox.Show("Rapport généré avec succès!", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la génération du rapport:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToCsvAsync(object? parameter)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichiers CSV (*.csv)|*.csv",
                    FileName = $"Statistiques_Vehicules_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var (success, message) = await _exportService.ExportStatisticsToCsvAsync(
                        VehicleStatistics.ToList(), saveDialog.FileName);

                    if (success)
                    {
                        MessageBox.Show("Export réussi!", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export CSV:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToExcelAsync(object? parameter)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichiers CSV (*.csv)|*.csv",
                    FileName = $"Statistiques_FleetManager_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Export CSV comme alternative à Excel
                    var (success, message) = await _exportService.ExportStatisticsToCsvAsync(
                        VehicleStatistics.ToList(), saveDialog.FileName);

                    if (success)
                    {
                        MessageBox.Show("Export réussi!\n\nLe fichier CSV peut être ouvert dans Excel.", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleAdvancedFilters(object? parameter)
        {
            ShowAdvancedFilters = !ShowAdvancedFilters;
        }

        private async Task ResetFiltersAsync(object? parameter)
        {
            SelectedVehicleType = null;
            SelectedFuelType = null;
            SearchText = string.Empty;
            StartDate = DateTime.Now.AddYears(-1);
            EndDate = DateTime.Now;
            SelectedPeriod = "Année";

            await LoadDataAsync();
        }

        private async Task CompareVehiclesAsync(object? parameter)
        {
            try
            {
                var window = new Views.CompareVehiclesWindow(_vehicleService, _statisticsService, _exportService);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la comparaison:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            await Task.CompletedTask;
        }

        private void ShowVehicleDetail(VehicleStatistics? stats)
        {
            if (stats != null)
            {
                var vehicle = Vehicles.FirstOrDefault(v => v.VehicleId == stats.VehicleId);
                if (vehicle != null)
                {
                    SelectedVehicle = vehicle;
                }
            }
        }

        #endregion

        #region Méthodes utilitaires

        private void InitializePeriods()
        {
            AvailablePeriods.Clear();
            AvailablePeriods.Add("Semaine");
            AvailablePeriods.Add("Mois");
            AvailablePeriods.Add("Trimestre");
            AvailablePeriods.Add("Année");
            AvailablePeriods.Add("Personnalisé");

            // Initialiser les types disponibles
            foreach (VehicleType type in Enum.GetValues<VehicleType>())
            {
                AvailableVehicleTypes.Add(type);
            }

            foreach (FuelType type in Enum.GetValues<FuelType>())
            {
                AvailableFuelTypes.Add(type);
            }
        }

        private void UpdateDateRange()
        {
            var now = DateTime.Now;
            switch (SelectedPeriod)
            {
                case "Semaine":
                    StartDate = now.AddDays(-7);
                    EndDate = now;
                    break;
                case "Mois":
                    StartDate = now.AddMonths(-1);
                    EndDate = now;
                    break;
                case "Trimestre":
                    StartDate = now.AddMonths(-3);
                    EndDate = now;
                    break;
                case "Année":
                    StartDate = now.AddYears(-1);
                    EndDate = now;
                    break;
                    // "Personnalisé" ne change pas les dates
            }
        }

        private string GenerateReportContent()
        {
            var content = $"RAPPORT STATISTIQUES FLEET MANAGER\n";
            content += $"Généré le: {DateTime.Now:dd/MM/yyyy HH:mm}\n";
            content += $"Période: du {StartDate:dd/MM/yyyy} au {EndDate:dd/MM/yyyy}\n\n";

            content += "=== STATISTIQUES GLOBALES ===\n";
            content += $"Nombre de véhicules: {Vehicles.Count}\n";
            content += $"Coût total carburant: {TotalFuelCost:C}\n";
            content += $"Coût total maintenance: {TotalMaintenanceCost:C}\n";
            content += $"Coût total: {TotalCost:C}\n";
            content += $"Consommation moyenne: {AverageConsumption:F2} L/100km\n";
            content += $"Kilométrage total: {TotalMileage:N0} km\n\n";

            content += "=== TOP 5 VÉHICULES PAR CONSOMMATION ===\n";
            foreach (var vehicle in TopPerformers.Take(5))
            {
                content += $"{vehicle.RegistrationNumber}: {vehicle.AverageConsumption:F2} L/100km\n";
            }

            content += "\n=== DÉTAIL PAR VÉHICULE ===\n";
            foreach (var vehicle in VehicleStatistics.Take(10))
            {
                content += $"\n{vehicle.VehicleName} ({vehicle.RegistrationNumber}):\n";
                content += $"  - Consommation: {vehicle.AverageConsumption:F2} L/100km\n";
                content += $"  - Coût carburant: {vehicle.TotalFuelCost:C}\n";
                content += $"  - Coût maintenance: {vehicle.TotalMaintenanceCost:C}\n";
                content += $"  - Kilométrage: {vehicle.CurrentMileage:N0} km\n";
            }

            return content;
        }

        private void NotifyCalculatedProperties()
        {
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(CostPerKilometer));
            OnPropertyChanged(nameof(FuelToMaintenanceRatio));
            OnPropertyChanged(nameof(AverageCostPerRefuel));
            OnPropertyChanged(nameof(AverageCostPerMaintenance));
        }

        #region Nouvelles méthodes pour les 5 commandes

        private void ShowAdvancedCharts()
        {
            try
            {
                var window = new Views.AdvancedChartsWindow();
                window.DataContext = new AdvancedChartsViewModel(_statisticsService, _vehicleService);
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des graphiques avancés:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComparePeriod()
        {
            try
            {
                var window = new Views.PeriodComparisonWindow(
                    new PeriodComparisonViewModel(_statisticsService));
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la comparaison de périodes:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SendReportAsync(object? parameter)
        {
            try
            {
                var window = new Views.SendReportWindow();
                window.DataContext = new SendReportViewModel(_exportService, _statisticsService);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            await Task.CompletedTask;
        }

        private void SetTargets()
        {
            try
            {
                var window = new Views.ObjectivesWindow();
                window.DataContext = new ObjectivesViewModel(_statisticsService);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des objectifs:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAnalysisSettings()
        {
            try
            {
                var window = new Views.AnalysisSettingsWindow();
                window.DataContext = new AnalysisSettingsViewModel();
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des paramètres d'analyse:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion
    }
}
