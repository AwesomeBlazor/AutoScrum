using System;
using System.Threading.Tasks;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.Models;
using Blazored.LocalStorage;

namespace AutoScrum.Services
{
    public class ConfigService
    {
        private const string ConfigKey = "current-config";
        private const string ThemeKey = "current-theme";
        
        private readonly ILocalStorageService _localStorage;

        public ConfigService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<AzureDevOpsConnectionInfo?> GetConfig() => await _localStorage.GetItemAsync<AzureDevOpsConnectionInfo>(ConfigKey);

        public async Task SetConfig(AzureDevOpsConnectionInfo config)
        {
            try
            {
                await _localStorage.SetItemAsync(ConfigKey, config);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        public async Task<ThemeSettings?> GetTheme()
        {
            return await _localStorage.GetItemAsync<ThemeSettings>(ThemeKey);
        }

        public async Task SetTheme(ThemeSettings theme)
        {
            try
            {
                await _localStorage.SetItemAsync(ThemeKey, theme);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task Clear()
        {
            try
            {
                await _localStorage.RemoveItemAsync(ConfigKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
