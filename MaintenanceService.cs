using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManager.Models;

namespace FleetManager.Services
{
    /// <summary>
    /// Service de gestion des entretiens
    /// </summary>
    public class MaintenanceService
    {
        private readonly MaintenanceRepository _maintenanceRepository;
        private readonly VehicleService _vehicleService;

        public MaintenanceService(MaintenanceRepository maintenanceRepository, VehicleService vehicleService)
        {
            _maintenanceRepository = maintenanceRepository;
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Récupère tous les entretiens
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetAllMaintenancesAsync()
        {
            return await _maintenanceRepository.GetAllAsync();
        }

        /// <summary>
        /// Récupère les entretiens d'un véhicule
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetMaintenancesByVehicleAsync(int vehicleId)
        {
            return await _maintenanceRepository.GetByVehicleIdAsync(vehicleId);
        }

        /// <summary>
        /// Récupère les entretiens depuis une date
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetMaintenancesSinceDateAsync(DateTime sinceDate)
        {
            return await _maintenanceRepository.GetSinceDateAsync(sinceDate);
        }

        /// <summary>
        /// Récupère un entretien par ID
        /// </summary>
        public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int maintenanceId)
        {
            return await _maintenanceRepository.GetByIdAsync(maintenanceId);
        }

        /// <summary>
        /// Ajoute un nouvel entretien
        /// </summary>
        public async Task<(bool Success, string Message)> AddMaintenanceAsync(MaintenanceRecord maintenance)
        {
            try
            {
                // Validation
                var validationResult = ValidateMaintenance(maintenance);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Vérifier que le véhicule existe
                var vehicle = await _vehicleService.GetVehicleByIdAsync(maintenance.VehicleId);
                if (vehicle == null)
                {
                    return (false, "Véhicule introuvable");
                }

                // Ajouter l'entretien
                var maintenanceId = await _maintenanceRepository.AddAsync(maintenance);

                if (maintenanceId > 0)
                {
                    // Mettre à jour le kilométrage du véhicule si nécessaire
                    if (maintenance.Mileage > vehicle.CurrentMileage)
                    {
                        vehicle.CurrentMileage = maintenance.Mileage;
                        await _vehicleService.UpdateVehicleAsync(vehicle);
                    }

                    return (true, "Entretien ajouté avec succès");
                }

                return (false, "Erreur lors de l'ajout de l'entretien");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Met à jour un entretien existant
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateMaintenanceAsync(MaintenanceRecord maintenance)
        {
            try
            {
                // Validation
                var validationResult = ValidateMaintenance(maintenance);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Vérifier que l'entretien existe
                var existing = await _maintenanceRepository.GetByIdAsync(maintenance.MaintenanceRecordId);
                if (existing == null)
                {
                    return (false, "Entretien introuvable");
                }

                // Mettre à jour
                var success = await _maintenanceRepository.UpdateAsync(maintenance);

                if (success)
                {
                    // Mettre à jour le kilométrage du véhicule si nécessaire
                    var vehicle = await _vehicleService.GetVehicleByIdAsync(maintenance.VehicleId);
                    if (vehicle != null && maintenance.Mileage > vehicle.CurrentMileage)
                    {
                        vehicle.CurrentMileage = maintenance.Mileage;
                        await _vehicleService.UpdateVehicleAsync(vehicle);
                    }

                    return (true, "Entretien modifié avec succès");
                }

                return (false, "Erreur lors de la modification");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime un entretien
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteMaintenanceAsync(int maintenanceId)
        {
            try
            {
                var existing = await _maintenanceRepository.GetByIdAsync(maintenanceId);
                if (existing == null)
                {
                    return (false, "Entretien introuvable");
                }

                var success = await _maintenanceRepository.DeleteAsync(maintenanceId);

                if (success)
                {
                    return (true, "Entretien supprimé avec succès");
                }

                return (false, "Erreur lors de la suppression");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Valide un entretien
        /// </summary>
        private (bool Success, string Message) ValidateMaintenance(MaintenanceRecord maintenance)
        {
            if (maintenance.VehicleId <= 0)
            {
                return (false, "Veuillez sélectionner un véhicule");
            }

            if (string.IsNullOrWhiteSpace(maintenance.MaintenanceType))
            {
                return (false, "Veuillez spécifier le type d'entretien");
            }

            if (string.IsNullOrWhiteSpace(maintenance.Description))
            {
                return (false, "Veuillez entrer une description");
            }

            if (maintenance.Mileage < 0)
            {
                return (false, "Le kilométrage ne peut pas être négatif");
            }

            if (maintenance.Cost < 0)
            {
                return (false, "Le coût ne peut pas être négatif");
            }

            if (maintenance.MaintenanceDate > DateTime.Now)
            {
                return (false, "La date d'entretien ne peut pas être dans le futur");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Récupère les statistiques d'entretien
        /// </summary>
        public async Task<MaintenanceStatistics> GetMaintenanceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var maintenances = await GetAllMaintenancesAsync();

            if (startDate.HasValue)
            {
                maintenances = maintenances.Where(m => m.MaintenanceDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                maintenances = maintenances.Where(m => m.MaintenanceDate <= endDate.Value).ToList();
            }

            return new MaintenanceStatistics
            {
                TotalMaintenances = maintenances.Count,
                TotalCost = maintenances.Sum(m => m.Cost),
                AverageCost = maintenances.Any() ? maintenances.Average(m => m.Cost) : 0,
                CompletedMaintenances = maintenances.Count(m => m.Status == "Terminée"),
                InProgressMaintenances = maintenances.Count(m => m.Status == "En cours"),
                ScheduledMaintenances = maintenances.Count(m => m.Status == "Planifiée"),
                MostCommonType = maintenances.GroupBy(m => m.MaintenanceType)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A",
                MostExpensiveType = maintenances.GroupBy(m => m.MaintenanceType)
                    .OrderByDescending(g => g.Sum(m => m.Cost))
                    .FirstOrDefault()?.Key ?? "N/A"
            };
        }
    }

    /// <summary>
    /// Statistiques d'entretien
    /// </summary>
    public class MaintenanceStatistics
    {
        public int TotalMaintenances { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
        public int CompletedMaintenances { get; set; }
        public int InProgressMaintenances { get; set; }
        public int ScheduledMaintenances { get; set; }
        public string MostCommonType { get; set; } = string.Empty;
        public string MostExpensiveType { get; set; } = string.Empty;
    }
}
