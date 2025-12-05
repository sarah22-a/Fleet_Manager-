using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Models;
using FleetManager.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Microsoft.Win32;
using System.Windows;

namespace FleetManager.ViewModels
{
    public class DetailedStatisticsViewModel : BaseViewModel
    {
        private readonly StatisticsService _statisticsService;
        private readonly ExportService _exportService;

        // Propriétés de données
        private int _activeVehiclesCount;
        private decimal _activeVehiclesPercentage;
        private decimal _totalLitersConsumed;
        private decimal _averageConsumption;
        private decimal _totalOperatingCost;
        private decimal _costPerKm;
        private decimal _totalKilometers;
        private decimal _averageKmPerVehicle;
        private string _periodDescription = string.Empty;
        private string _lastUpdated = string.Empty;
        private bool _isLoading;
        private string _searchText = string.Empty;

        // Collections
        private ObservableCollection<VehicleStatistics> _allVehicleStatistics = new();
        private ObservableCollection<VehicleStatistics> _filteredVehicleStatistics = new();
        private ObservableCollection<VehicleStatistics> _topEfficientVehicles = new();
        private ObservableCollection<VehicleStatistics> _topCostlyVehicles = new();
        private ObservableCollection<MonthlyStatistics> _monthlyStatistics = new();

        // Séries pour les graphiques
        private IEnumerable<ISeries> _costDistributionSeries = Array.Empty<ISeries>();
        private IEnumerable<ISeries> _vehicleTypeSeries = Array.Empty<ISeries>();
        private IEnumerable<ISeries> _monthlyTrendsSeries = Array.Empty<ISeries>();

        public DetailedStatisticsViewModel(StatisticsService statisticsService, ExportService exportService)
        {
            _statisticsService = statisticsService;
            _exportService = exportService;

            // Commandes
            RefreshCommand = new AsyncRelayCommand(_ => LoadDataAsync());
            ExportCommand = new AsyncRelayCommand(_ => ExportAllDataAsync());
            ExportExcelCommand = new AsyncRelayCommand(_ => ExportToExcelAsync());
            ExportPdfCommand = new AsyncRelayCommand(_ => ExportToPdfAsync());
            CloseCommand = new RelayCommand(_ => CloseWindow());

            // Charger les données
            _ = LoadDataAsync();
        }

        #region Propriétés

        public int ActiveVehiclesCount
        {
            get => _activeVehiclesCount;
            set => SetProperty(ref _activeVehiclesCount, value);
        }

        public decimal ActiveVehiclesPercentage
        {
            get => _activeVehiclesPercentage;
            set => SetProperty(ref _activeVehiclesPercentage, value);
        }

        public decimal TotalLitersConsumed
        {
            get => _totalLitersConsumed;
            set => SetProperty(ref _totalLitersConsumed, value);
        }

        public decimal AverageConsumption
        {
            get => _averageConsumption;
            set => SetProperty(ref _averageConsumption, value);
        }

        public decimal TotalOperatingCost
        {
            get => _totalOperatingCost;
            set => SetProperty(ref _totalOperatingCost, value);
        }

        public decimal CostPerKm
        {
            get => _costPerKm;
            set => SetProperty(ref _costPerKm, value);
        }

        public decimal TotalKilometers
        {
            get => _totalKilometers;
            set => SetProperty(ref _totalKilometers, value);
        }

        public decimal AverageKmPerVehicle
        {
            get => _averageKmPerVehicle;
            set => SetProperty(ref _averageKmPerVehicle, value);
        }

        public string PeriodDescription
        {
            get => _periodDescription;
            set => SetProperty(ref _periodDescription, value);
        }

        public string LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterVehicles();
                }
            }
        }

        public ObservableCollection<VehicleStatistics> AllVehicleStatistics
        {
            get => _allVehicleStatistics;
            set => SetProperty(ref _allVehicleStatistics, value);
        }

        public ObservableCollection<VehicleStatistics> FilteredVehicleStatistics
        {
            get => _filteredVehicleStatistics;
            set => SetProperty(ref _filteredVehicleStatistics, value);
        }

        public ObservableCollection<VehicleStatistics> TopEfficientVehicles
        {
            get => _topEfficientVehicles;
            set => SetProperty(ref _topEfficientVehicles, value);
        }

        public ObservableCollection<VehicleStatistics> TopCostlyVehicles
        {
            get => _topCostlyVehicles;
            set => SetProperty(ref _topCostlyVehicles, value);
        }

        public IEnumerable<ISeries> CostDistributionSeries
        {
            get => _costDistributionSeries;
            set => SetProperty(ref _costDistributionSeries, value);
        }

        public IEnumerable<ISeries> VehicleTypeSeries
        {
            get => _vehicleTypeSeries;
            set => SetProperty(ref _vehicleTypeSeries, value);
        }

        public IEnumerable<ISeries> MonthlyTrendsSeries
        {
            get => _monthlyTrendsSeries;
            set => SetProperty(ref _monthlyTrendsSeries, value);
        }

        public string TotalVehiclesDisplay => $"Total: {FilteredVehicleStatistics.Count} véhicule(s) affiché(s)";

        #endregion

        #region Commandes

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand CloseCommand { get; }

        #endregion

        #region Méthodes

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Obtenir les données du dashboard
                var dashboardData = await _statisticsService.GetDashboardDataAsync();

                // Statistiques de base
                ActiveVehiclesCount = dashboardData.FleetStats.ActiveVehicles;
                var totalVehicles = dashboardData.FleetStats.TotalVehicles;
                ActiveVehiclesPercentage = totalVehicles > 0 ? (decimal)ActiveVehiclesCount / totalVehicles * 100 : 0;

                TotalOperatingCost = dashboardData.FleetStats.TotalFuelCost + dashboardData.FleetStats.TotalMaintenanceCost;
                AverageConsumption = dashboardData.FleetStats.AverageFleetConsumption;
                TotalKilometers = dashboardData.FleetStats.TotalMileage;
                
                // Charger les statistiques détaillées par véhicule
                var vehicleStats = await _statisticsService.GetAllVehicleStatisticsAsync();
                AllVehicleStatistics = new ObservableCollection<VehicleStatistics>(vehicleStats);
                
                TotalLitersConsumed = vehicleStats.Sum(v => v.TotalLiters);
                AverageKmPerVehicle = vehicleStats.Count > 0 ? vehicleStats.Average(v => v.CurrentMileage) : 0;
                CostPerKm = TotalKilometers > 0 ? TotalOperatingCost / TotalKilometers : 0;

                // Top véhicules
                var rankedByEfficiency = vehicleStats.OrderBy(v => v.AverageConsumption).Take(5).ToList();
                for (int i = 0; i < rankedByEfficiency.Count; i++)
                {
                    rankedByEfficiency[i].Rank = i + 1;
                }
                TopEfficientVehicles = new ObservableCollection<VehicleStatistics>(rankedByEfficiency);

                var rankedByCost = vehicleStats.OrderByDescending(v => v.TotalCost).Take(5).ToList();
                for (int i = 0; i < rankedByCost.Count; i++)
                {
                    rankedByCost[i].Rank = i + 1;
                }
                TopCostlyVehicles = new ObservableCollection<VehicleStatistics>(rankedByCost);

                // Statistiques mensuelles
                _monthlyStatistics = new ObservableCollection<MonthlyStatistics>(dashboardData.MonthlyTrends);

                // Générer les graphiques
                GenerateCharts(dashboardData, vehicleStats);

                FilterVehicles();

                PeriodDescription = $"Période: 12 derniers mois (depuis {DateTime.Now.AddMonths(-12):MMMM yyyy})";
                LastUpdated = $"Mis à jour le {DateTime.Now:dd/MM/yyyy à HH:mm}";

                OnPropertyChanged(nameof(TotalVehiclesDisplay));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des statistiques:\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateCharts(DashboardData dashboardData, List<VehicleStatistics> vehicleStats)
        {
            // Répartition des coûts
            var fuelCost = dashboardData.FleetStats.TotalFuelCost;
            var maintenanceCost = dashboardData.FleetStats.TotalMaintenanceCost;

            CostDistributionSeries = new ISeries[]
            {
                new PieSeries<decimal>
                {
                    Values = new[] { fuelCost },
                    Name = "Carburant",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:C0}",
                    DataLabelsSize = 12
                },
                new PieSeries<decimal>
                {
                    Values = new[] { maintenanceCost },
                    Name = "Maintenance",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:C0}",
                    DataLabelsSize = 12
                }
            };

            // Répartition par type de véhicule
            var typeGroups = vehicleStats.GroupBy(v => v.VehicleType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            VehicleTypeSeries = typeGroups.Select(g => new PieSeries<int>
            {
                Values = new[] { g.Count },
                Name = g.Type,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}",
                DataLabelsSize = 14
            }).ToArray();

            // Tendances mensuelles
            if (_monthlyStatistics.Any())
            {
                MonthlyTrendsSeries = new ISeries[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = _monthlyStatistics.Select(m => m.FuelCost).ToArray(),
                        Name = "Carburant",
                        Fill = new SolidColorPaint(new SKColor(76, 175, 80)),
                        Stroke = new SolidColorPaint(new SKColor(56, 142, 60), 1)
                    },
                    new ColumnSeries<decimal>
                    {
                        Values = _monthlyStatistics.Select(m => m.MaintenanceCost).ToArray(),
                        Name = "Maintenance",
                        Fill = new SolidColorPaint(new SKColor(255, 152, 0)),
                        Stroke = new SolidColorPaint(new SKColor(245, 124, 0), 1)
                    }
                };
            }
        }

        private void FilterVehicles()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredVehicleStatistics = new ObservableCollection<VehicleStatistics>(AllVehicleStatistics);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = AllVehicleStatistics.Where(v =>
                    v.RegistrationNumber.ToLower().Contains(searchLower) ||
                    v.VehicleName.ToLower().Contains(searchLower) ||
                    v.VehicleType.ToLower().Contains(searchLower)
                ).ToList();
                FilteredVehicleStatistics = new ObservableCollection<VehicleStatistics>(filtered);
            }

            OnPropertyChanged(nameof(TotalVehiclesDisplay));
        }

        private async Task ExportAllDataAsync()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                FileName = $"Statistiques_Detaillees_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var result = await _exportService.ExportStatisticsToCsvAsync(AllVehicleStatistics.ToList(), saveDialog.FileName);
                if (result.Success)
                {
                    MessageBox.Show("Export CSV réalisé avec succès!", "Succès", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportToExcelAsync()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                FileName = $"Statistiques_Excel_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var result = await _exportService.ExportStatisticsToCsvAsync(AllVehicleStatistics.ToList(), saveDialog.FileName);
                if (result.Success)
                {
                    MessageBox.Show("Export Excel réalisé avec succès!\n\nVous pouvez ouvrir le fichier CSV dans Excel.", "Succès", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportToPdfAsync()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Fichiers PDF (*.pdf)|*.pdf",
                FileName = $"Rapport_Statistiques_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var fleetStats = new FleetStatistics
                    {
                        TotalVehicles = AllVehicleStatistics.Count,
                        ActiveVehicles = ActiveVehiclesCount,
                        TotalFuelCost = AllVehicleStatistics.Sum(v => v.TotalFuelCost),
                        TotalMaintenanceCost = AllVehicleStatistics.Sum(v => v.TotalMaintenanceCost),
                        AverageFleetConsumption = AverageConsumption,
                        TotalMileage = TotalKilometers
                    };

                    var result = _exportService.GenerateAdvancedPdfReport(
                        "Rapport de Statistiques Détaillées - Fleet Manager",
                        fleetStats,
                        AllVehicleStatistics.ToList(),
                        saveDialog.FileName
                    );

                    if (result.Success)
                    {
                        MessageBox.Show("Rapport PDF généré avec succès!", "Succès", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Erreur", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la génération du PDF:\n{ex.Message}", "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
