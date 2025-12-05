using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetManager.Models;
using MySqlConnector;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Services
{
    /// <summary>
    /// Repository pour MaintenanceRecords utilisant ADO.NET directement
    /// Contourne Entity Framework Core pour éviter les problèmes de propriétés fantômes
    /// </summary>
    public class MaintenanceRepository
    {
        private readonly string _connectionString;

        public MaintenanceRepository(FleetDbContext context)
        {
            _connectionString = context.Database.GetConnectionString() ?? throw new InvalidOperationException("Connection string not found");
        }

        /// <summary>
        /// Charge tous les MaintenanceRecords
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetAllAsync()
        {
            return await LoadMaintenanceRecordsAsync();
        }

        /// <summary>
        /// Charge les MaintenanceRecords pour un véhicule spécifique
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetByVehicleIdAsync(int vehicleId)
        {
            return await LoadMaintenanceRecordsAsync("VehicleId = @vehicleId", 
                new MySqlParameter("@vehicleId", vehicleId));
        }

        /// <summary>
        /// Charge les MaintenanceRecords depuis une date
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetSinceDateAsync(DateTime sinceDate)
        {
            return await LoadMaintenanceRecordsAsync("MaintenanceDate >= @sinceDate",
                new MySqlParameter("@sinceDate", sinceDate));
        }

        /// <summary>
        /// Récupère un MaintenanceRecord par ID
        /// </summary>
        public async Task<MaintenanceRecord?> GetByIdAsync(int maintenanceId)
        {
            var records = await LoadMaintenanceRecordsAsync("MaintenanceRecordId = @id",
                new MySqlParameter("@id", maintenanceId));
            return records.Count > 0 ? records[0] : null;
        }

        /// <summary>
        /// Ajoute un nouveau MaintenanceRecord
        /// </summary>
        public async Task<int> AddAsync(MaintenanceRecord maintenance)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"INSERT INTO MaintenanceRecords 
                (VehicleId, MaintenanceDate, Mileage, MaintenanceType, Description, Cost, 
                Garage, NextMaintenanceDate, NextMaintenanceMileage, Parts, TechnicianName, Status, Notes, CreatedAt)
                VALUES (@VehicleId, @MaintenanceDate, @Mileage, @MaintenanceType, @Description, @Cost,
                @Garage, @NextMaintenanceDate, @NextMaintenanceMileage, @Parts, @TechnicianName, @Status, @Notes, @CreatedAt);
                SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@VehicleId", maintenance.VehicleId);
            command.Parameters.AddWithValue("@MaintenanceDate", maintenance.MaintenanceDate);
            command.Parameters.AddWithValue("@Mileage", maintenance.Mileage);
            command.Parameters.AddWithValue("@MaintenanceType", maintenance.MaintenanceType);
            command.Parameters.AddWithValue("@Description", maintenance.Description);
            command.Parameters.AddWithValue("@Cost", maintenance.Cost);
            command.Parameters.AddWithValue("@Garage", (object?)maintenance.Garage ?? DBNull.Value);
            command.Parameters.AddWithValue("@NextMaintenanceDate", (object?)maintenance.NextMaintenanceDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@NextMaintenanceMileage", (object?)maintenance.NextMaintenanceMileage ?? DBNull.Value);
            command.Parameters.AddWithValue("@Parts", (object?)maintenance.Parts ?? DBNull.Value);
            command.Parameters.AddWithValue("@TechnicianName", (object?)maintenance.TechnicianName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", maintenance.Status);
            command.Parameters.AddWithValue("@Notes", (object?)maintenance.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", maintenance.CreatedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Met à jour un MaintenanceRecord existant
        /// </summary>
        public async Task<bool> UpdateAsync(MaintenanceRecord maintenance)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"UPDATE MaintenanceRecords SET
                VehicleId = @VehicleId, MaintenanceDate = @MaintenanceDate, Mileage = @Mileage,
                MaintenanceType = @MaintenanceType, Description = @Description, Cost = @Cost,
                Garage = @Garage, NextMaintenanceDate = @NextMaintenanceDate, 
                NextMaintenanceMileage = @NextMaintenanceMileage, Parts = @Parts,
                TechnicianName = @TechnicianName, Status = @Status, Notes = @Notes
                WHERE MaintenanceRecordId = @MaintenanceRecordId";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@MaintenanceRecordId", maintenance.MaintenanceRecordId);
            command.Parameters.AddWithValue("@VehicleId", maintenance.VehicleId);
            command.Parameters.AddWithValue("@MaintenanceDate", maintenance.MaintenanceDate);
            command.Parameters.AddWithValue("@Mileage", maintenance.Mileage);
            command.Parameters.AddWithValue("@MaintenanceType", maintenance.MaintenanceType);
            command.Parameters.AddWithValue("@Description", maintenance.Description);
            command.Parameters.AddWithValue("@Cost", maintenance.Cost);
            command.Parameters.AddWithValue("@Garage", (object?)maintenance.Garage ?? DBNull.Value);
            command.Parameters.AddWithValue("@NextMaintenanceDate", (object?)maintenance.NextMaintenanceDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@NextMaintenanceMileage", (object?)maintenance.NextMaintenanceMileage ?? DBNull.Value);
            command.Parameters.AddWithValue("@Parts", (object?)maintenance.Parts ?? DBNull.Value);
            command.Parameters.AddWithValue("@TechnicianName", (object?)maintenance.TechnicianName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", maintenance.Status);
            command.Parameters.AddWithValue("@Notes", (object?)maintenance.Notes ?? DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Supprime un MaintenanceRecord
        /// </summary>
        public async Task<bool> DeleteAsync(int maintenanceId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "DELETE FROM MaintenanceRecords WHERE MaintenanceRecordId = @id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", maintenanceId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Compte les MaintenanceRecords
        /// </summary>
        public async Task<int> CountAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("SELECT COUNT(*) FROM MaintenanceRecords", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Charge les MaintenanceRecords avec requête SQL
        /// </summary>
        private async Task<List<MaintenanceRecord>> LoadMaintenanceRecordsAsync(string? whereClause = null, params MySqlParameter[] parameters)
        {
            var records = new List<MaintenanceRecord>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"SELECT MaintenanceRecordId, VehicleId, MaintenanceDate, Mileage, MaintenanceType, 
                        Description, Cost, Garage, NextMaintenanceDate, NextMaintenanceMileage, 
                        Parts, TechnicianName, Status, Notes, CreatedAt 
                        FROM MaintenanceRecords";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += " WHERE " + whereClause;
            }

            using var command = new MySqlCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                records.Add(new MaintenanceRecord
                {
                    MaintenanceRecordId = reader.GetInt32("MaintenanceRecordId"),
                    VehicleId = reader.GetInt32("VehicleId"),
                    MaintenanceDate = reader.GetDateTime("MaintenanceDate"),
                    Mileage = reader.GetDecimal("Mileage"),
                    MaintenanceType = reader.GetString("MaintenanceType"),
                    Description = reader.GetString("Description"),
                    Cost = reader.GetDecimal("Cost"),
                    Garage = reader.IsDBNull(reader.GetOrdinal("Garage")) ? null : reader.GetString("Garage"),
                    NextMaintenanceDate = reader.IsDBNull(reader.GetOrdinal("NextMaintenanceDate")) ? null : reader.GetDateTime("NextMaintenanceDate"),
                    NextMaintenanceMileage = reader.IsDBNull(reader.GetOrdinal("NextMaintenanceMileage")) ? null : reader.GetDecimal("NextMaintenanceMileage"),
                    Parts = reader.IsDBNull(reader.GetOrdinal("Parts")) ? null : reader.GetString("Parts"),
                    TechnicianName = reader.IsDBNull(reader.GetOrdinal("TechnicianName")) ? null : reader.GetString("TechnicianName"),
                    Status = reader.GetString("Status"),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString("Notes"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return records;
        }
    }
}
