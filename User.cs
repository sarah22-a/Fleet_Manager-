using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models
{
    /// <summary>
    /// Modèle représentant un utilisateur du système
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("Email")]
        public string? Email { get; set; }

        [Column("Role")]
        public string Role { get; set; } = "User";

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("LastLogin")]
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public virtual ICollection<FuelRecord> FuelRecords { get; set; } = new List<FuelRecord>();
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    }
}