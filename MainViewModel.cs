using System;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Services;
using FleetManager.Views;
using FleetManager.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FleetManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AuthenticationService _authService;
        private readonly IServiceProvider _serviceProvider;
        private object? _currentView;
        private string _currentUserName = "Utilisateur";
        private string _currentUserRole = "User";
        private bool _isAdmin;
        private bool _isSuperAdmin;

        public event PropertyChangedEventHandler? PropertyChanged;

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                _currentUserName = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                _currentUserRole = value;
                OnPropertyChanged();
            }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                _isAdmin = value;
                OnPropertyChanged();
            }
        }

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set
            {
                _isSuperAdmin = value;
                OnPropertyChanged();
            }
        }

        // Commandes de navigation
        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiclesCommand { get; }
        public ICommand NavigateToFuelCommand { get; }
        public ICommand NavigateToStatisticsCommand { get; }
        public ICommand NavigateToMaintenanceCommand { get; }
        public ICommand NavigateToUsersCommand { get; }

        public MainViewModel(AuthenticationService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;

            // Initialisation des commandes
            NavigateCommand = new RelayCommand(param => Navigate(param as string));
            LogoutCommand = new RelayCommand(param => Logout(param as Window));
            NavigateToDashboardCommand = new RelayCommand(param => Navigate("DashboardView"));
            NavigateToVehiclesCommand = new RelayCommand(param => Navigate("VehiclesView"));
            NavigateToFuelCommand = new RelayCommand(param => Navigate("FuelView"));
            NavigateToStatisticsCommand = new RelayCommand(param => Navigate("StatisticsView"));
            NavigateToMaintenanceCommand = new RelayCommand(param => Navigate("MaintenanceView"));
            NavigateToUsersCommand = new RelayCommand(param => Navigate("UsersView"));

            // Initialisation des informations utilisateur
            InitializeUserInfo();

            // Vue par défaut
            Navigate("DashboardView");
        }

        private void InitializeUserInfo()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                if (currentUser != null)
                {
                    CurrentUserName = currentUser.FullName ?? currentUser.Username;
                    CurrentUserRole = GetRoleDisplayName(currentUser.Role);
                    IsAdmin = currentUser.Role == "Admin" || currentUser.Role == "SuperAdmin";
                    IsSuperAdmin = currentUser.Role == "SuperAdmin";

                    System.Diagnostics.Debug.WriteLine($"Utilisateur connecté: {CurrentUserName} ({CurrentUserRole}) - Admin: {IsAdmin} - SuperAdmin: {IsSuperAdmin}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ATTENTION: Aucun utilisateur connecté trouvé dans MainViewModel");
                    CurrentUserName = "Utilisateur inconnu";
                    CurrentUserRole = "Non connecté";
                    IsAdmin = false;
                    IsSuperAdmin = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation des infos utilisateur: {ex.Message}");
            }
        }

        private string GetRoleDisplayName(string role)
        {
            return role switch
            {
                "SuperAdmin" => "Super Administrateur",
                "Admin" => "Administrateur",
                "User" => "Utilisateur",
                _ => role
            };
        }

        private void Navigate(string? viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                System.Diagnostics.Debug.WriteLine("Nom de vue null ou vide");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== NAVIGATION VERS: {viewName} ===");
                System.Diagnostics.Debug.WriteLine($"ServiceProvider null? {_serviceProvider == null}");

                switch (viewName)
                {
                    case "DashboardView":
                        System.Diagnostics.Debug.WriteLine("Récupération DashboardView...");
                        var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                        var dashboardView = new DashboardView(dashboardViewModel);
                        System.Diagnostics.Debug.WriteLine($"DashboardView DataContext: {dashboardView.DataContext?.GetType().Name}");
                        CurrentView = dashboardView;
                        System.Diagnostics.Debug.WriteLine("DashboardView chargée et assignée");
                        break;

                    case "VehiclesView":
                        System.Diagnostics.Debug.WriteLine("Récupération VehiclesView...");
                        var vehiclesView = _serviceProvider.GetRequiredService<VehiclesView>();
                        System.Diagnostics.Debug.WriteLine($"VehiclesView récupérée: {vehiclesView != null}");
                        System.Diagnostics.Debug.WriteLine($"VehiclesView DataContext: {vehiclesView?.DataContext?.GetType().Name}");
                        CurrentView = vehiclesView;
                        System.Diagnostics.Debug.WriteLine("VehiclesView chargée et assignée");
                        break;

                    case "FuelView":
                        System.Diagnostics.Debug.WriteLine("Récupération FuelView...");
                        var fuelViewModel = _serviceProvider.GetRequiredService<FuelViewModel>();
                        var fuelView = new FuelView(fuelViewModel);
                        System.Diagnostics.Debug.WriteLine($"FuelView DataContext: {fuelView.DataContext?.GetType().Name}");
                        CurrentView = fuelView;
                        System.Diagnostics.Debug.WriteLine("FuelView chargée et assignée");
                        break;

                    case "StatisticsView":
                        var statisticsViewModel = _serviceProvider.GetRequiredService<StatisticsViewModel>();
                        var statisticsView = new StatisticsView(statisticsViewModel);
                        System.Diagnostics.Debug.WriteLine($"StatisticsView DataContext: {statisticsView.DataContext?.GetType().Name}");
                        CurrentView = statisticsView;
                        System.Diagnostics.Debug.WriteLine("StatisticsView chargée et assignée");
                        break;

                    case "MaintenanceView":
                        System.Diagnostics.Debug.WriteLine("Récupération MaintenanceView...");
                        var maintenanceViewModel = _serviceProvider.GetRequiredService<MaintenanceViewModel>();
                        var maintenanceView = new MaintenanceView(maintenanceViewModel);
                        System.Diagnostics.Debug.WriteLine($"MaintenanceView DataContext: {maintenanceView.DataContext?.GetType().Name}");
                        CurrentView = maintenanceView;
                        System.Diagnostics.Debug.WriteLine("MaintenanceView chargée et assignée");
                        break;

                    case "UsersView":
                        if (IsAdmin)
                        {
                            System.Diagnostics.Debug.WriteLine("Récupération UsersView...");
                            var usersViewModel = _serviceProvider.GetRequiredService<UsersViewModel>();
                            var usersView = new UsersView(usersViewModel);
                            System.Diagnostics.Debug.WriteLine($"UsersView DataContext: {usersView.DataContext?.GetType().Name}");
                            CurrentView = usersView;
                            System.Diagnostics.Debug.WriteLine("UsersView chargée et assignée");
                        }
                        else
                        {
                            MessageBox.Show("Accès refusé. Seuls les administrateurs peuvent accéder à cette section.",
                                "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Vue inconnue: {viewName}");
                        MessageBox.Show($"Vue non implémentée: {viewName}", "Information",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la navigation vers {viewName}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                MessageBox.Show($"Erreur lors du chargement de la vue {viewName}:\n\n{ex.Message}",
                    "Erreur de navigation", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback vers le dashboard en cas d'erreur
                if (viewName != "DashboardView")
                {
                    Navigate("DashboardView");
                }
            }
        }

        private object CreateTemporaryStatisticsView()
        {
            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = "📊 Module Statistiques\n\nCette section sera bientôt disponible.\nElle contiendra les graphiques et rapports de performance du parc automobile.",
                FontSize = 16,
                TextAlignment = System.Windows.TextAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };
            return textBlock;
        }

        private object CreateTemporaryMaintenanceView()
        {
            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = "🔧 Module Entretien\n\nCette section sera bientôt disponible.\nElle permettra de gérer les maintenances et réparations des véhicules.",
                FontSize = 16,
                TextAlignment = System.Windows.TextAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };
            return textBlock;
        }

        private object CreateTemporaryUsersView()
        {
            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = "👥 Gestion des Utilisateurs\n\nCette section sera bientôt disponible.\nElle permettra de gérer les comptes utilisateurs et leurs permissions.",
                FontSize = 16,
                TextAlignment = System.Windows.TextAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };
            return textBlock;
        }

        private void Logout(Window? window)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Déconnexion demandée");

                var result = MessageBox.Show("Êtes-vous sûr de vouloir vous déconnecter ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _authService.Logout();
                    System.Diagnostics.Debug.WriteLine("Utilisateur déconnecté");

                    var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                    loginWindow.Show();
                    System.Diagnostics.Debug.WriteLine("LoginWindow affichée");

                    if (window != null)
                    {
                        window.Close();
                        System.Diagnostics.Debug.WriteLine("MainWindow fermée");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la déconnexion: {ex.Message}");
                MessageBox.Show($"Erreur lors de la déconnexion: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
