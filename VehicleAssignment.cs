using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models
{
    /// <summary>
    /// Modèle représentant une affectation de véhicule à un conducteur
    /// </summary>
    [Table("VehicleAssignments")]
    public class VehicleAssignment
    {
        [Key]
        [Column("AssignmentId")]
        public int AssignmentId { get; set; }

        [Required]
        [Column("VehicleId")]
        public int VehicleId { get; set; }

        [Required]
        [Column("DriverId")]
        public int DriverId { get; set; }

        [Required]
        [Column("AssignmentDate")]
        public DateTime AssignmentDate { get; set; } = DateTime.Now;

        [Column("ReturnDate")]
        public DateTime? ReturnDate { get; set; }

        [Column("StartMileage", TypeName = "decimal(10,2)")]
        public decimal? StartMileage { get; set; }

        [Column("EndMileage", TypeName = "decimal(10,2)")]
        public decimal? EndMileage { get; set; }

        [Column("Status")]
        public string Status { get; set; } = "EnCours";

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; } = null!;
    }
}