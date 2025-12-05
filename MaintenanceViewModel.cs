using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Models;
using FleetManager.Services;
using Microsoft.Win32;

namespace FleetManager.ViewModels
{
    public class MaintenanceViewModel : BaseViewModel
    {
        private readonly MaintenanceService _maintenanceService;
        private readonly VehicleService _vehicleService;
        private readonly ExportService _exportService;

        // Collections
        private ObservableCollection<MaintenanceRecordViewModel> _maintenanceRecords = new();
        private ObservableCollection<Vehicle> _vehicles = new();
        private ObservableCollection<string> _maintenanceTypes = new();

        // Sélections et filtres
        private MaintenanceRecordViewModel? _selectedMaintenance;
        private Vehicle? _selectedVehicle;
        private string? _selectedMaintenanceType;
        private DateTime _startDate = DateTime.Now.AddMonths(-6);
        private DateTime _endDate = DateTime.Now;
        private string _searchText = string.Empty;

        // Statistiques
        private int _totalMaintenances;
        private decimal _totalCost;

        // État
        private bool _isLoading;

        public ObservableCollection<MaintenanceRecordViewModel> MaintenanceRecords
        {
            get => _maintenanceRecords;
            set => SetProperty(ref _maintenanceRecords, value);
        }

        public ObservableCollection<Vehicle> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public ObservableCollection<string> MaintenanceTypes
        {
            get => _maintenanceTypes;
            set => SetProperty(ref _maintenanceTypes, value);
        }

        public MaintenanceRecordViewModel? SelectedMaintenance
        {
            get => _selectedMaintenance;
            set => SetProperty(ref _selectedMaintenance, value);
        }

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (SetProperty(ref _selectedVehicle, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string? SelectedMaintenanceType
        {
            get => _selectedMaintenanceType;
            set
            {
                if (SetProperty(ref _selectedMaintenanceType, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    _ = ApplyFiltersAsync();
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

        public int TotalMaintenances
        {
            get => _totalMaintenances;
            set => SetProperty(ref _totalMaintenances, value);
        }

        public decimal TotalCost
        {
            get => _totalCost;
            set => SetProperty(ref _totalCost, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commandes
        public ICommand AddMaintenanceCommand { get; }
        public ICommand EditMaintenanceCommand { get; }
        public ICommand DeleteMaintenanceCommand { get; }
        public ICommand ViewMaintenanceCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ExportCommand { get; }

        public MaintenanceViewModel(
            MaintenanceService maintenanceService,
            VehicleService vehicleService,
            ExportService exportService)
        {
            _maintenanceService = maintenanceService;
            _vehicleService = vehicleService;
            _exportService = exportService;

            // Initialiser les commandes
            AddMaintenanceCommand = new AsyncRelayCommand(AddMaintenanceAsync);
            EditMaintenanceCommand = new RelayCommand<MaintenanceRecordViewModel>(EditMaintenance);
            DeleteMaintenanceCommand = new AsyncRelayCommand<MaintenanceRecordViewModel>(DeleteMaintenanceAsync);
            ViewMaintenanceCommand = new RelayCommand<MaintenanceRecordViewModel>(ViewMaintenance);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            ResetFiltersCommand = new AsyncRelayCommand(ResetFiltersAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);

            // Initialiser les types de maintenance
            InitializeMaintenanceTypes();

            // Charger les données
            _ = LoadDataAsync(null);
        }

        private void InitializeMaintenanceTypes()
        {
            MaintenanceTypes = new ObservableCollection<string>
            {
                "Tous les types",
                "Vidange",
                "Révision",
                "Pneus",
                "Freins",
                "Courroie",
                "Climatisation",
                "Batterie",
                "Contrôle technique",
                "Réparation",
                "Carrosserie",
                "Autre"
            };
            SelectedMaintenanceType = "Tous les types";
        }

        private async Task LoadDataAsync(object? parameter)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("=== MAINTENANCE: Début chargement données ===");

                // Charger les véhicules
                System.Diagnostics.Debug.WriteLine("Chargement des véhicules...");
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                Vehicles = new ObservableCollection<Vehicle>(vehicles);
                System.Diagnostics.Debug.WriteLine($"Véhicules chargés: {Vehicles.Count}");

                // Charger les entretiens
                System.Diagnostics.Debug.WriteLine("Chargement des entretiens...");
                await LoadMaintenancesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERREUR MAINTENANCE: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                MessageBox.Show($"Erreur de chargement:\n\n{ex.Message}\n\nDétails: {ex.InnerException?.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("=== MAINTENANCE: Fin chargement données ===");
            }
        }

        private async Task LoadMaintenancesAsync()
        {
            System.Diagnostics.Debug.WriteLine("Appel MaintenanceService.GetAllMaintenancesAsync()...");
            var maintenances = await _maintenanceService.GetAllMaintenancesAsync();
            System.Diagnostics.Debug.WriteLine($"Entretiens récupérés: {maintenances.Count}");
            
            var viewModels = new List<MaintenanceRecordViewModel>();
            foreach (var maintenance in maintenances)
            {
                var vehicle = Vehicles.FirstOrDefault(v => v.VehicleId == maintenance.VehicleId);
                viewModels.Add(new MaintenanceRecordViewModel
                {
                    MaintenanceRecord = maintenance,
                    VehicleRegistration = vehicle?.RegistrationNumber ?? "N/A",
                    VehicleName = vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : "N/A"
                });
            }

            MaintenanceRecords = new ObservableCollection<MaintenanceRecordViewModel>(
                viewModels.OrderByDescending(m => m.MaintenanceRecord.MaintenanceDate));
            
            System.Diagnostics.Debug.WriteLine($"MaintenanceRecords ObservableCollection créée avec {MaintenanceRecords.Count} éléments");

            await ApplyFiltersAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            var filtered = MaintenanceRecords.AsEnumerable();

            // Filtre par véhicule
            if (SelectedVehicle != null)
            {
                filtered = filtered.Where(m => m.MaintenanceRecord.VehicleId == SelectedVehicle.VehicleId);
            }

            // Filtre par type
            if (!string.IsNullOrEmpty(SelectedMaintenanceType) && SelectedMaintenanceType != "Tous les types")
            {
                filtered = filtered.Where(m => m.MaintenanceRecord.MaintenanceType == SelectedMaintenanceType);
            }

            // Filtre par date
            filtered = filtered.Where(m => m.MaintenanceRecord.MaintenanceDate >= StartDate 
                && m.MaintenanceRecord.MaintenanceDate <= EndDate);

            // Filtre par recherche
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(m =>
                    m.VehicleRegistration.ToLower().Contains(search) ||
                    m.VehicleName.ToLower().Contains(search) ||
                    m.MaintenanceRecord.MaintenanceType.ToLower().Contains(search) ||
                    (m.MaintenanceRecord.Garage?.ToLower().Contains(search) ?? false) ||
                    m.MaintenanceRecord.Description.ToLower().Contains(search));
            }

            var result = filtered.ToList();
            MaintenanceRecords = new ObservableCollection<MaintenanceRecordViewModel>(result);

            // Mettre à jour les statistiques
            TotalMaintenances = result.Count;
            TotalCost = result.Sum(m => m.MaintenanceRecord.Cost);

            await Task.CompletedTask;
        }

        private async Task AddMaintenanceAsync(object? parameter)
        {
            try
            {
                var window = new Views.AddEditMaintenanceWindow(
                    _maintenanceService, 
                    _vehicleService,
                    null); // null = mode ajout
                
                if (window.ShowDialog() == true)
                {
                    await LoadMaintenancesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditMaintenance(MaintenanceRecordViewModel? maintenanceVM)
        {
            if (maintenanceVM == null) return;

            try
            {
                var window = new Views.AddEditMaintenanceWindow(
                    _maintenanceService,
                    _vehicleService,
                    maintenanceVM.MaintenanceRecord);

                if (window.ShowDialog() == true)
                {
                    _ = LoadMaintenancesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteMaintenanceAsync(MaintenanceRecordViewModel? maintenanceVM)
        {
            if (maintenanceVM == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Voulez-vous vraiment supprimer cet entretien?\n\n" +
                    $"Véhicule: {maintenanceVM.VehicleRegistration}\n" +
                    $"Type: {maintenanceVM.MaintenanceRecord.MaintenanceType}\n" +
                    $"Date: {maintenanceVM.MaintenanceRecord.MaintenanceDate:dd/MM/yyyy}\n" +
                    $"Coût: {maintenanceVM.MaintenanceRecord.Cost:C}",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var (success, message) = await _maintenanceService.DeleteMaintenanceAsync(
                        maintenanceVM.MaintenanceRecord.MaintenanceRecordId);

                    if (success)
                    {
                        MessageBox.Show("Entretien supprimé avec succès", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMaintenancesAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Erreur:\n\n{message}", "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewMaintenance(MaintenanceRecordViewModel? maintenanceVM)
        {
            if (maintenanceVM == null) return;

            var m = maintenanceVM.MaintenanceRecord;
            var details = $"📋 DÉTAILS DE L'ENTRETIEN\n\n" +
                $"Véhicule: {maintenanceVM.VehicleRegistration} ({maintenanceVM.VehicleName})\n" +
                $"Date: {m.MaintenanceDate:dd/MM/yyyy}\n" +
                $"Type: {m.MaintenanceType}\n" +
                $"Kilométrage: {m.Mileage:N0} km\n" +
                $"Coût: {m.Cost:C}\n\n" +
                $"Description:\n{m.Description}\n\n" +
                (string.IsNullOrEmpty(m.Garage) ? "" : $"Garage: {m.Garage}\n") +
                (string.IsNullOrEmpty(m.TechnicianName) ? "" : $"Technicien: {m.TechnicianName}\n") +
                (string.IsNullOrEmpty(m.Parts) ? "" : $"\nPièces: {m.Parts}\n") +
                (m.NextMaintenanceDate.HasValue ? $"\nProchaine maintenance prévue le: {m.NextMaintenanceDate:dd/MM/yyyy}\n" : "") +
                (m.NextMaintenanceMileage.HasValue ? $"Prochain entretien à: {m.NextMaintenanceMileage:N0} km\n" : "") +
                (string.IsNullOrEmpty(m.Notes) ? "" : $"\nNotes: {m.Notes}");

            MessageBox.Show(details, "Détails de l'entretien", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task ResetFiltersAsync(object? parameter)
        {
            SelectedVehicle = null;
            SelectedMaintenanceType = "Tous les types";
            StartDate = DateTime.Now.AddMonths(-6);
            EndDate = DateTime.Now;
            SearchText = string.Empty;

            await LoadMaintenancesAsync();
        }

        private async Task ExportAsync(object? parameter)
        {
            try
            {
                if (MaintenanceRecords.Count == 0)
                {
                    MessageBox.Show("Aucune donnée à exporter", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichiers CSV (*.csv)|*.csv",
                    FileName = $"Entretiens_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // TODO: Créer méthode d'export dans ExportService
                    MessageBox.Show("Export en cours de développement", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'export:\n\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// ViewModel pour un MaintenanceRecord avec informations du véhicule
    /// </summary>
    public class MaintenanceRecordViewModel
    {
        public MaintenanceRecord MaintenanceRecord { get; set; } = new();
        public string VehicleRegistration { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
    }
}
