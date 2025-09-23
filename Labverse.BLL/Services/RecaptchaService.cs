using Labverse.BLL.DTOs.Recaptcha;
using Labverse.BLL.Interfaces;
using Labverse.BLL.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Labverse.BLL.Services;

public class RecaptchaService : IRecaptchaService
{
    private readonly RecaptchaSettings _recaptchaSettings;
    private readonly HttpClient _httpClient;

    public RecaptchaService(IOptions<RecaptchaSettings> recaptchaSettings, HttpClient httpClient)
    {
        _recaptchaSettings = recaptchaSettings.Value;
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        if (_recaptchaSettings.Bypass)
            return true;

        var response = await _httpClient.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaSettings.SecretKey}&response={token}",
            null
        );

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>();
        return result?.Success ?? false;
    }
}
