using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Models;
using FleetManager.Services;

namespace FleetManager.ViewModels
{
    public class AddVehicleViewModel : BaseViewModel
    {
        private readonly VehicleService _vehicleService;
        private readonly AuthenticationService _authService;

        // Propriétés du véhicule
        private string _registrationNumber = string.Empty;
        private string _brand = string.Empty;
        private string _model = string.Empty;
        private int _year = DateTime.Now.Year;
        private string _vehicleType = "Berline";
        private string _fuelType = "Essence";
        private double _currentMileage;
        private double _fuelTankCapacity = 50;
        private double _averageFuelConsumption = 7;
        private DateTime _acquisitionDate = DateTime.Now;
        private decimal _purchasePrice;
        private string _status = "Actif";
        private string _description = string.Empty;

        // Propriétés de validation
        private string _validationMessage = string.Empty;
        private bool _hasValidationError;
        private bool _canSave = true;

        public AddVehicleViewModel(VehicleService vehicleService, AuthenticationService authService)
        {
            _vehicleService = vehicleService;
            _authService = authService;

            // Initialiser les commandes
            SaveCommand = new AsyncRelayCommand(SaveVehicleAsync, CanExecuteSave);
            CancelCommand = new RelayCommand(param => CancelAdd(param as Window));

            // Validation en temps réel
            PropertyChanged += OnPropertyChanged;
        }

        #region Propriétés du véhicule

        public string RegistrationNumber
        {
            get => _registrationNumber;
            set
            {
                SetProperty(ref _registrationNumber, value);
                ValidateForm();
            }
        }

        public string Brand
        {
            get => _brand;
            set
            {
                SetProperty(ref _brand, value);
                ValidateForm();
            }
        }

        public string Model
        {
            get => _model;
            set
            {
                SetProperty(ref _model, value);
                ValidateForm();
            }
        }

        public int Year
        {
            get => _year;
            set
            {
                SetProperty(ref _year, value);
                ValidateForm();
            }
        }

        // VIN supprimé du modèle Vehicle, donc propriété non utilisée

        public string VehicleType
        {
            get => _vehicleType;
            set => SetProperty(ref _vehicleType, value);
        }

        public string FuelType
        {
            get => _fuelType;
            set => SetProperty(ref _fuelType, value);
        }

        public double CurrentMileage
        {
            get => _currentMileage;
            set => SetProperty(ref _currentMileage, value);
        }

        public double FuelTankCapacity
        {
            get => _fuelTankCapacity;
            set => SetProperty(ref _fuelTankCapacity, value);
        }

        public double AverageFuelConsumption
        {
            get => _averageFuelConsumption;
            set => SetProperty(ref _averageFuelConsumption, value);
        }

        public DateTime AcquisitionDate
        {
            get => _acquisitionDate;
            set => SetProperty(ref _acquisitionDate, value);
        }

        public decimal PurchasePrice
        {
            get => _purchasePrice;
            set => SetProperty(ref _purchasePrice, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        #endregion

        #region Propriétés de validation

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

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ValidationMessage) &&
                e.PropertyName != nameof(HasValidationError) &&
                e.PropertyName != nameof(CanSave))
            {
                ValidateForm();
            }
        }

        private void ValidateForm()
        {
            var errors = new List<string>();

            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(RegistrationNumber))
                errors.Add("L'immatriculation est obligatoire");

            if (string.IsNullOrWhiteSpace(Brand))
                errors.Add("La marque est obligatoire");

            if (string.IsNullOrWhiteSpace(Model))
                errors.Add("Le modèle est obligatoire");

            if (Year < 1900 || Year > DateTime.Now.Year + 1)
                errors.Add("L'année doit être comprise entre 1900 et " + (DateTime.Now.Year + 1));

            // Validation du format de l'immatriculation (français)
            if (!string.IsNullOrWhiteSpace(RegistrationNumber))
            {
                var regexPattern = @"^[A-Z]{2}-\d{3}-[A-Z]{2}$|^\d{3,4}\s?[A-Z]{1,3}\s?\d{2}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(RegistrationNumber.ToUpper(), regexPattern))
                {
                    errors.Add("Format d'immatriculation invalide (ex: AB-123-CD ou 123 AB 45)");
                }
            }

            // VIN validation removed (VIN property no longer exists)

            // Validation des valeurs numériques
            if (CurrentMileage < 0)
                errors.Add("Le kilométrage ne peut pas être négatif");

            if (FuelTankCapacity <= 0)
                errors.Add("La capacité du réservoir doit être positive");

            if (AverageFuelConsumption <= 0)
                errors.Add("La consommation moyenne doit être positive");

            if (PurchasePrice < 0)
                errors.Add("Le prix d'achat ne peut pas être négatif");

            // Mettre à jour l'état de validation
            HasValidationError = errors.Count > 0;
            ValidationMessage = string.Join("\n", errors);
            CanSave = !HasValidationError;

            // Notifier le changement pour la commande
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSave(object? parameter)
        {
            return CanSave && !string.IsNullOrWhiteSpace(RegistrationNumber) &&
                   !string.IsNullOrWhiteSpace(Brand) && !string.IsNullOrWhiteSpace(Model);
        }

        private async Task SaveVehicleAsync(object? parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AJOUT VÉHICULE DÉMARRÉ ===");

                // Validation finale
                ValidateForm();
                if (HasValidationError)
                {
                    MessageBox.Show("Veuillez corriger les erreurs avant de continuer.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Créer l'objet véhicule
                var vehicle = new Vehicle
                {
                    RegistrationNumber = RegistrationNumber.ToUpper().Trim(),
                    Brand = Brand.Trim(),
                    Model = Model.Trim(),
                    Year = Year,
                    VehicleType = VehicleType,
                    FuelType = FuelType,
                    CurrentMileage = (decimal)CurrentMileage,
                    TankCapacity = (decimal)FuelTankCapacity,
                    AverageFuelConsumption = (decimal)AverageFuelConsumption,
                    PurchaseDate = AcquisitionDate,
                    PurchasePrice = PurchasePrice,
                    Status = Status,
                    Notes = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    CreatedAt = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"Création véhicule: {vehicle.RegistrationNumber} - {vehicle.Brand} {vehicle.Model}");

                // Sauvegarder via le service
                var (success, message) = await _vehicleService.AddVehicleAsync(vehicle);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("Véhicule ajouté avec succès");

                    MessageBox.Show("Véhicule ajouté avec succès !", "Succès",
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
                    System.Diagnostics.Debug.WriteLine($"Erreur ajout véhicule: {message}");
                    MessageBox.Show($"Erreur lors de l'ajout du véhicule:\n\n{message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception ajout véhicule: {ex.Message}");
                MessageBox.Show($"Erreur inattendue:\n\n{ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAdd(Window? window)
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir annuler ?\nTous les données saisies seront perdues.",
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
