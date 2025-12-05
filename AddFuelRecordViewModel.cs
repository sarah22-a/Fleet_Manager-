using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Models;
using FleetManager.Services;

namespace FleetManager.ViewModels
{
    public class AddFuelRecordViewModel : BaseViewModel
    {
        private readonly FuelService _fuelService;
        private readonly VehicleService _vehicleService;
        private readonly AuthenticationService _authService;

        // Collections
        private ObservableCollection<Vehicle> _availableVehicles = new();

        // Propriétés du plein
        private Vehicle? _selectedVehicle;
        private decimal _currentMileage;
        private string _fuelType = "Essence";
        private decimal _quantityLiters;
        private decimal _pricePerLiter;
        private decimal _totalCost;
        private DateTime _refuelDate = DateTime.Now;
        private string _stationName = string.Empty;
        private bool _isFullTank = true;
        private string _notes = string.Empty;

        // Propriétés calculées
        private decimal _distanceTraveled;
        private decimal _fuelConsumption;

        // Propriétés de validation
        private string _validationMessage = string.Empty;
        private bool _hasValidationError;
        private bool _canSave = true;

        public AddFuelRecordViewModel(FuelService fuelService, VehicleService vehicleService, AuthenticationService authService)
        {
            _fuelService = fuelService;
            _vehicleService = vehicleService;
            _authService = authService;

            // Initialiser les commandes
            SaveCommand = new AsyncRelayCommand(SaveFuelRecordAsync, CanExecuteSave);
            CancelCommand = new RelayCommand(param => CancelAdd(param as Window));

            // Charger les véhicules disponibles
            _ = LoadAvailableVehiclesAsync();

            // Validation en temps réel
            PropertyChanged += OnPropertyChanged;
        }

        #region Propriétés

        public ObservableCollection<Vehicle> AvailableVehicles
        {
            get => _availableVehicles;
            set => SetProperty(ref _availableVehicles, value);
        }

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (SetProperty(ref _selectedVehicle, value))
                {
                    OnVehicleSelected();
                    ValidateForm();
                }
            }
        }

        public decimal CurrentMileage
        {
            get => _currentMileage;
            set
            {
                if (SetProperty(ref _currentMileage, value))
                {
                    CalculateDistanceAndConsumption();
                    ValidateForm();
                }
            }
        }

        public string FuelType
        {
            get => _fuelType;
            set => SetProperty(ref _fuelType, value);
        }

        public decimal QuantityLiters
        {
            get => _quantityLiters;
            set
            {
                if (SetProperty(ref _quantityLiters, value))
                {
                    CalculateTotalCost();
                    CalculateDistanceAndConsumption();
                    ValidateForm();
                }
            }
        }

        public decimal PricePerLiter
        {
            get => _pricePerLiter;
            set
            {
                if (SetProperty(ref _pricePerLiter, value))
                {
                    CalculateTotalCost();
                    ValidateForm();
                }
            }
        }

        public decimal TotalCost
        {
            get => _totalCost;
            set => SetProperty(ref _totalCost, value);
        }

        public DateTime RefuelDate
        {
            get => _refuelDate;
            set
            {
                SetProperty(ref _refuelDate, value);
                ValidateForm();
            }
        }

        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        public bool IsFullTank
        {
            get => _isFullTank;
            set
            {
                if (SetProperty(ref _isFullTank, value))
                {
                    CalculateDistanceAndConsumption();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public decimal DistanceTraveled
        {
            get => _distanceTraveled;
            set => SetProperty(ref _distanceTraveled, value);
        }

        public decimal FuelConsumption
        {
            get => _fuelConsumption;
            set => SetProperty(ref _fuelConsumption, value);
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            set => SetProperty(ref _hasValidationError, value);
        }

        public bool CanSave
        {
            get => _canSave;
            set => SetProperty(ref _canSave, value);
        }

        #endregion

        #region Commandes

        public AsyncRelayCommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Méthodes

        private async Task LoadAvailableVehiclesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Chargement des véhicules disponibles...");
                var vehicles = await _vehicleService.GetAllVehiclesAsync();

                // Filtrer les véhicules actifs uniquement
                var activeVehicles = vehicles.Where(v => v.Status == "Actif").ToList();

                AvailableVehicles = new ObservableCollection<Vehicle>(activeVehicles);
                System.Diagnostics.Debug.WriteLine($"{activeVehicles.Count} véhicules actifs chargés");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement véhicules: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des véhicules: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void OnVehicleSelected()
        {
            if (SelectedVehicle == null) return;

            try
            {
                // Définir le type de carburant selon le véhicule
                FuelType = SelectedVehicle.FuelType; // Already string

                // Définir le kilométrage actuel du véhicule
                CurrentMileage = SelectedVehicle.CurrentMileage;

                System.Diagnostics.Debug.WriteLine($"Véhicule sélectionné: {SelectedVehicle.DisplayName}");
                System.Diagnostics.Debug.WriteLine($"Kilométrage: {CurrentMileage}, Carburant: {FuelType}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur sélection véhicule: {ex.Message}");
            }
        }

        private void CalculateTotalCost()
        {
            TotalCost = QuantityLiters * PricePerLiter;
        }

        private async void CalculateDistanceAndConsumption()
        {
            if (SelectedVehicle == null || !IsFullTank || QuantityLiters <= 0)
            {
                DistanceTraveled = 0;
                FuelConsumption = 0;
                return;
            }

            try
            {
                // Récupérer le dernier plein pour ce véhicule
                var fuelRecords = await _fuelService.GetFuelRecordsByVehicleAsync(SelectedVehicle.VehicleId);
                var lastFuelRecord = fuelRecords.FirstOrDefault();

                if (lastFuelRecord != null && lastFuelRecord.IsFullTank)
                {
                    // Calculer la distance parcourue
                    DistanceTraveled = CurrentMileage - lastFuelRecord.Mileage;

                    // Calculer la consommation (L/100km)
                    if (DistanceTraveled > 0)
                    {
                        FuelConsumption = Math.Round((QuantityLiters * 100) / DistanceTraveled, 2);
                    }
                    else
                    {
                        FuelConsumption = 0;
                    }
                }
                else
                {
                    DistanceTraveled = 0;
                    FuelConsumption = 0;
                }

                System.Diagnostics.Debug.WriteLine($"Distance: {DistanceTraveled} km, Consommation: {FuelConsumption} L/100km");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur calcul consommation: {ex.Message}");
                DistanceTraveled = 0;
                FuelConsumption = 0;
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ValidationMessage) &&
                e.PropertyName != nameof(HasValidationError) &&
                e.PropertyName != nameof(CanSave) &&
                e.PropertyName != nameof(TotalCost) &&
                e.PropertyName != nameof(DistanceTraveled) &&
                e.PropertyName != nameof(FuelConsumption))
            {
                ValidateForm();
            }
        }

        private void ValidateForm()
        {
            var errors = new List<string>();

            // Validation des champs obligatoires
            if (SelectedVehicle == null)
                errors.Add("Veuillez sélectionner un véhicule");

            if (CurrentMileage <= 0)
                errors.Add("Le kilométrage doit être positif");

            if (QuantityLiters <= 0)
                errors.Add("La quantité de carburant doit être positive");

            if (PricePerLiter <= 0)
                errors.Add("Le prix par litre doit être positif");

            if (RefuelDate > DateTime.Now)
                errors.Add("La date du plein ne peut pas être dans le futur");

            // Validation logique
            if (SelectedVehicle != null && CurrentMileage < SelectedVehicle.CurrentMileage)
            {
                errors.Add("Le kilométrage ne peut pas être inférieur au kilométrage actuel du véhicule");
            }

            if (QuantityLiters > 200)
                errors.Add("La quantité semble excessive (>200L)");

            if (PricePerLiter > 5)
                errors.Add("Le prix par litre semble excessif (>5€)");

            // Mettre à jour l'état de validation
            HasValidationError = errors.Count > 0;
            ValidationMessage = string.Join("\n", errors);
            CanSave = !HasValidationError;

            // Notifier le changement pour la commande
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSave(object? parameter)
        {
            return CanSave && SelectedVehicle != null && QuantityLiters > 0 && PricePerLiter > 0;
        }

        private async Task SaveFuelRecordAsync(object? parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AJOUT PLEIN CARBURANT DÉMARRÉ ===");

                // Validation finale
                ValidateForm();
                if (HasValidationError)
                {
                    MessageBox.Show("Veuillez corriger les erreurs avant de continuer.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedVehicle == null)
                {
                    MessageBox.Show("Veuillez sélectionner un véhicule.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Créer l'enregistrement de carburant
                var fuelRecord = new FuelRecord
                {
                    VehicleId = SelectedVehicle.VehicleId,
                    RefuelDate = RefuelDate,
                    FuelType = FuelType,
                    LitersRefueled = QuantityLiters,
                    PricePerLiter = PricePerLiter,
                    TotalCost = TotalCost,
                    Mileage = CurrentMileage,
                    IsFullTank = IsFullTank,
                    Station = string.IsNullOrWhiteSpace(StationName) ? null : StationName.Trim(),
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                    CreatedAt = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"Création plein: {SelectedVehicle.RegistrationNumber} - {QuantityLiters}L à {PricePerLiter}€/L");

                // Sauvegarder via le service
                var (success, message) = await _fuelService.AddFuelRecordAsync(fuelRecord);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("Plein ajouté avec succès");

                    // Mettre à jour le kilométrage du véhicule si nécessaire
                    if (CurrentMileage > SelectedVehicle.CurrentMileage)
                    {
                        SelectedVehicle.CurrentMileage = CurrentMileage;
                        await _vehicleService.UpdateVehicleAsync(SelectedVehicle);
                    }

                    MessageBox.Show("Plein de carburant enregistré avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Fermer la fenêtre
                    if (parameter is Window window)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur ajout plein: {message}");
                    MessageBox.Show($"Erreur lors de l'enregistrement du plein:\n\n{message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception ajout plein: {ex.Message}");
                MessageBox.Show($"Erreur inattendue:\n\n{ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAdd(Window? window)
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir annuler ?\nToutes les données saisies seront perdues.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        #endregion
    }
}
