using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Models;
using FleetManager.Services;

namespace FleetManager.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;

        // Collections
        private ObservableCollection<User> _users = new();
        private ObservableCollection<string> _roles = new();
        private ObservableCollection<string> _statusList = new();

        // Sélections et filtres
        private User? _selectedUser;
        private string _selectedRole = "Tous les rôles";
        private string _selectedStatus = "Tous";
        private string _searchText = string.Empty;

        // Statistiques
        private int _totalUsers;
        private int _activeUsers;
        private int _adminUsers;

        // État
        private bool _isLoading;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public ObservableCollection<string> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public ObservableCollection<string> StatusList
        {
            get => _statusList;
            set => SetProperty(ref _statusList, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public int TotalUsers
        {
            get => _totalUsers;
            set => SetProperty(ref _totalUsers, value);
        }

        public int ActiveUsers
        {
            get => _activeUsers;
            set => SetProperty(ref _activeUsers, value);
        }

        public int AdminUsers
        {
            get => _adminUsers;
            set => SetProperty(ref _adminUsers, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commandes
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ViewUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        public UsersViewModel(AuthenticationService authService)
        {
            _authService = authService;

            // Initialiser les commandes
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            EditUserCommand = new RelayCommand<User>(EditUser);
            DeleteUserCommand = new AsyncRelayCommand<User>(DeleteUserAsync);
            ViewUserCommand = new RelayCommand<User>(ViewUser);
            ResetPasswordCommand = new AsyncRelayCommand<User>(ResetPasswordAsync);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            ResetFiltersCommand = new AsyncRelayCommand(ResetFiltersAsync);

            // Initialiser les listes
            InitializeFilters();

            // Charger les données
            _ = LoadDataAsync(null);
        }

        private void InitializeFilters()
        {
            Roles = new ObservableCollection<string>
            {
                "Tous les rôles",
                "SuperAdmin",
                "Admin",
                "User"
            };

            StatusList = new ObservableCollection<string>
            {
                "Tous",
                "Actif",
                "Inactif"
            };
        }

        private async Task LoadDataAsync(object? parameter)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("=== USERS: Début chargement données ===");

                // Charger les utilisateurs depuis le service
                var allUsers = await _authService.GetAllUsersAsync();
                
                // Appliquer les filtres
                await ApplyFiltersAsync(allUsers);

                // Calculer les statistiques
                TotalUsers = allUsers.Count();
                ActiveUsers = allUsers.Count(u => u.IsActive);
                AdminUsers = allUsers.Count(u => u.Role == "Admin" || u.Role == "SuperAdmin");

                System.Diagnostics.Debug.WriteLine($"Utilisateurs chargés: {TotalUsers}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERREUR USERS: {ex.Message}");
                MessageBox.Show($"Erreur de chargement:\n\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplyFiltersAsync()
        {
            try
            {
                var allUsers = await _authService.GetAllUsersAsync();
                await ApplyFiltersAsync(allUsers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur filtrage: {ex.Message}");
            }
        }

        private Task ApplyFiltersAsync(IEnumerable<User> allUsers)
        {
            var filtered = allUsers.AsEnumerable();

            // Filtre par rôle
            if (SelectedRole != "Tous les rôles")
            {
                filtered = filtered.Where(u => u.Role == SelectedRole);
            }

            // Filtre par statut
            if (SelectedStatus == "Actif")
            {
                filtered = filtered.Where(u => u.IsActive);
            }
            else if (SelectedStatus == "Inactif")
            {
                filtered = filtered.Where(u => !u.IsActive);
            }

            // Filtre par recherche
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(u => 
                    u.Username.ToLower().Contains(search) ||
                    u.FullName.ToLower().Contains(search) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)));
            }

            Users = new ObservableCollection<User>(filtered.OrderBy(u => u.Username));
            return Task.CompletedTask;
        }

        private async Task AddUserAsync(object? parameter)
        {
            try
            {
                var window = new Views.AddEditUserWindow(_authService, null);
                if (window.ShowDialog() == true)
                {
                    await LoadDataAsync(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n\n{ex.Message}", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser(User? user)
        {
            if (user == null) return;

            try
            {
                var window = new Views.AddEditUserWindow(_authService, user);
                if (window.ShowDialog() == true)
                {
                    _ = LoadDataAsync(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n\n{ex.Message}", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteUserAsync(User? user)
        {
            if (user == null) return;

            // Ne pas permettre la suppression de l'utilisateur connecté
            if (user.UserId == _authService.CurrentUser?.UserId)
            {
                MessageBox.Show("Vous ne pouvez pas supprimer votre propre compte.", 
                    "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Protection SuperAdmin : ne peut jamais être supprimé
            if (user.Role == "SuperAdmin")
            {
                MessageBox.Show("Un Super Administrateur ne peut pas être supprimé.", 
                    "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Seul un SuperAdmin peut supprimer un Admin
            if (user.Role == "Admin" && _authService.CurrentUser?.Role != "SuperAdmin")
            {
                MessageBox.Show("Seul un Super Administrateur peut supprimer un administrateur.", 
                    "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'utilisateur '{user.Username}' ?\n\nCette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _authService.DeleteUserAsync(user.UserId);
                    if (success)
                    {
                        MessageBox.Show("Utilisateur supprimé avec succès.", "Succès", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync(null);
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la suppression de l'utilisateur.", 
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur:\n\n{ex.Message}", "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewUser(User? user)
        {
            if (user == null) return;

            var details = $"Nom d'utilisateur: {user.Username}\n" +
                         $"Nom complet: {user.FullName}\n" +
                         $"Email: {user.Email ?? "Non renseigné"}\n" +
                         $"Rôle: {user.Role}\n" +
                         $"Statut: {(user.IsActive ? "Actif" : "Inactif")}\n" +
                         $"Date de création: {user.CreatedAt:dd/MM/yyyy HH:mm}\n" +
                         $"Dernière connexion: {(user.LastLogin.HasValue ? user.LastLogin.Value.ToString("dd/MM/yyyy HH:mm") : "Jamais")}";

            MessageBox.Show(details, $"Détails de {user.Username}", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task ResetPasswordAsync(User? user)
        {
            if (user == null) return;

            var result = MessageBox.Show(
                $"Réinitialiser le mot de passe de '{user.Username}' ?\n\nLe nouveau mot de passe sera: password123",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _authService.ResetPasswordAsync(user.UserId, "password123");
                    if (success)
                    {
                        MessageBox.Show($"Mot de passe réinitialisé avec succès.\n\nNouveau mot de passe: password123\n\nL'utilisateur devra le changer à la prochaine connexion.", 
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la réinitialisation du mot de passe.", 
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur:\n\n{ex.Message}", "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ResetFiltersAsync(object? parameter)
        {
            SelectedRole = "Tous les rôles";
            SelectedStatus = "Tous";
            SearchText = string.Empty;
            await LoadDataAsync(null);
        }
    }
}
