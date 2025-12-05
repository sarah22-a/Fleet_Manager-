using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FleetManager.Services
{
    /// <summary>
    /// Modèle pour les objectifs de performance
    /// </summary>
    public class PerformanceTarget
    {
        public int VehicleId { get; set; }
        public string VehicleRegistration { get; set; } = string.Empty;
        public decimal TargetConsumption { get; set; }
        public decimal MaxMonthlyCost { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// Interface pour le service de gestion des objectifs
    /// </summary>
    public interface ITargetService
    {
        Task<List<PerformanceTarget>> GetAllTargetsAsync();
        Task<PerformanceTarget> GetTargetByVehicleAsync(int vehicleId);
        Task<bool> SaveTargetAsync(PerformanceTarget target);
        Task<bool> DeleteTargetAsync(int vehicleId);
        Task<(bool success, string message)> ValidateTargetAsync(PerformanceTarget target);
    }

    /// <summary>
    /// Service de gestion des objectifs de performance par véhicule
    /// </summary>
    public class TargetService : ITargetService
    {
        private List<PerformanceTarget> _targets = new();

        public TargetService()
        {
            InitializeSampleTargets();
        }

        /// <summary>
        /// Obtenir tous les objectifs
        /// </summary>
        public async Task<List<PerformanceTarget>> GetAllTargetsAsync()
        {
            return await Task.FromResult(new List<PerformanceTarget>(_targets));
        }

        /// <summary>
        /// Obtenir l'objectif d'un véhicule
        /// </summary>
        public async Task<PerformanceTarget> GetTargetByVehicleAsync(int vehicleId)
        {
            var target = _targets.FirstOrDefault(t => t.VehicleId == vehicleId);
            if (target == null)
            {
                // Retourner un objet par défaut au lieu de null pour éviter les avertissements de nullabilité
                target = new PerformanceTarget
                {
                    VehicleId = vehicleId,
                    VehicleRegistration = string.Empty,
                    TargetConsumption = 0,
                    MaxMonthlyCost = 0,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
            }
            return await Task.FromResult(target);
        }

        /// <summary>
        /// Sauvegarder ou mettre à jour un objectif
        /// </summary>
        public async Task<bool> SaveTargetAsync(PerformanceTarget target)
        {
            try
            {
                if (target == null)
                    return false;

                var existing = _targets.FirstOrDefault(t => t.VehicleId == target.VehicleId);
                if (existing != null)
                {
                    // Mettre à jour
                    existing.TargetConsumption = target.TargetConsumption;
                    existing.MaxMonthlyCost = target.MaxMonthlyCost;
                    existing.ModifiedDate = DateTime.Now;
                }
                else
                {
                    // Ajouter
                    target.CreatedDate = DateTime.Now;
                    target.ModifiedDate = DateTime.Now;
                    _targets.Add(target);
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur save target: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprimer un objectif
        /// </summary>
        public async Task<bool> DeleteTargetAsync(int vehicleId)
        {
            try
            {
                var target = _targets.FirstOrDefault(t => t.VehicleId == vehicleId);
                if (target != null)
                {
                    _targets.Remove(target);
                    return await Task.FromResult(true);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur delete target: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Valider un objectif
        /// </summary>
        public async Task<(bool, string)> ValidateTargetAsync(PerformanceTarget target)
        {
            if (target == null)
                return (false, "Objectif vide");

            if (target.TargetConsumption <= 0)
                return (false, "La consommation cible doit être positive");

            if (target.MaxMonthlyCost <= 0)
                return (false, "Le coût cible doit être positif");

            return await Task.FromResult((true, "Validation réussie"));
        }

        /// <summary>
        /// Initialiser quelques objectifs d'exemple
        /// </summary>
        private void InitializeSampleTargets()
        {
            _targets = new List<PerformanceTarget>
            {
                new() { VehicleId = 1, VehicleRegistration = "AA-123-BB", TargetConsumption = 7.5m, MaxMonthlyCost = 500m, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now },
                new() { VehicleId = 2, VehicleRegistration = "CC-456-DD", TargetConsumption = 8.0m, MaxMonthlyCost = 550m, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now }
            };
        }
    }
}
