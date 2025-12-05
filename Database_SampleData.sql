-- ============================================
-- FLEET MANAGER - Script de données de test
-- Base de données MySQL
-- Version: 2.0
-- Date: 2025-11-24
-- ============================================

-- Création de la base de données
DROP DATABASE IF EXISTS fleet_manager;
CREATE DATABASE fleet_manager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE fleet_manager;

-- ============================================
-- 1. TABLE USERS (Utilisateurs)
-- ============================================
CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Role VARCHAR(20) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLogin DATETIME NULL,
    INDEX idx_username (Username),
    INDEX idx_role (Role)
) ENGINE=InnoDB;

-- Mot de passe: "Admin123!" (hashé avec BCrypt)
-- Rôles disponibles: SuperAdmin, Admin, User
INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, IsActive, LastLogin) VALUES
('superadmin', '$2a$11$8Z3qN5x7Y2hWJKf.3LZxY.NwF5mVZ6p8QxN3C1K8L9mB5H7J6D4Fy', 'Admin Système', 'superadmin@fleetmanager.fr', 'SuperAdmin', TRUE, '2025-11-24 08:00:00'),
('admin', '$2a$11$8Z3qN5x7Y2hWJKf.3LZxY.NwF5mVZ6p8QxN3C1K8L9mB5H7J6D4Fy', 'Jean Dupont', 'admin@fleetmanager.fr', 'Admin', TRUE, '2025-11-24 08:30:00'),
('user1', '$2a$11$8Z3qN5x7Y2hWJKf.3LZxY.NwF5mVZ6p8QxN3C1K8L9mB5H7J6D4Fy', 'Pierre Bernard', 'pierre.bernard@fleetmanager.fr', 'User', TRUE, '2025-11-24 09:00:00'),
('user2', '$2a$11$8Z3qN5x7Y2hWJKf.3LZxY.NwF5mVZ6p8QxN3C1K8L9mB5H7J6D4Fy', 'Sophie Dubois', 'sophie.dubois@fleetmanager.fr', 'User', TRUE, '2025-11-23 14:20:00'),
('user3', '$2a$11$8Z3qN5x7Y2hWJKf.3LZxY.NwF5mVZ6p8QxN3C1K8L9mB5H7J6D4Fy', 'Marie Martin', 'marie.martin@fleetmanager.fr', 'User', TRUE, '2025-11-23 10:15:00');

-- ============================================
-- 2. TABLE DRIVERS (Conducteurs)
-- ============================================
CREATE TABLE Drivers (
    DriverId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    LicenseNumber VARCHAR(20) NOT NULL UNIQUE,
    LicenseExpiryDate DATE NOT NULL,
    PhoneNumber VARCHAR(20),
    Email VARCHAR(100),
    HireDate DATE NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Actif',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

INSERT INTO Drivers (FirstName, LastName, LicenseNumber, LicenseExpiryDate, PhoneNumber, Email, HireDate, Status) VALUES
('Thomas', 'Lefebvre', 'DL123456789', '2027-06-15', '06 12 34 56 78', 'thomas.lefebvre@email.fr', '2020-03-15', 'Actif'),
('Julie', 'Moreau', 'DL987654321', '2028-02-20', '06 23 45 67 89', 'julie.moreau@email.fr', '2019-05-10', 'Actif'),
('Marc', 'Simon', 'DL456789123', '2026-11-30', '06 34 56 78 90', 'marc.simon@email.fr', '2021-01-20', 'Actif'),
('Isabelle', 'Laurent', 'DL789123456', '2027-09-10', '06 45 67 89 01', 'isabelle.laurent@email.fr', '2018-08-05', 'Actif'),
('Nicolas', 'Michel', 'DL321654987', '2026-04-25', '06 56 78 90 12', 'nicolas.michel@email.fr', '2022-06-12', 'Actif'),
('Claire', 'Garcia', 'DL654987321', '2028-07-18', '06 67 89 01 23', 'claire.garcia@email.fr', '2020-11-30', 'Actif'),
('Philippe', 'Roux', 'DL147258369', '2027-01-05', '06 78 90 12 34', 'philippe.roux@email.fr', '2019-03-22', 'Actif'),
('Sandrine', 'Fontaine', 'DL963852741', '2026-08-14', '06 89 01 23 45', 'sandrine.fontaine@email.fr', '2021-09-15', 'Actif');

-- ============================================
-- 3. TABLE VEHICLES (Véhicules)
-- ============================================
CREATE TABLE Vehicles (
    VehicleId INT AUTO_INCREMENT PRIMARY KEY,
    RegistrationNumber VARCHAR(20) NOT NULL UNIQUE,
    Brand VARCHAR(50) NOT NULL,
    Model VARCHAR(50) NOT NULL,
    Year INT NOT NULL,
    VehicleType VARCHAR(20) NOT NULL COMMENT 'Voiture, Camion, Moto, Utilitaire',
    FuelType VARCHAR(20) NOT NULL COMMENT 'Essence, Diesel, Electrique, Hybride, GPL',
    CurrentMileage DECIMAL(10,2) NOT NULL DEFAULT 0,
    TankCapacity DECIMAL(5,2) NOT NULL,
    AverageFuelConsumption DECIMAL(5,2) NOT NULL DEFAULT 0,
    Status VARCHAR(20) NOT NULL DEFAULT 'Actif' COMMENT 'Actif, EnMaintenance, HorsService, Vendu',
    PurchaseDate DATE,
    PurchasePrice DECIMAL(10,2),
    InsuranceExpiryDate DATE,
    TechnicalInspectionDate DATE,
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_status (Status),
    INDEX idx_registration (RegistrationNumber),
    INDEX idx_vehicle_type (VehicleType),
    INDEX idx_fuel_type (FuelType)
) ENGINE=InnoDB;

INSERT INTO Vehicles (RegistrationNumber, Brand, Model, Year, VehicleType, FuelType, CurrentMileage, TankCapacity, AverageFuelConsumption, Status, PurchaseDate, PurchasePrice, InsuranceExpiryDate, TechnicalInspectionDate, Notes) VALUES
-- Véhicules de tourisme (Voiture)
('AB-123-CD', 'Renault', 'Clio V', 2022, 'Voiture', 'Essence', 45280.50, 42.00, 5.8, 'Actif', '2022-01-15', 18500.00, '2026-01-15', '2026-01-15', 'Véhicule de direction'),
('EF-456-GH', 'Peugeot', '308', 2021, 'Voiture', 'Diesel', 68420.75, 50.00, 4.9, 'Actif', '2021-03-20', 22000.00, '2026-03-20', '2025-12-10', 'Véhicule commercial'),
('IJ-789-KL', 'Citroën', 'C3', 2023, 'Voiture', 'Essence', 28150.00, 45.00, 6.2, 'Actif', '2023-05-10', 17800.00, '2027-05-10', '2027-05-10', NULL),
('MN-012-OP', 'Renault', 'Mégane', 2020, 'Voiture', 'Diesel', 95600.25, 50.00, 5.1, 'Actif', '2020-06-18', 24500.00, '2025-12-18', '2025-12-18', 'Maintenance fréquente'),
('QR-345-ST', 'Volkswagen', 'Golf', 2021, 'Voiture', 'Essence', 52340.00, 50.00, 5.5, 'Actif', '2021-09-25', 26000.00, '2026-09-25', '2026-03-15', NULL),
('IJ-678-KL', 'Peugeot', '208', 2019, 'Voiture', 'Essence', 105600.00, 40.00, 5.9, 'EnMaintenance', '2019-03-15', 16500.00, '2026-03-15', '2025-11-20', 'Révision importante en cours'),
('QR-234-ST', 'Renault', 'Zoe', 2023, 'Voiture', 'Electrique', 18500.00, 52.00, 0.0, 'Actif', '2023-09-01', 32000.00, '2027-09-01', '2027-09-01', 'Véhicule électrique - 0 émission'),

-- Utilitaires
('UV-678-WX', 'Renault', 'Kangoo', 2022, 'Utilitaire', 'Diesel', 72500.50, 60.00, 6.8, 'Actif', '2022-02-10', 21000.00, '2026-02-10', '2026-02-10', 'Véhicule de livraison'),
('YZ-901-AB', 'Peugeot', 'Partner', 2021, 'Utilitaire', 'Diesel', 88230.00, 55.00, 6.5, 'Actif', '2021-07-15', 19500.00, '2025-12-15', '2025-12-15', NULL),
('CD-234-EF', 'Citroën', 'Berlingo', 2023, 'Utilitaire', 'Diesel', 35800.75, 55.00, 6.3, 'Actif', '2023-03-22', 20800.00, '2027-03-22', '2027-03-22', 'Véhicule neuf'),
('GH-567-IJ', 'Fiat', 'Ducato', 2020, 'Utilitaire', 'Diesel', 112450.00, 90.00, 8.2, 'Actif', '2020-04-08', 28000.00, '2026-04-08', '2025-11-30', 'Grand volume'),
('MN-901-OP', 'Citroën', 'Jumpy', 2020, 'Utilitaire', 'Diesel', 115200.25, 80.00, 7.5, 'EnMaintenance', '2020-08-22', 26000.00, '2026-08-22', '2025-12-05', 'Changement embrayage'),
('YZ-890-AB', 'Renault', 'Trafic', 2017, 'Utilitaire', 'Diesel', 185600.00, 90.00, 8.0, 'HorsService', '2017-11-20', 24000.00, '2025-10-20', '2024-05-20', 'Réforme programmée'),

-- Camions
('WX-789-YZ', 'Mercedes', 'Sprinter', 2020, 'Camion', 'Diesel', 125800.75, 100.00, 9.5, 'Actif', '2020-05-20', 38000.00, '2026-05-20', '2025-12-01', 'Gros porteur'),
('AB-012-CD', 'Renault', 'Master', 2021, 'Camion', 'Diesel', 98700.00, 95.00, 9.2, 'Actif', '2021-10-12', 35000.00, '2026-10-12', '2026-04-10', NULL),
('EF-345-GH', 'Ford', 'Transit', 2022, 'Camion', 'Diesel', 67500.50, 80.00, 8.8, 'Actif', '2022-06-30', 33000.00, '2026-06-30', '2026-06-30', 'Aménagement spécial'),

-- Motos (ajout de nouveaux types)
('MO-111-AA', 'Honda', 'CB500X', 2022, 'Moto', 'Essence', 12500.00, 17.50, 4.2, 'Actif', '2022-04-15', 7500.00, '2026-04-15', '2026-04-15', 'Moto de liaison'),
('MO-222-BB', 'Yamaha', 'MT-07', 2023, 'Moto', 'Essence', 8300.00, 14.00, 4.5, 'Actif', '2023-02-20', 8200.00, '2027-02-20', '2027-02-20', 'Véhicule urbain'),

-- Véhicules hors service
('UV-567-WX', 'Peugeot', '3008', 2018, 'Voiture', 'Diesel', 142000.50, 53.00, 6.7, 'HorsService', '2018-02-10', 28000.00, '2025-11-10', '2024-08-15', 'Accident - En attente réparation');

-- ============================================
-- 4. TABLE VEHICLE_ASSIGNMENTS (Affectations)
-- ============================================
CREATE TABLE VehicleAssignments (
    AssignmentId INT AUTO_INCREMENT PRIMARY KEY,
    VehicleId INT NOT NULL,
    DriverId INT NOT NULL,
    AssignmentDate DATE NOT NULL,
    ReturnDate DATE,
    StartMileage DECIMAL(10,2) NOT NULL,
    EndMileage DECIMAL(10,2),
    Status VARCHAR(20) NOT NULL DEFAULT 'EnCours',
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId) ON DELETE CASCADE,
    FOREIGN KEY (DriverId) REFERENCES Drivers(DriverId) ON DELETE CASCADE
) ENGINE=InnoDB;

INSERT INTO VehicleAssignments (VehicleId, DriverId, AssignmentDate, ReturnDate, StartMileage, EndMileage, Status, Notes) VALUES
-- Affectations en cours
(1, 1, '2025-10-01', NULL, 42500.00, NULL, 'EnCours', 'Affectation longue durée'),
(2, 2, '2025-09-15', NULL, 65800.00, NULL, 'EnCours', 'Commercial terrain'),
(3, 3, '2025-11-01', NULL, 27000.00, NULL, 'EnCours', NULL),
(5, 4, '2025-10-20', NULL, 50100.00, NULL, 'EnCours', 'Déplacements région'),
(6, 5, '2025-09-01', NULL, 68000.00, NULL, 'EnCours', 'Livraisons quotidiennes'),
(10, 6, '2025-10-15', NULL, 56800.00, NULL, 'EnCours', NULL),
(12, 7, '2025-11-10', NULL, 21500.00, NULL, 'EnCours', 'Test véhicule hybride'),

-- Affectations terminées (historique)
(1, 2, '2025-08-01', '2025-09-30', 38200.00, 42500.00, 'Terminée', 'Mission temporaire'),
(2, 1, '2025-07-01', '2025-09-14', 62450.00, 65800.00, 'Terminée', NULL),
(4, 3, '2025-06-15', '2025-10-19', 89200.00, 95600.25, 'Terminée', 'Longue mission'),
(6, 4, '2025-05-01', '2025-08-31', 58900.00, 68000.00, 'Terminée', 'Saison estivale'),
(7, 5, '2025-04-10', '2025-10-10', 78500.00, 88230.00, 'Terminée', NULL),
(9, 6, '2025-03-01', '2025-08-30', 98700.00, 112450.00, 'Terminée', 'Transport lourd'),
(11, 7, '2025-02-15', '2025-09-15', 42300.00, 47650.50, 'Terminée', NULL),
(13, 8, '2025-01-10', '2025-10-31', 112800.00, 125800.75, 'Terminée', 'Gros volume');

-- ============================================
-- 5. TABLE FUEL_RECORDS (Pleins de carburant)
-- ============================================
CREATE TABLE FuelRecords (
    FuelRecordId INT AUTO_INCREMENT PRIMARY KEY,
    VehicleId INT NOT NULL,
    DriverId INT,
    RefuelDate DATETIME NOT NULL,
    Mileage DECIMAL(10,2) NOT NULL,
    LitersRefueled DECIMAL(10,2) NOT NULL,
    PricePerLiter DECIMAL(5,3) NOT NULL,
    TotalCost DECIMAL(10,2) NOT NULL,
    FuelType VARCHAR(20) NOT NULL,
    Station VARCHAR(100),
    CalculatedConsumption DECIMAL(5,2),
    IsFullTank BOOLEAN DEFAULT TRUE,
    PaymentMethod VARCHAR(20),
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId) ON DELETE CASCADE,
    FOREIGN KEY (DriverId) REFERENCES Drivers(DriverId) ON DELETE SET NULL
) ENGINE=InnoDB;

-- Générer des pleins sur les 6 derniers mois
INSERT INTO FuelRecords (VehicleId, DriverId, RefuelDate, Mileage, LitersRefueled, PricePerLiter, TotalCost, FuelType, Station, CalculatedConsumption, IsFullTank, PaymentMethod, Notes) VALUES
-- Véhicule 1 (Clio) - Essence
(1, 1, '2025-11-15 08:30:00', 45280.50, 38.5, 1.899, 73.11, 'Essence', 'Total Access - Centre', 5.8, TRUE, 'CartePro', NULL),
(1, 1, '2025-11-01 14:20:00', 44630.00, 40.2, 1.879, 75.54, 'Essence', 'Shell - Route Nationale', 5.9, TRUE, 'CartePro', NULL),
(1, 1, '2025-10-18 09:15:00', 43950.00, 39.8, 1.859, 73.99, 'Essence', 'Total Access - Centre', 5.7, TRUE, 'CartePro', NULL),
(1, 1, '2025-10-03 16:45:00', 43280.00, 38.9, 1.849, 71.93, 'Essence', 'Esso - Autoroute A1', 5.8, TRUE, 'CartePro', 'Plein autoroute'),
(1, 2, '2025-09-20 11:30:00', 42610.00, 40.5, 1.839, 74.48, 'Essence', 'Total Access - Centre', 5.9, TRUE, 'CartePro', NULL),
(1, 2, '2025-09-05 08:00:00', 41920.00, 39.1, 1.829, 71.51, 'Essence', 'Shell - Route Nationale', 5.7, TRUE, 'CartePro', NULL),

-- Véhicule 2 (308) - Diesel
(2, 2, '2025-11-16 07:45:00', 68420.75, 47.8, 1.759, 84.08, 'Diesel', 'Carrefour - Zone Commercial', 4.9, TRUE, 'CartePro', NULL),
(2, 2, '2025-10-28 15:20:00', 67450.00, 48.5, 1.749, 84.83, 'Diesel', 'Leclerc - Supermarché', 4.8, TRUE, 'CartePro', 'Prix avantageux'),
(2, 2, '2025-10-12 09:30:00', 66500.00, 47.2, 1.739, 82.08, 'Diesel', 'Total Access - Centre', 5.0, TRUE, 'CartePro', NULL),
(2, 1, '2025-09-25 14:00:00', 65550.00, 49.1, 1.729, 84.89, 'Diesel', 'Shell - Route Nationale', 4.9, TRUE, 'CartePro', NULL),
(2, 1, '2025-09-08 10:15:00', 64600.00, 48.3, 1.719, 83.03, 'Diesel', 'Esso - Centre-ville', 4.8, TRUE, 'CartePro', NULL),
(2, 2, '2025-08-22 16:30:00', 63650.00, 47.9, 1.709, 81.86, 'Diesel', 'Total Access - Centre', 5.0, TRUE, 'CartePro', NULL),

-- Véhicule 3 (C3) - Essence
(3, 3, '2025-11-14 12:00:00', 28150.00, 42.1, 1.889, 79.53, 'Essence', 'Shell - Route Nationale', 6.2, TRUE, 'CartePro', NULL),
(3, 3, '2025-10-30 09:45:00', 27480.00, 41.8, 1.869, 78.12, 'Essence', 'Total Access - Centre', 6.1, TRUE, 'CartePro', NULL),
(3, 3, '2025-10-14 14:20:00', 26820.00, 43.2, 1.849, 79.88, 'Essence', 'Esso - Centre-ville', 6.3, TRUE, 'CartePro', NULL),
(3, 3, '2025-09-28 08:30:00', 26150.00, 42.5, 1.839, 78.16, 'Essence', 'Shell - Route Nationale', 6.2, TRUE, 'CartePro', NULL),

-- Véhicule 6 (Kangoo) - Diesel
(6, 5, '2025-11-17 06:30:00', 72500.50, 57.8, 1.759, 101.67, 'Diesel', 'Station 24/7 - Zone Industrielle', 6.8, TRUE, 'CartePro', 'Plein matinal'),
(6, 5, '2025-11-09 15:45:00', 71650.00, 58.2, 1.749, 101.79, 'Diesel', 'Total Access - Centre', 6.7, TRUE, 'CartePro', NULL),
(6, 5, '2025-11-01 07:00:00', 70800.00, 57.5, 1.739, 99.99, 'Diesel', 'Shell - Route Nationale', 6.9, TRUE, 'CartePro', NULL),
(6, 5, '2025-10-23 14:30:00', 69950.00, 59.1, 1.729, 102.18, 'Diesel', 'Leclerc - Supermarché', 6.8, TRUE, 'CartePro', NULL),
(6, 5, '2025-10-14 08:15:00', 69050.00, 58.7, 1.719, 100.90, 'Diesel', 'Carrefour - Zone Commercial', 6.7, TRUE, 'CartePro', NULL),
(6, 5, '2025-10-05 16:00:00', 68200.00, 57.3, 1.709, 97.93, 'Diesel', 'Total Access - Centre', 6.9, TRUE, 'CartePro', NULL),

-- Véhicule 10 (Duster) - Diesel
(10, 6, '2025-11-12 11:20:00', 58900.25, 48.5, 1.759, 85.31, 'Diesel', 'Total Access - Centre', 6.0, TRUE, 'CartePro', NULL),
(10, 6, '2025-10-26 09:30:00', 58100.00, 49.2, 1.749, 86.05, 'Diesel', 'Shell - Route Nationale', 5.9, TRUE, 'CartePro', NULL),
(10, 6, '2025-10-09 14:15:00', 57300.00, 48.8, 1.739, 84.86, 'Diesel', 'Esso - Centre-ville', 6.1, TRUE, 'CartePro', NULL),
(10, 6, '2025-09-22 10:45:00', 56500.00, 49.5, 1.729, 85.59, 'Diesel', 'Total Access - Centre', 6.0, TRUE, 'CartePro', NULL),

-- Véhicule 12 (Captur Hybride)
(12, 7, '2025-11-16 13:30:00', 22100.00, 45.2, 1.889, 85.38, 'Hybride', 'Shell - Route Nationale', 4.5, TRUE, 'CartePro', 'Excellent rendement'),
(12, 7, '2025-10-28 10:00:00', 21100.00, 44.8, 1.869, 83.73, 'Hybride', 'Total Access - Centre', 4.4, TRUE, 'CartePro', NULL),
(12, 7, '2025-10-10 15:45:00', 20150.00, 46.1, 1.849, 85.24, 'Hybride', 'Esso - Centre-ville', 4.6, TRUE, 'CartePro', NULL),

-- Véhicule 13 (Sprinter) - Diesel
(13, 8, '2025-11-10 07:15:00', 125800.75, 95.8, 1.759, 168.51, 'Diesel', 'Station Poids Lourds - Rocade', 9.5, TRUE, 'CartePro', 'Grand réservoir'),
(13, 8, '2025-10-21 08:30:00', 124800.00, 97.2, 1.749, 170.04, 'Diesel', 'Total Relais Routier', 9.4, TRUE, 'CartePro', NULL),
(13, 8, '2025-10-02 14:45:00', 123750.00, 96.5, 1.739, 167.81, 'Diesel', 'Shell - Station Service PL', 9.6, TRUE, 'CartePro', NULL);

-- ============================================
-- 6. TABLE MAINTENANCE_RECORDS (Maintenances)
-- ============================================
CREATE TABLE MaintenanceRecords (
    MaintenanceRecordId INT AUTO_INCREMENT PRIMARY KEY,
    VehicleId INT NOT NULL,
    MaintenanceDate DATE NOT NULL,
    Mileage DECIMAL(10,2) NOT NULL,
    MaintenanceType VARCHAR(50) NOT NULL COMMENT 'Vidange, Révision, Réparation, Pneus, Freins, Autre',
    Description TEXT NOT NULL,
    Cost DECIMAL(10,2) NOT NULL,
    Garage VARCHAR(100),
    NextMaintenanceDate DATE,
    NextMaintenanceMileage DECIMAL(10,2),
    Parts TEXT,
    TechnicianName VARCHAR(100),
    Status VARCHAR(20) DEFAULT 'Terminée' COMMENT 'Planifiée, EnCours, Terminée, Annulée',
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_vehicle (VehicleId),
    INDEX idx_maintenance_date (MaintenanceDate),
    INDEX idx_status (Status),
    INDEX idx_maintenance_type (MaintenanceType),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId) ON DELETE CASCADE
) ENGINE=InnoDB;

INSERT INTO MaintenanceRecords (VehicleId, MaintenanceDate, Mileage, MaintenanceType, Description, Cost, Garage, NextMaintenanceDate, NextMaintenanceMileage, Parts, TechnicianName, Status, Notes) VALUES
-- Révisions régulières
(1, '2025-10-15', 45000.00, 'Révision', 'Révision 45000 km - Vidange, filtres, contrôle général', 285.50, 'Garage Renault Centre', '2026-04-15', 60000.00, 'Huile 5W30 5L, Filtre à huile, Filtre à air', 'Michel Dubois', 'Terminée', NULL),
(2, '2025-09-20', 65000.00, 'Révision', 'Révision 65000 km - Vidange, filtres', 320.00, 'Garage Peugeot Sud', '2026-03-20', 80000.00, 'Huile diesel 5W40, Filtres', 'Jean Martin', 'Terminée', NULL),
(3, '2025-11-05', 28000.00, 'Révision', 'Première révision - Vidange et contrôles', 195.00, 'Garage Citroën Nord', '2026-05-05', 43000.00, 'Huile 5W30, Filtre à huile', 'Pierre Leroux', 'Terminée', 'Garantie constructeur'),
(5, '2025-08-12', 50000.00, 'Révision', 'Révision 50000 km - Complète', 340.00, 'Garage Volkswagen', '2026-02-12', 65000.00, 'Huile, Filtres air/huile/carburant', 'Service VW', 'Terminée', NULL),

-- Vidanges
(4, '2025-07-20', 90000.00, 'Vidange', 'Vidange + filtres huile et air', 150.00, 'Garage Renault Centre', '2025-12-20', 105000.00, 'Huile 5W40, Filtres', 'Michel Dubois', 'Terminée', NULL),
(8, '2025-09-10', 70000.00, 'Vidange', 'Vidange diesel', 165.00, 'Garage Peugeot Sud', '2026-02-10', 85000.00, 'Huile diesel, Filtre huile', 'Jean Martin', 'Terminée', NULL),
(13, '2025-10-05', 120000.00, 'Vidange', 'Vidange poids lourd', 280.00, 'Garage PL Industriel', '2026-01-05', 135000.00, 'Huile 15W40 20L, Filtres', 'Équipe PL', 'Terminée', NULL),

-- Changements de pneus
(1, '2025-03-20', 40500.00, 'Pneus', 'Changement 4 pneus été', 480.00, 'Centre Auto Express', NULL, NULL, 'Michelin Primacy 4 195/65R15 x4', 'Service montage', 'Terminée', 'Stockage pneus hiver'),
(2, '2025-10-28', 67000.00, 'Pneus', 'Changement 4 pneus hiver', 520.00, 'Euromaster', NULL, NULL, 'Continental WinterContact 205/55R16 x4', 'Équipe montage', 'Terminée', NULL),
(6, '2025-09-15', 70000.00, 'Pneus', 'Changement 2 pneus avant', 280.00, 'First Stop', NULL, NULL, 'Michelin Agilis 195/70R15 x2', 'Monteur pneumatiques', 'Terminée', 'Usure irrégulière'),
(16, '2025-04-10', 8000.00, 'Pneus', 'Pneus moto - Changement train complet', 320.00, 'Moto Expert', NULL, NULL, 'Michelin Road 5 AV/AR', 'Technicien moto', 'Terminée', 'Pneus sport touring'),

-- Freinage
(4, '2025-08-10', 92000.00, 'Freins', 'Remplacement plaquettes et disques avant', 385.00, 'Garage Renault Centre', NULL, NULL, 'Plaquettes avant, Disques avant x2', 'Michel Dubois', 'Terminée', NULL),
(9, '2025-07-22', 85000.00, 'Freins', 'Changement plaquettes arrière', 185.00, 'Garage Peugeot Sud', NULL, NULL, 'Plaquettes arrière', 'Jean Martin', 'Terminée', NULL),
(13, '2025-06-18', 118000.00, 'Freins', 'Révision système freinage complet', 680.00, 'Garage Poids Lourds Industriel', NULL, NULL, 'Plaquettes AV/AR, Disques avant, Liquide de frein', 'Équipe mécanique PL', 'Terminée', 'Véhicule lourd'),
(7, '2025-08-05', 43000.00, 'Freins', 'Remplacement plaquettes avant', 220.00, 'Centre Auto Express', NULL, NULL, 'Plaquettes Bosch', 'Service rapide', 'Terminée', NULL),

-- Réparations diverses
(2, '2025-04-15', 63500.00, 'Réparation', 'Changement embrayage', 850.00, 'Garage Peugeot Sud', NULL, NULL, 'Kit embrayage complet', 'Jean Martin', 'Terminée', 'Embrayage usé prématurément'),
(4, '2025-05-28', 89000.00, 'Réparation', 'Remplacement alternateur', 420.00, 'Garage Renault Centre', NULL, NULL, 'Alternateur 120A', 'Michel Dubois', 'Terminée', 'Panne sur route'),
(11, '2025-07-10', 110000.00, 'Réparation', 'Remplacement courroie distribution', 680.00, 'Garage Poids Lourds Industriel', '2027-07-10', 180000.00, 'Kit distribution complet, Pompe à eau', 'Équipe mécanique PL', 'Terminée', 'Maintenance préventive'),
(9, '2025-08-15', 86500.00, 'Réparation', 'Changement batterie', 145.00, 'Centre Auto Express', NULL, NULL, 'Batterie 70Ah Bosch', 'Service rapide', 'Terminée', 'Batterie faible'),
(10, '2025-09-22', 113000.00, 'Réparation', 'Remplacement démarreur', 380.00, 'Garage Auto Service', NULL, NULL, 'Démarreur Valeo', 'Mécanicien', 'Terminée', 'Défaillance électrique'),
(18, '2025-06-05', 139000.00, 'Réparation', 'Réparation turbo', 1250.00, 'Garage Diesel Expert', NULL, NULL, 'Turbocompresseur reconditionné', 'Spécialiste diesel', 'Terminée', 'Perte de puissance'),

-- Contrôles techniques
(2, '2025-11-08', 68000.00, 'Autre', 'Contrôle technique périodique', 78.00, 'Dekra Auto Contrôle', '2027-11-08', NULL, NULL, 'Contrôleur technique', 'Terminée', 'Aucune contre-visite'),
(4, '2025-10-15', 95000.00, 'Autre', 'Contrôle technique avec contre-visite', 128.00, 'Autosur Contrôle Technique', '2027-10-15', NULL, NULL, 'Contrôleur technique', 'Terminée', 'Défaut feux - Réparé'),
(8, '2025-09-25', 71000.00, 'Autre', 'Contrôle technique', 75.00, 'Dekra Auto Contrôle', '2027-09-25', NULL, NULL, 'Contrôleur technique', 'Terminée', 'Conforme'),

-- Climatisation
(1, '2025-06-05', 42000.00, 'Autre', 'Recharge climatisation', 95.00, 'Centre Auto Express', NULL, NULL, 'Gaz R134a', 'Technicien climatisation', 'Terminée', NULL),
(5, '2025-05-20', 49000.00, 'Autre', 'Désinfection et recharge clim', 125.00, 'Garage Auto Service', NULL, NULL, 'Gaz R134a, Produit désinfectant', 'Service climatisation', 'Terminée', NULL),

-- Carrosserie
(1, '2025-02-18', 39000.00, 'Autre', 'Réparation rayure portière', 320.00, 'Carrosserie Pro', NULL, NULL, 'Peinture, Vernis', 'Carrossier', 'Terminée', 'Dommage parking'),
(7, '2025-03-25', 27000.00, 'Autre', 'Remplacement pare-brise', 280.00, 'Carglass', NULL, NULL, 'Pare-brise d\'origine', 'Technicien vitrage', 'Terminée', 'Impact gravillons - Assurance'),

-- Maintenances en cours
(6, '2025-11-22', 72000.00, 'Révision', 'Révision majeure 70000 km', 520.00, 'Garage Renault Pro', '2026-05-22', 87000.00, 'Vidange, Filtres complets, Bougies', 'Équipe entretien', 'EnCours', 'En cours de réalisation'),
(12, '2025-11-23', 114500.00, 'Réparation', 'Changement embrayage', 980.00, 'Garage Utilitaires Pro', '2026-02-23', 130000.00, 'Kit embrayage renforcé', 'Atelier mécanique', 'EnCours', 'Patinage embrayage détecté'),

-- Maintenances planifiées
(5, '2025-11-28', 53000.00, 'Révision', 'Révision 50000 km programmée', 310.00, 'Garage Volkswagen', '2026-05-28', 68000.00, 'Huile, Filtres', 'Service entretien', 'Planifiée', 'Rendez-vous confirmé le 28/11'),
(3, '2025-12-02', 29000.00, 'Pneus', 'Changement pneus hiver', 450.00, 'Euromaster', NULL, NULL, 'Pneus hiver 195/65R15 x4', 'Monteur', 'Planifiée', 'Commande en cours'),
(14, '2025-12-10', 99000.00, 'Révision', 'Révision 100000 km', 580.00, 'Garage Master Pro', '2026-06-10', 115000.00, 'Vidange, Filtres, Courroie accessoires', 'Mécanicien spécialisé', 'Planifiée', 'Grande révision prévue'),
(17, '2025-12-15', 19000.00, 'Vidange', 'Première vidange véhicule électrique', 95.00, 'Garage Renault Electric', '2026-06-15', 35000.00, 'Contrôle batterie, Liquide refroidissement', 'Technicien VE', 'Planifiée', 'Entretien spécifique VE');

-- ============================================
-- 7. INDEX pour optimiser les performances
-- ============================================
-- Note: Les index principaux sont déjà créés dans les définitions de tables

-- ============================================
-- 8. VUES pour analyses rapides
-- ============================================

-- Vue: Statistiques par véhicule
CREATE VIEW VehicleStatisticsView AS
SELECT 
    v.VehicleId,
    v.RegistrationNumber,
    v.Brand,
    v.Model,
    v.VehicleType,
    v.FuelType,
    v.CurrentMileage,
    v.Status,
    COUNT(DISTINCT fr.FuelRecordId) AS TotalRefuels,
    COALESCE(SUM(fr.LitersRefueled), 0) AS TotalLiters,
    COALESCE(SUM(fr.TotalCost), 0) AS TotalFuelCost,
    COALESCE(AVG(fr.CalculatedConsumption), 0) AS AverageConsumption,
    COUNT(DISTINCT mr.MaintenanceRecordId) AS TotalMaintenances,
    COALESCE(SUM(mr.Cost), 0) AS TotalMaintenanceCost,
    (COALESCE(SUM(fr.TotalCost), 0) + COALESCE(SUM(mr.Cost), 0)) AS TotalOperatingCost
FROM Vehicles v
LEFT JOIN FuelRecords fr ON v.VehicleId = fr.VehicleId
LEFT JOIN MaintenanceRecords mr ON v.VehicleId = mr.VehicleId
GROUP BY v.VehicleId, v.RegistrationNumber, v.Brand, v.Model, v.VehicleType, v.FuelType, v.CurrentMileage, v.Status;

-- Vue: Coûts mensuels
CREATE VIEW MonthlyCostsView AS
SELECT 
    DATE_FORMAT(fr.RefuelDate, '%Y-%m') AS YearMonth,
    YEAR(fr.RefuelDate) AS Year,
    MONTH(fr.RefuelDate) AS Month,
    MONTHNAME(fr.RefuelDate) AS MonthName,
    COUNT(DISTINCT fr.FuelRecordId) AS RefuelCount,
    SUM(fr.TotalCost) AS FuelCost,
    SUM(fr.LitersRefueled) AS TotalLiters,
    AVG(fr.PricePerLiter) AS AveragePricePerLiter,
    AVG(fr.CalculatedConsumption) AS AverageConsumption,
    COALESCE((SELECT SUM(mr.Cost) 
              FROM MaintenanceRecords mr 
              WHERE DATE_FORMAT(mr.MaintenanceDate, '%Y-%m') = DATE_FORMAT(fr.RefuelDate, '%Y-%m')), 0) AS MaintenanceCost,
    (SUM(fr.TotalCost) + COALESCE((SELECT SUM(mr.Cost) 
                                   FROM MaintenanceRecords mr 
                                   WHERE DATE_FORMAT(mr.MaintenanceDate, '%Y-%m') = DATE_FORMAT(fr.RefuelDate, '%Y-%m')), 0)) AS TotalMonthlyCost
FROM FuelRecords fr
GROUP BY YearMonth, Year, Month, MonthName
ORDER BY YearMonth DESC;

-- Vue: Alertes maintenance
CREATE VIEW MaintenanceAlertsView AS
SELECT 
    v.VehicleId,
    v.RegistrationNumber,
    v.Brand,
    v.Model,
    v.VehicleType,
    v.Status,
    v.CurrentMileage,
    mr.NextMaintenanceDate,
    mr.NextMaintenanceMileage,
    mr.MaintenanceType AS LastMaintenanceType,
    DATEDIFF(mr.NextMaintenanceDate, CURDATE()) AS DaysUntilMaintenance,
    CASE 
        WHEN mr.NextMaintenanceMileage IS NOT NULL AND v.CurrentMileage >= mr.NextMaintenanceMileage THEN 'Urgent - Kilométrage dépassé'
        WHEN mr.NextMaintenanceDate <= CURDATE() THEN 'Urgent - Date dépassée'
        WHEN DATEDIFF(mr.NextMaintenanceDate, CURDATE()) <= 15 THEN 'Très proche (< 15 jours)'
        WHEN DATEDIFF(mr.NextMaintenanceDate, CURDATE()) <= 30 THEN 'Proche (< 30 jours)'
        ELSE 'Planifiée'
    END AS AlertLevel
FROM Vehicles v
INNER JOIN MaintenanceRecords mr ON v.VehicleId = mr.VehicleId
WHERE mr.NextMaintenanceDate IS NOT NULL
  AND mr.Status = 'Terminée'
  AND mr.MaintenanceRecordId = (
      SELECT MAX(MaintenanceRecordId) 
      FROM MaintenanceRecords 
      WHERE VehicleId = v.VehicleId
        AND NextMaintenanceDate IS NOT NULL
  )
ORDER BY 
    CASE 
        WHEN mr.NextMaintenanceMileage IS NOT NULL AND v.CurrentMileage >= mr.NextMaintenanceMileage THEN 1
        WHEN mr.NextMaintenanceDate <= CURDATE() THEN 2
        WHEN DATEDIFF(mr.NextMaintenanceDate, CURDATE()) <= 15 THEN 3
        WHEN DATEDIFF(mr.NextMaintenanceDate, CURDATE()) <= 30 THEN 4
        ELSE 5
    END;

-- Vue: Historique des conducteurs
CREATE VIEW DriverAssignmentsView AS
SELECT 
    d.DriverId,
    d.FirstName,
    d.LastName,
    d.LicenseNumber,
    d.Status AS DriverStatus,
    v.VehicleId,
    v.RegistrationNumber,
    v.Brand,
    v.Model,
    va.AssignmentDate,
    va.ReturnDate,
    va.Status AS AssignmentStatus,
    va.StartMileage,
    va.EndMileage,
    (va.EndMileage - va.StartMileage) AS MileageDriven,
    DATEDIFF(COALESCE(va.ReturnDate, CURDATE()), va.AssignmentDate) AS DaysAssigned
FROM Drivers d
LEFT JOIN VehicleAssignments va ON d.DriverId = va.DriverId
LEFT JOIN Vehicles v ON va.VehicleId = v.VehicleId
ORDER BY va.AssignmentDate DESC;

-- Vue: Véhicules nécessitant une attention
CREATE VIEW VehicleAttentionView AS
SELECT 
    v.VehicleId,
    v.RegistrationNumber,
    v.Brand,
    v.Model,
    v.VehicleType,
    v.Status,
    v.CurrentMileage,
    v.InsuranceExpiryDate,
    v.TechnicalInspectionDate,
    CASE 
        WHEN v.Status = 'HorsService' THEN 'Véhicule hors service'
        WHEN v.Status = 'EnMaintenance' THEN 'En maintenance'
        WHEN v.InsuranceExpiryDate <= CURDATE() THEN 'Assurance expirée'
        WHEN v.InsuranceExpiryDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY) THEN 'Assurance expire bientôt'
        WHEN v.TechnicalInspectionDate <= CURDATE() THEN 'Contrôle technique expiré'
        WHEN v.TechnicalInspectionDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY) THEN 'Contrôle technique proche'
        ELSE 'Autre alerte'
    END AS AlertType,
    CASE 
        WHEN v.Status IN ('HorsService', 'EnMaintenance') THEN 1
        WHEN v.InsuranceExpiryDate <= CURDATE() OR v.TechnicalInspectionDate <= CURDATE() THEN 2
        WHEN v.InsuranceExpiryDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY) 
             OR v.TechnicalInspectionDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY) THEN 3
        ELSE 4
    END AS Priority
FROM Vehicles v
WHERE v.Status IN ('HorsService', 'EnMaintenance')
   OR v.InsuranceExpiryDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY)
   OR v.TechnicalInspectionDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY)
ORDER BY Priority, v.RegistrationNumber;

-- ============================================
-- 9. PROCÉDURES STOCKÉES utiles
-- ============================================

DELIMITER //

-- Procédure: Mise à jour du kilométrage véhicule
CREATE PROCEDURE UpdateVehicleMileage(
    IN p_VehicleId INT,
    IN p_NewMileage DECIMAL(10,2)
)
BEGIN
    DECLARE v_CurrentMileage DECIMAL(10,2);
    
    SELECT CurrentMileage INTO v_CurrentMileage
    FROM Vehicles 
    WHERE VehicleId = p_VehicleId;
    
    IF p_NewMileage >= v_CurrentMileage THEN
        UPDATE Vehicles 
        SET CurrentMileage = p_NewMileage 
        WHERE VehicleId = p_VehicleId;
        
        SELECT 'Kilométrage mis à jour avec succès' AS Message, p_NewMileage AS NewMileage;
    ELSE
        SELECT 'Erreur: Le nouveau kilométrage ne peut pas être inférieur au kilométrage actuel' AS Message;
    END IF;
END //

-- Procédure: Calcul consommation automatique
CREATE PROCEDURE CalculateFuelConsumption(
    IN p_FuelRecordId INT
)
BEGIN
    DECLARE v_VehicleId INT;
    DECLARE v_CurrentMileage DECIMAL(10,2);
    DECLARE v_PreviousMileage DECIMAL(10,2);
    DECLARE v_LitersRefueled DECIMAL(10,2);
    DECLARE v_Consumption DECIMAL(5,2);
    
    -- Récupérer les données du plein actuel
    SELECT VehicleId, Mileage, LitersRefueled 
    INTO v_VehicleId, v_CurrentMileage, v_LitersRefueled
    FROM FuelRecords 
    WHERE FuelRecordId = p_FuelRecordId;
    
    -- Trouver le plein précédent
    SELECT Mileage INTO v_PreviousMileage
    FROM FuelRecords
    WHERE VehicleId = v_VehicleId 
      AND FuelRecordId < p_FuelRecordId
      AND IsFullTank = TRUE
    ORDER BY FuelRecordId DESC
    LIMIT 1;
    
    -- Calculer la consommation (L/100km)
    IF v_PreviousMileage IS NOT NULL AND v_CurrentMileage > v_PreviousMileage THEN
        SET v_Consumption = (v_LitersRefueled / (v_CurrentMileage - v_PreviousMileage)) * 100;
        
        UPDATE FuelRecords 
        SET CalculatedConsumption = v_Consumption 
        WHERE FuelRecordId = p_FuelRecordId;
        
        -- Mettre à jour la consommation moyenne du véhicule
        UPDATE Vehicles v
        SET AverageFuelConsumption = (
            SELECT AVG(CalculatedConsumption)
            FROM FuelRecords
            WHERE VehicleId = v_VehicleId
              AND CalculatedConsumption IS NOT NULL
              AND CalculatedConsumption > 0
        )
        WHERE VehicleId = v_VehicleId;
        
        SELECT 'Consommation calculée avec succès' AS Message, v_Consumption AS Consumption;
    ELSE
        SELECT 'Impossible de calculer la consommation - Pas de plein précédent ou kilométrage invalide' AS Message;
    END IF;
END //

-- Procédure: Obtenir statistiques flotte
CREATE PROCEDURE GetFleetStatistics()
BEGIN
    SELECT 
        'Statistiques générales' AS Category,
        COUNT(*) AS TotalVehicles,
        SUM(CASE WHEN Status = 'Actif' THEN 1 ELSE 0 END) AS ActiveVehicles,
        SUM(CASE WHEN Status = 'EnMaintenance' THEN 1 ELSE 0 END) AS VehiclesInMaintenance,
        SUM(CASE WHEN Status = 'HorsService' THEN 1 ELSE 0 END) AS OutOfServiceVehicles,
        ROUND(SUM(CurrentMileage), 2) AS TotalMileage,
        ROUND(AVG(CurrentMileage), 2) AS AverageMileage,
        ROUND((SELECT SUM(TotalCost) FROM FuelRecords), 2) AS TotalFuelCost,
        ROUND((SELECT SUM(Cost) FROM MaintenanceRecords WHERE Status = 'Terminée'), 2) AS TotalMaintenanceCost,
        ROUND((SELECT SUM(TotalCost) FROM FuelRecords) + 
              (SELECT SUM(Cost) FROM MaintenanceRecords WHERE Status = 'Terminée'), 2) AS TotalOperatingCost
    FROM Vehicles;
    
    -- Statistiques par type de véhicule
    SELECT 
        'Par type de véhicule' AS Category,
        VehicleType,
        COUNT(*) AS Count,
        ROUND(AVG(CurrentMileage), 2) AS AvgMileage,
        ROUND(AVG(AverageFuelConsumption), 2) AS AvgConsumption
    FROM Vehicles
    WHERE Status = 'Actif'
    GROUP BY VehicleType
    ORDER BY Count DESC;
    
    -- Statistiques par type de carburant
    SELECT 
        'Par type de carburant' AS Category,
        FuelType,
        COUNT(*) AS Count,
        ROUND(AVG(AverageFuelConsumption), 2) AS AvgConsumption
    FROM Vehicles
    WHERE Status = 'Actif'
    GROUP BY FuelType
    ORDER BY Count DESC;
END //

-- Procédure: Vérifier les véhicules nécessitant une maintenance
CREATE PROCEDURE CheckMaintenanceNeeded()
BEGIN
    SELECT 
        v.VehicleId,
        v.RegistrationNumber,
        v.Brand,
        v.Model,
        v.CurrentMileage,
        mr.NextMaintenanceDate,
        mr.NextMaintenanceMileage,
        mr.MaintenanceType,
        CASE 
            WHEN mr.NextMaintenanceMileage IS NOT NULL 
                 AND v.CurrentMileage >= mr.NextMaintenanceMileage THEN 'Kilométrage atteint'
            WHEN mr.NextMaintenanceDate <= CURDATE() THEN 'Date dépassée'
            WHEN mr.NextMaintenanceDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY) THEN 'Dans moins de 30 jours'
            ELSE 'À venir'
        END AS MaintenanceStatus,
        DATEDIFF(mr.NextMaintenanceDate, CURDATE()) AS DaysRemaining,
        (mr.NextMaintenanceMileage - v.CurrentMileage) AS KmRemaining
    FROM Vehicles v
    INNER JOIN (
        SELECT VehicleId, MAX(MaintenanceRecordId) AS LastMaintenanceId
        FROM MaintenanceRecords
        WHERE Status = 'Terminée'
          AND NextMaintenanceDate IS NOT NULL
        GROUP BY VehicleId
    ) lm ON v.VehicleId = lm.VehicleId
    INNER JOIN MaintenanceRecords mr ON lm.LastMaintenanceId = mr.MaintenanceRecordId
    WHERE v.Status = 'Actif'
      AND (mr.NextMaintenanceDate <= DATE_ADD(CURDATE(), INTERVAL 30 DAY)
           OR (mr.NextMaintenanceMileage IS NOT NULL AND v.CurrentMileage >= mr.NextMaintenanceMileage - 1000))
    ORDER BY 
        CASE 
            WHEN mr.NextMaintenanceMileage IS NOT NULL 
                 AND v.CurrentMileage >= mr.NextMaintenanceMileage THEN 1
            WHEN mr.NextMaintenanceDate <= CURDATE() THEN 2
            ELSE 3
        END,
        mr.NextMaintenanceDate;
END //

-- Procédure: Rapport mensuel des coûts
CREATE PROCEDURE GetMonthlyCostReport(
    IN p_Year INT,
    IN p_Month INT
)
BEGIN
    DECLARE v_StartDate DATE;
    DECLARE v_EndDate DATE;
    
    SET v_StartDate = STR_TO_DATE(CONCAT(p_Year, '-', p_Month, '-01'), '%Y-%m-%d');
    SET v_EndDate = LAST_DAY(v_StartDate);
    
    -- Coûts de carburant
    SELECT 
        'Carburant' AS CostType,
        COUNT(*) AS TransactionCount,
        ROUND(SUM(TotalCost), 2) AS TotalCost,
        ROUND(AVG(TotalCost), 2) AS AverageCost,
        ROUND(SUM(LitersRefueled), 2) AS TotalLiters,
        ROUND(AVG(PricePerLiter), 3) AS AvgPricePerLiter
    FROM FuelRecords
    WHERE DATE(RefuelDate) BETWEEN v_StartDate AND v_EndDate;
    
    -- Coûts de maintenance
    SELECT 
        'Maintenance' AS CostType,
        MaintenanceType,
        COUNT(*) AS Count,
        ROUND(SUM(Cost), 2) AS TotalCost,
        ROUND(AVG(Cost), 2) AS AverageCost
    FROM MaintenanceRecords
    WHERE MaintenanceDate BETWEEN v_StartDate AND v_EndDate
      AND Status = 'Terminée'
    GROUP BY MaintenanceType
    ORDER BY TotalCost DESC;
    
    -- Résumé total
    SELECT 
        'Total mensuel' AS Summary,
        ROUND((SELECT COALESCE(SUM(TotalCost), 0) FROM FuelRecords 
               WHERE DATE(RefuelDate) BETWEEN v_StartDate AND v_EndDate), 2) AS FuelCost,
        ROUND((SELECT COALESCE(SUM(Cost), 0) FROM MaintenanceRecords 
               WHERE MaintenanceDate BETWEEN v_StartDate AND v_EndDate 
               AND Status = 'Terminée'), 2) AS MaintenanceCost,
        ROUND((SELECT COALESCE(SUM(TotalCost), 0) FROM FuelRecords 
               WHERE DATE(RefuelDate) BETWEEN v_StartDate AND v_EndDate) +
              (SELECT COALESCE(SUM(Cost), 0) FROM MaintenanceRecords 
               WHERE MaintenanceDate BETWEEN v_StartDate AND v_EndDate 
               AND Status = 'Terminée'), 2) AS TotalCost;
END //

DELIMITER ;

-- ============================================
-- 10. TRIGGERS pour automatisation
-- ============================================

DELIMITER //

-- Trigger: Mise à jour automatique du kilométrage lors d'un plein
CREATE TRIGGER UpdateMileageAfterRefuel
AFTER INSERT ON FuelRecords
FOR EACH ROW
BEGIN
    UPDATE Vehicles 
    SET CurrentMileage = NEW.Mileage
    WHERE VehicleId = NEW.VehicleId
      AND CurrentMileage < NEW.Mileage;
END //

-- Trigger: Mise à jour automatique du kilométrage lors d'une maintenance
CREATE TRIGGER UpdateMileageAfterMaintenance
AFTER INSERT ON MaintenanceRecords
FOR EACH ROW
BEGIN
    UPDATE Vehicles 
    SET CurrentMileage = NEW.Mileage
    WHERE VehicleId = NEW.VehicleId
      AND CurrentMileage < NEW.Mileage;
END //

-- Trigger: Calcul automatique de la consommation moyenne
CREATE TRIGGER UpdateAverageConsumption
AFTER INSERT ON FuelRecords
FOR EACH ROW
BEGIN
    UPDATE Vehicles v
    SET AverageFuelConsumption = (
        SELECT AVG(CalculatedConsumption)
        FROM FuelRecords
        WHERE VehicleId = NEW.VehicleId
          AND CalculatedConsumption IS NOT NULL
          AND CalculatedConsumption > 0
    )
    WHERE VehicleId = NEW.VehicleId;
END //

-- Trigger: Mise à jour du statut véhicule lors d'une affectation
CREATE TRIGGER UpdateVehicleStatusOnAssignment
AFTER INSERT ON VehicleAssignments
FOR EACH ROW
BEGIN
    IF NEW.Status = 'EnCours' THEN
        UPDATE Vehicles 
        SET Status = 'Actif'
        WHERE VehicleId = NEW.VehicleId
          AND Status NOT IN ('HorsService', 'EnMaintenance');
    END IF;
END //

DELIMITER ;

-- ============================================
-- 11. Informations de connexion et commandes utiles
-- ============================================

-- Serveur: localhost ou 127.0.0.1
-- Port: 3306 (par défaut)
-- Base de données: fleet_manager
-- Utilisateur suggéré: fleet_user
-- Mot de passe suggéré: Fleet2025!Secure

-- Pour créer l'utilisateur et définir les permissions:
-- CREATE USER 'fleet_user'@'localhost' IDENTIFIED BY 'Fleet2025!Secure';
-- GRANT ALL PRIVILEGES ON fleet_manager.* TO 'fleet_user'@'localhost';
-- FLUSH PRIVILEGES;

-- ============================================
-- 12. EXEMPLES DE REQUÊTES UTILES
-- ============================================

-- Exemple 1: Utiliser une vue pour les statistiques
-- SELECT * FROM VehicleStatisticsView WHERE Status = 'Actif';

-- Exemple 2: Appeler une procédure stockée
-- CALL GetFleetStatistics();
-- CALL CheckMaintenanceNeeded();
-- CALL GetMonthlyCostReport(2025, 11);

-- Exemple 3: Requête avec jointure
-- SELECT v.RegistrationNumber, v.Brand, v.Model, d.FirstName, d.LastName, va.AssignmentDate
-- FROM Vehicles v
-- INNER JOIN VehicleAssignments va ON v.VehicleId = va.VehicleId
-- INNER JOIN Drivers d ON va.DriverId = d.DriverId
-- WHERE va.Status = 'EnCours';

-- Exemple 4: Statistiques de consommation par type de carburant
-- SELECT 
--     FuelType,
--     COUNT(*) as NbVehicles,
--     ROUND(AVG(AverageFuelConsumption), 2) as ConsommationMoyenne,
--     ROUND(SUM(CurrentMileage), 2) as KilometrageTotal
-- FROM Vehicles
-- WHERE Status = 'Actif'
-- GROUP BY FuelType
-- ORDER BY ConsommationMoyenne;

-- ============================================
-- FIN DU SCRIPT
-- ============================================

SELECT 'Base de données Fleet Manager créée avec succès!' AS Message;
SELECT '================================================' AS Separator;
SELECT 'Résumé de la création:' AS Summary;
SELECT COUNT(*) AS TotalVehicles FROM Vehicles;
SELECT COUNT(*) AS TotalDrivers FROM Drivers;
SELECT COUNT(*) AS TotalUsers FROM Users;
SELECT COUNT(*) AS TotalFuelRecords FROM FuelRecords;
SELECT COUNT(*) AS TotalMaintenanceRecords FROM MaintenanceRecords;
SELECT COUNT(*) AS TotalAssignments FROM VehicleAssignments;
SELECT '================================================' AS Separator;
SELECT 'Vues créées: VehicleStatisticsView, MonthlyCostsView, MaintenanceAlertsView, DriverAssignmentsView, VehicleAttentionView' AS Views;
SELECT 'Procédures: GetFleetStatistics, CheckMaintenanceNeeded, GetMonthlyCostReport, UpdateVehicleMileage, CalculateFuelConsumption' AS Procedures;
SELECT '================================================' AS Separator;
SELECT 'Utilisateurs de test (mot de passe: Admin123!):' AS TestUsers;
SELECT Username, Role, IsActive FROM Users ORDER BY 
    CASE Role 
        WHEN 'SuperAdmin' THEN 1 
        WHEN 'Admin' THEN 2 
        ELSE 3 
    END;
