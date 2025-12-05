using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FleetManager.Helpers;
using FleetManager.Services;
using FleetManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FleetManager.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel(AuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            DiagnosticCommand = new AsyncRelayCommand(RunDiagnosticAsync);
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AsyncRelayCommand LoginCommand { get; }
        public AsyncRelayCommand DiagnosticCommand { get; }

        private bool CanLogin(object? parameter)
        {
            var canLogin = !string.IsNullOrWhiteSpace(Username) &&
                          !string.IsNullOrWhiteSpace(Password) &&
                          !IsLoading;

            System.Diagnostics.Debug.WriteLine($"CanLogin: Username='{Username}', Password.Length={Password?.Length}, IsLoading={IsLoading}, Result={canLogin}");
            return canLogin;
        }

        private async Task LoginAsync(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine("=== LoginAsync DÉCLENCHÉE !!!");
            IsLoading = true;
            ErrorMessage = "Connexion en cours...";

            try
            {
                System.Diagnostics.Debug.WriteLine($"Tentative de connexion: Username='{Username}', Password length={Password?.Length ?? 0}");

                // Validation des entrées
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Veuillez saisir un nom d'utilisateur et un mot de passe.";
                    System.Diagnostics.Debug.WriteLine("ERREUR: Nom d'utilisateur ou mot de passe vide");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Initialisation des utilisateurs par défaut...");

                // S'assurer que les utilisateurs par défaut existent
                await _authService.InitializeDefaultUsersAsync();

                System.Diagnostics.Debug.WriteLine("Appel de LoginAsync sur AuthService...");
                var (success, message) = await _authService.LoginAsync(Username, Password);
                System.Diagnostics.Debug.WriteLine($"Résultat AuthService.LoginAsync: success={success}, message='{message}'");

                if (success)
                {
                    ErrorMessage = "Connexion réussie ! Ouverture...";
                    System.Diagnostics.Debug.WriteLine("LOGIN RÉUSSI - Tentative d'ouverture de MainWindow...");

                    try
                    {
                        // Ouvrir la fenêtre principale
                        var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                        System.Diagnostics.Debug.WriteLine("MainWindow récupérée du ServiceProvider");

                        mainWindow.Show();
                        System.Diagnostics.Debug.WriteLine("MainWindow.Show() appelée");

                        // Fermer la fenêtre de login
                        if (parameter is Window loginWindow)
                        {
                            System.Diagnostics.Debug.WriteLine("Fermeture de la LoginWindow...");
                            loginWindow.Close();
                            System.Diagnostics.Debug.WriteLine("LoginWindow fermée avec succès");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"ATTENTION: parameter n'est pas une Window, type reçu: {parameter?.GetType().Name ?? "null"}");
                        }
                    }
                    catch (Exception windowEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERREUR lors de l'ouverture de MainWindow: {windowEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {windowEx.StackTrace}");
                        ErrorMessage = $"Erreur lors de l'ouverture de la fenêtre principale: {windowEx.Message}";
                    }
                }
                else
                {
                    ErrorMessage = $"Échec de connexion: {message}";
                    System.Diagnostics.Debug.WriteLine($"ECHEC DE CONNEXION: {message}");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Erreur: {ex.Message}";
                ErrorMessage = errorMsg;
                System.Diagnostics.Debug.WriteLine($"EXCEPTION dans LoginAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Type d'exception: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Afficher aussi l'erreur à l'utilisateur
                MessageBox.Show(
                    $"Erreur lors de la connexion:\n\n{ex.Message}\n\nDétails techniques:\n{ex.InnerException?.Message}",
                    "Erreur de connexion",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("=== LoginAsync TERMINÉE");
            }
        }

        private async Task RunDiagnosticAsync(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine("=== DIAGNOSTIC DÉMARRÉ ===");
            IsLoading = true;
            ErrorMessage = "Test de la base de données...";

            try
            {
                bool success = await DatabaseDiagnostic.TestDatabaseConnectionAsync();

                if (success)
                {
                    ErrorMessage = "✓ Base de données OK - Vous pouvez vous connecter";
                    MessageBox.Show(
                        "✓ Diagnostic réussi !\n\nLa base de données fonctionne correctement.\nLes utilisateurs par défaut sont prêts:\n\n- admin / admin123\n- user / user123",
                        "Diagnostic - Succès",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "✗ Problème avec la base de données";
                    MessageBox.Show(
                        "✗ Diagnostic échoué !\n\nIl y a un problème avec la base de données.\nVérifiez que MySQL est démarré et configuré correctement.\n\nConsultez la console pour plus de détails.",
                        "Diagnostic - Échec",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors du diagnostic: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception diagnostic: {ex}");
                MessageBox.Show(
                    $"Erreur lors du diagnostic:\n\n{ex.Message}\n\nDétails:\n{ex.InnerException?.Message}",
                    "Erreur de diagnostic",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("=== DIAGNOSTIC TERMINÉ ===");
            }
        }
    }
}
