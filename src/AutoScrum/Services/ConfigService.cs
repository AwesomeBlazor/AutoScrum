using System;
using System.Threading.Tasks;
using AutoScrum.AzureDevOps.Models;
using Blazored.LocalStorage;

namespace AutoScrum.Services
{
    public class ConfigService
    {
        private const string ConfigKey = "current-config";
        private readonly ILocalStorageService _localStorage;

        public ConfigService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<AzureDevOpsConnectionInfo> GetConfig()
        {
            return await _localStorage.GetItemAsync<AzureDevOpsConnectionInfo>(ConfigKey);
        }

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
