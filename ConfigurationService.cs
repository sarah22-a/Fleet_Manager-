using System;
using System.Collections.Generic;

namespace FleetManager.Services
{
    /// <summary>
    /// Interface pour le service de configuration de l'application
    /// </summary>
    public interface IConfigurationService
    {
        T GetSetting<T>(string key, T defaultValue);
        void SetSetting<T>(string key, T value);
        Dictionary<string, object> GetAllSettings();
    }

    /// <summary>
    /// Service de configuration de l'application
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private Dictionary<string, object> _settings = new();

        public ConfigurationService()
        {
            LoadDefaultSettings();
        }

        /// <summary>
        /// Obtenir une valeur de configuration
        /// </summary>
        public T GetSetting<T>(string key, T defaultValue)
        {
            try
            {
                if (_settings.TryGetValue(key, out var value))
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                }
            }
            catch { }

            return defaultValue;
        }

        /// <summary>
        /// Définir une valeur de configuration
        /// </summary>
        public void SetSetting<T>(string key, T value)
        {
            _settings[key] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Obtenir tous les paramètres
        /// </summary>
        public Dictionary<string, object> GetAllSettings()
        {
            return new Dictionary<string, object>(_settings);
        }

        /// <summary>
        /// Charger les paramètres par défaut
        /// </summary>
        private void LoadDefaultSettings()
        {
            // Paramètres dashboard
            _settings["Dashboard:RefreshInterval"] = 300000; // 5 minutes en ms
            _settings["Dashboard:ShowAlerts"] = true;
            _settings["Dashboard:MaxRecentMovements"] = 10;

            // Paramètres statistiques
            _settings["Statistics:DefaultPeriod"] = "Année";
            _settings["Statistics:TrendDays"] = 90;
            _settings["Statistics:TopVehiclesCount"] = 5;

            // Paramètres export
            _settings["Export:DefaultFormat"] = "PDF";
            _settings["Export:IncludeGraphs"] = true;
            _settings["Export:CompanyName"] = "Fleet Manager";

            // Paramètres email
            _settings["Email:Enabled"] = false;
            _settings["Email:FromAddress"] = "noreply@fleetmanager.com";
        }
    }
}
