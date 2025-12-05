using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    /// <summary>
    /// Service de gestion des véhicules
    /// </summary>
    public class VehicleService
    {
        private readonly FleetDbContext _context;

        public VehicleService(FleetDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les véhicules
        /// </summary>
        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère un véhicule par son ID
        /// </summary>
        public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
        {
            return await _context.Vehicles.FindAsync(vehicleId);
        }

        /// <summary>
        /// Recherche des véhicules
        /// </summary>
        /// <summary>
        /// Ajoute un nouveau véhicule
        /// </summary>
        public async Task<(bool Success, string Message)> AddVehicleAsync(Vehicle vehicle)
        {
            try
            {
                // Validation des données
                if (vehicle == null)
                {
                    return (false, "Les données du véhicule sont requises.");
                }

                if (string.IsNullOrWhiteSpace(vehicle.RegistrationNumber))
                {
                    return (false, "Le numéro d'immatriculation est requis.");
                }

                // Vérifier l'unicité de l'immatriculation
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.RegistrationNumber == vehicle.RegistrationNumber);

                if (existingVehicle != null)
                {
                    return (false, $"Un véhicule avec l'immatriculation '{vehicle.RegistrationNumber}' existe déjà.");
                }

                // Ajouter le véhicule
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                return (true, "Véhicule ajouté avec succès.");
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Erreur de base de données : {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'ajout : {ex.Message}");
            }
        }

        /// <summary>
        /// Recherche des véhicules
        /// </summary>
        public async Task<List<Vehicle>> SearchVehiclesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllVehiclesAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Vehicles
                .Where(v => v.RegistrationNumber.ToLower().Contains(searchTerm) ||
                           v.Brand.ToLower().Contains(searchTerm) ||
                           v.Model.ToLower().Contains(searchTerm))
                .ToListAsync();
        }



        /// <summary>
        /// Met à jour un véhicule
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                var existing = await _context.Vehicles.FindAsync(vehicle.VehicleId);
                if (existing == null)
                {
                    return (false, "Véhicule introuvable.");
                }

                // Vérifier si l'immatriculation n'est pas déjà utilisée par un autre véhicule
                var duplicate = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.RegistrationNumber == vehicle.RegistrationNumber &&
                                             v.VehicleId != vehicle.VehicleId);

                if (duplicate != null)
                {
                    return (false, "Cette immatriculation est déjà utilisée par un autre véhicule.");
                }

                // Mettre à jour les propriétés
                existing.RegistrationNumber = vehicle.RegistrationNumber;
                existing.Brand = vehicle.Brand;
                existing.Model = vehicle.Model;
                existing.Year = vehicle.Year;
                existing.VehicleType = vehicle.VehicleType;
                existing.FuelType = vehicle.FuelType;
                existing.CurrentMileage = vehicle.CurrentMileage;
                existing.TankCapacity = vehicle.TankCapacity;
                existing.AverageFuelConsumption = vehicle.AverageFuelConsumption;
                existing.Status = vehicle.Status;
                existing.PurchaseDate = vehicle.PurchaseDate;
                existing.PurchasePrice = vehicle.PurchasePrice;
                existing.InsuranceExpiryDate = vehicle.InsuranceExpiryDate;
                existing.TechnicalInspectionDate = vehicle.TechnicalInspectionDate;
                existing.Notes = vehicle.Notes;

                await _context.SaveChangesAsync();
                return (true, "Véhicule mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime un véhicule
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteVehicleAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                {
                    return (false, "Véhicule introuvable.");
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                return (true, "Véhicule supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère les statistiques d'un véhicule
        /// </summary>
        public async Task<VehicleStatistics> GetVehicleStatisticsAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                return new VehicleStatistics();

            var maintenanceRepo = new MaintenanceRepository(_context);

            var fuelRecords = await _context.FuelRecords
                .Where(f => f.VehicleId == vehicleId)
                .ToListAsync();

            var maintenanceRecords = await maintenanceRepo.GetByVehicleIdAsync(vehicleId);

            var stats = new VehicleStatistics
            {
                VehicleId = vehicle.VehicleId,
                VehicleName = $"{vehicle.Brand} {vehicle.Model}",
                RegistrationNumber = vehicle.RegistrationNumber,
                CurrentMileage = vehicle.CurrentMileage,
                TotalRefuels = fuelRecords.Count,
                TotalLiters = fuelRecords.Sum(f => f.LitersRefueled),
                TotalFuelCost = fuelRecords.Sum(f => f.TotalCost),
                AverageConsumption = fuelRecords.Where(f => f.CalculatedConsumption > 0).DefaultIfEmpty().Average(f => f?.CalculatedConsumption ?? 0),
                AveragePricePerLiter = fuelRecords.Any() ? fuelRecords.Average(f => f.PricePerLiter) : 0,
                TotalMaintenances = maintenanceRecords.Count,
                TotalMaintenanceCost = maintenanceRecords.Sum(m => m.Cost),
                LastMaintenanceDate = maintenanceRecords.OrderByDescending(m => m.MaintenanceDate).FirstOrDefault()?.MaintenanceDate,
                NextMaintenanceDate = maintenanceRecords.OrderByDescending(m => m.NextMaintenanceDate).FirstOrDefault()?.NextMaintenanceDate
            };

            return stats;
        }
    }

    
}
