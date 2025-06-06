using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MediTrack.Frontend.ViewModels.Base
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public virtual async Task InitializeAsync()
        {
            // Override en ViewModels que necesiten inicialización async
            await Task.CompletedTask;
        }

        protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsLoading = true;
                ErrorMessage = string.Empty;

                await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                await HandleErrorAsync(ex);
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
            }
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, T defaultValue = default(T))
        {
            if (IsBusy) return defaultValue;

            try
            {
                IsBusy = true;
                IsLoading = true;
                ErrorMessage = string.Empty;

                return await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                await HandleErrorAsync(ex);
                return defaultValue;
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
            }
        }

        protected virtual async Task HandleErrorAsync(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Error en {GetType().Name}: {exception.Message}");

            // Aquí puedes agregar logging, analytics, etc.
            await Task.CompletedTask;
        }

        protected virtual async Task ShowAlertAsync(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        protected virtual async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        // Commands básicos que pueden usar todos los ViewModels
        [RelayCommand]
        protected virtual async Task GoBack()
        {
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        protected virtual async Task Refresh()
        {
            await InitializeAsync();
        }
    }
}