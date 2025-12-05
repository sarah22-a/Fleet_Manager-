using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FleetManager.ViewModels
{
    public class AdvancedChartsViewModel : INotifyPropertyChanged
    {
        private readonly StatisticsService _statisticsService;
        private readonly VehicleService _vehicleService;
        
        private bool _isLoading;
        private string _selectedChartType;
        private string _selectedPeriod;
        private string _selectedMetric;
        private DateTime _lastUpdateTime;
        private string _trendAnalysis;

        // Séries de graphiques
        private IEnumerable<ISeries> _timeSeriesData;
        private IEnumerable<Axis> _timeXAxes;
        private IEnumerable<Axis> _timeYAxes;

        private IEnumerable<ISeries> _comparisonSeriesData;
        private IEnumerable<Axis> _comparisonXAxes;
        private IEnumerable<Axis> _comparisonYAxes;

        private IEnumerable<ISeries> _trendSeriesData;
        private IEnumerable<Axis> _trendXAxes;
        private IEnumerable<Axis> _trendYAxes;

        private IEnumerable<ISeries> _costPieSeriesData;
        private IEnumerable<ISeries> _consumptionPieSeriesData;

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

        public AdvancedChartsViewModel(StatisticsService statisticsService, VehicleService vehicleService)
        {
            _statisticsService = statisticsService;
            _vehicleService = vehicleService;
            
            _selectedChartType = "Évolution temporelle";
            _selectedPeriod = "6 derniers mois";
            _selectedMetric = "Consommation";
            _lastUpdateTime = DateTime.Now;
            _trendAnalysis = "Chargement de l'analyse...";

            // Initialiser les séries vides
            _timeSeriesData = Array.Empty<ISeries>();
            _timeXAxes = Array.Empty<Axis>();
            _timeYAxes = Array.Empty<Axis>();
            _comparisonSeriesData = Array.Empty<ISeries>();
            _comparisonXAxes = Array.Empty<Axis>();
            _comparisonYAxes = Array.Empty<Axis>();
            _trendSeriesData = Array.Empty<ISeries>();
            _trendXAxes = Array.Empty<Axis>();
            _trendYAxes = Array.Empty<Axis>();
            _costPieSeriesData = Array.Empty<ISeries>();
            _consumptionPieSeriesData = Array.Empty<ISeries>();

            InitializeCollections();
            InitializeCommands();
            _ = LoadDataAsync();
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SelectedChartType
        {
            get => _selectedChartType;
            set
            {
                if (SetProperty(ref _selectedChartType, value))
                    _ = LoadDataAsync();
            }
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                    _ = LoadDataAsync();
            }
        }

        public string SelectedMetric
        {
            get => _selectedMetric;
            set
            {
                if (SetProperty(ref _selectedMetric, value))
                    _ = LoadDataAsync();
            }
        }

        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetProperty(ref _lastUpdateTime, value);
        }

        public string TrendAnalysis
        {
            get => _trendAnalysis;
            set => SetProperty(ref _trendAnalysis, value);
        }

        // Séries de graphiques
        public IEnumerable<ISeries> TimeSeriesData
        {
            get => _timeSeriesData;
            set => SetProperty(ref _timeSeriesData, value);
        }

        public IEnumerable<Axis> TimeXAxes
        {
            get => _timeXAxes;
            set => SetProperty(ref _timeXAxes, value);
        }

        public IEnumerable<Axis> TimeYAxes
        {
            get => _timeYAxes;
            set => SetProperty(ref _timeYAxes, value);
        }

        public IEnumerable<ISeries> ComparisonSeriesData
        {
            get => _comparisonSeriesData;
            set => SetProperty(ref _comparisonSeriesData, value);
        }

        public IEnumerable<Axis> ComparisonXAxes
        {
            get => _comparisonXAxes;
            set => SetProperty(ref _comparisonXAxes, value);
        }

        public IEnumerable<Axis> ComparisonYAxes
        {
            get => _comparisonYAxes;
            set => SetProperty(ref _comparisonYAxes, value);
        }

        public IEnumerable<ISeries> TrendSeriesData
        {
            get => _trendSeriesData;
            set => SetProperty(ref _trendSeriesData, value);
        }

        public IEnumerable<Axis> TrendXAxes
        {
            get => _trendXAxes;
            set => SetProperty(ref _trendXAxes, value);
        }

        public IEnumerable<Axis> TrendYAxes
        {
            get => _trendYAxes;
            set => SetProperty(ref _trendYAxes, value);
        }

        public IEnumerable<ISeries> CostPieSeriesData
        {
            get => _costPieSeriesData;
            set => SetProperty(ref _costPieSeriesData, value);
        }

        public IEnumerable<ISeries> ConsumptionPieSeriesData
        {
            get => _consumptionPieSeriesData;
            set => SetProperty(ref _consumptionPieSeriesData, value);
        }

        public List<string> ChartTypes { get; private set; } = new();
        public List<string> Periods { get; private set; } = new();
        public List<string> Metrics { get; private set; } = new();
        public ObservableCollection<Vehicle> AvailableVehicles { get; private set; } = new();
        public ObservableCollection<Vehicle> SelectedVehicles { get; private set; } = new();

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; private set; } = null!;
        public ICommand ExportCommand { get; private set; } = null!;
        public ICommand CloseCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            RefreshCommand = new RelayCommand(_ => _ = LoadDataAsync());
            ExportCommand = new RelayCommand(_ => ExportCharts());
            CloseCommand = new RelayCommand(param =>
            {
                if (param is Window window)
                    window.Close();
            });
        }

        #endregion

        #region Methods

        private void InitializeCollections()
        {
            ChartTypes = new List<string>
            {
                "Évolution temporelle",
                "Comparaison véhicules",
                "Tendances et prédictions",
                "Répartition"
            };

            Periods = new List<string>
            {
                "7 derniers jours",
                "30 derniers jours",
                "3 derniers mois",
                "6 derniers mois",
                "12 derniers mois",
                "Année en cours",
                "Année dernière"
            };

            Metrics = new List<string>
            {
                "Consommation",
                "Coûts carburant",
                "Coûts maintenance",
                "Coûts totaux",
                "Kilométrage",
                "Coût par kilomètre"
            };
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Charger les véhicules
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                AvailableVehicles.Clear();
                foreach (var vehicle in vehicles)
                {
                    AvailableVehicles.Add(vehicle);
                }

                // Déterminer la période
                var (startDate, endDate) = GetDateRangeFromPeriod(SelectedPeriod);

                // Charger les données selon la métrique sélectionnée
                await LoadTimeSeriesDataAsync(startDate, endDate);
                await LoadComparisonDataAsync();
                await LoadTrendDataAsync(startDate, endDate);
                await LoadPieChartsDataAsync();

                LastUpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTimeSeriesDataAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var monthsDiff = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
                var monthlyStats = await _statisticsService.GetMonthlyTrendsAsync(Math.Max(1, monthsDiff));
                
                if (!monthlyStats.Any())
                {
                    TimeSeriesData = Array.Empty<ISeries>();
                    return;
                }

                var labels = monthlyStats.Select(m => new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy")).ToArray();
                var values = monthlyStats.Select(m =>
                {
                    return SelectedMetric switch
                    {
                        "Consommation" => (double)m.AverageConsumption,
                        "Coûts carburant" => (double)m.FuelCost,
                        "Coûts maintenance" => (double)m.MaintenanceCost,
                        "Coûts totaux" => (double)m.TotalCost,
                        "Kilométrage" => (double)m.TotalMileage,
                        _ => 0.0
                    };
                }).ToArray();

                TimeSeriesData = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = values,
                        Name = SelectedMetric,
                        Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                        Fill = new LinearGradientPaint(
                            new SKColor(33, 150, 243, 100),
                            new SKColor(33, 150, 243, 20)),
                        GeometrySize = 8,
                        LineSmoothness = 0.5
                    }
                };

                TimeXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 45
                    }
                };

                TimeYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = SelectedMetric,
                        Labeler = value => value.ToString("N2")
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur LoadTimeSeriesDataAsync: {ex.Message}");
            }
        }

        private async Task LoadComparisonDataAsync()
        {
            try
            {
                var vehicleStats = await _statisticsService.GetAllVehicleStatisticsAsync();
                
                if (!vehicleStats.Any())
                {
                    ComparisonSeriesData = Array.Empty<ISeries>();
                    return;
                }

                var topVehicles = vehicleStats.OrderByDescending(v =>
                {
                    return SelectedMetric switch
                    {
                        "Consommation" => v.AverageConsumption,
                        "Coûts carburant" => v.TotalFuelCost,
                        "Coûts maintenance" => v.TotalMaintenanceCost,
                        "Coûts totaux" => v.TotalCost,
                        "Kilométrage" => v.CurrentMileage,
                        "Coût par kilomètre" => v.CostPerKilometer,
                        _ => 0
                    };
                }).Take(10).ToList();

                var labels = topVehicles.Select(v => v.RegistrationNumber).ToArray();
                var values = topVehicles.Select(v =>
                {
                    return SelectedMetric switch
                    {
                        "Consommation" => (double)v.AverageConsumption,
                        "Coûts carburant" => (double)v.TotalFuelCost,
                        "Coûts maintenance" => (double)v.TotalMaintenanceCost,
                        "Coûts totaux" => (double)v.TotalCost,
                        "Kilométrage" => (double)v.CurrentMileage,
                        "Coût par kilomètre" => (double)v.CostPerKilometer,
                        _ => 0.0
                    };
                }).ToArray();

                ComparisonSeriesData = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = values,
                        Name = SelectedMetric,
                        Fill = new SolidColorPaint(new SKColor(76, 175, 80)),
                        Stroke = new SolidColorPaint(new SKColor(56, 142, 60), 2)
                    }
                };

                ComparisonXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 45
                    }
                };

                ComparisonYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = SelectedMetric,
                        Labeler = value => value.ToString("N2")
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur LoadComparisonDataAsync: {ex.Message}");
            }
        }

        private async Task LoadTrendDataAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var monthsDiff = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
                var monthlyStats = await _statisticsService.GetMonthlyTrendsAsync(Math.Max(1, monthsDiff));
                
                if (monthlyStats.Count < 3)
                {
                    TrendAnalysis = "Données insuffisantes pour l'analyse prédictive (minimum 3 mois requis).";
                    TrendSeriesData = Array.Empty<ISeries>();
                    return;
                }

                // Données historiques
                var labels = monthlyStats.Select(m => new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy")).ToList();
                var values = monthlyStats.Select(m =>
                {
                    return SelectedMetric switch
                    {
                        "Consommation" => (double)m.AverageConsumption,
                        "Coûts carburant" => (double)m.FuelCost,
                        "Coûts maintenance" => (double)m.MaintenanceCost,
                        "Coûts totaux" => (double)m.TotalCost,
                        _ => 0.0
                    };
                }).ToList();

                // Calcul de la tendance (régression linéaire simple)
                var trend = CalculateLinearTrend(values.ToArray());
                
                // Prédiction pour les 3 prochains mois
                var predictedValues = new List<double>();
                for (int i = 0; i < 3; i++)
                {
                    predictedValues.Add(trend.slope * (values.Count + i) + trend.intercept);
                    labels.Add(startDate.AddMonths(monthlyStats.Count + i).ToString("MMM yyyy"));
                }

                TrendSeriesData = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = values,
                        Name = "Données réelles",
                        Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                        GeometrySize = 8
                    },
                    new LineSeries<double>
                    {
                        Values = predictedValues,
                        Name = "Prédiction",
                        Stroke = new SolidColorPaint(new SKColor(255, 152, 0), 3),
                        GeometrySize = 8
                    }
                };

                TrendXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels.ToArray(),
                        LabelsRotation = 45
                    }
                };

                TrendYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = SelectedMetric,
                        Labeler = value => value.ToString("N2")
                    }
                };

                // Analyse de la tendance
                var avgValue = values.Average();
                var lastValue = values.Last();
                var nextPrediction = predictedValues.First();
                var changePercent = ((nextPrediction - lastValue) / lastValue) * 100;

                TrendAnalysis = $"Tendance {(trend.slope > 0 ? "à la hausse" : "à la baisse")} détectée. " +
                               $"Valeur moyenne : {avgValue:F2}. " +
                               $"Valeur actuelle : {lastValue:F2}. " +
                               $"Prédiction mois prochain : {nextPrediction:F2} ({(changePercent > 0 ? "+" : "")}{changePercent:F1}%). " +
                               $"Recommandation : {(Math.Abs(changePercent) > 10 ? "Attention, variation importante prévue !" : "Évolution stable prévue.")}";
            }
            catch (Exception ex)
            {
                TrendAnalysis = $"Erreur lors du calcul des tendances: {ex.Message}";
            }
        }

        private async Task LoadPieChartsDataAsync()
        {
            try
            {
                var vehicleStats = await _statisticsService.GetAllVehicleStatisticsAsync();
                
                if (!vehicleStats.Any())
                {
                    CostPieSeriesData = Array.Empty<ISeries>();
                    ConsumptionPieSeriesData = Array.Empty<ISeries>();
                    return;
                }

                // Top 5 véhicules par coût
                var topCostVehicles = vehicleStats.OrderByDescending(v => v.TotalCost).Take(5).ToList();
                CostPieSeriesData = new ISeries[]
                {
                    new PieSeries<double>
                    {
                        Values = topCostVehicles.Select(v => (double)v.TotalCost).ToArray(),
                        Name = "Coûts totaux"
                    }
                };

                // Top 5 véhicules par consommation
                var topConsumptionVehicles = vehicleStats.OrderByDescending(v => v.TotalLiters).Take(5).ToList();
                ConsumptionPieSeriesData = new ISeries[]
                {
                    new PieSeries<double>
                    {
                        Values = topConsumptionVehicles.Select(v => (double)v.TotalLiters).ToArray(),
                        Name = "Consommation (litres)"
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur LoadPieChartsDataAsync: {ex.Message}");
            }
        }

        private (double slope, double intercept) CalculateLinearTrend(double[] values)
        {
            int n = values.Length;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += values[i];
                sumXY += i * values[i];
                sumX2 += i * i;
            }

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }

        private (DateTime startDate, DateTime endDate) GetDateRangeFromPeriod(string period)
        {
            var endDate = DateTime.Now;
            var startDate = period switch
            {
                "7 derniers jours" => endDate.AddDays(-7),
                "30 derniers jours" => endDate.AddDays(-30),
                "3 derniers mois" => endDate.AddMonths(-3),
                "6 derniers mois" => endDate.AddMonths(-6),
                "12 derniers mois" => endDate.AddMonths(-12),
                "Année en cours" => new DateTime(endDate.Year, 1, 1),
                "Année dernière" => new DateTime(endDate.Year - 1, 1, 1),
                _ => endDate.AddMonths(-6)
            };

            return (startDate, endDate);
        }

        private void ExportCharts()
        {
            MessageBox.Show("Fonctionnalité d'export en cours de développement.\n\nLes graphiques pourront être exportés en PNG, PDF ou Excel.",
                "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
