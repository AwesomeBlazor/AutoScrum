using System;
using System.Threading.Tasks;
using AutoScrum.Core.Config;
using Blazored.LocalStorage;

namespace AutoScrum.Services;

public class OldConfigService
{
    private const string ConfigKey = "current-config";

    private readonly ILocalStorageService _localStorage;

    public OldConfigService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SetConfig(ProjectConfigAzureDevOps config)
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