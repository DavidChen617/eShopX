using System.Net.Http.Json;
using Infrastructure.Auth.ThirdPartyAuth.Google.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.ThirdPartyAuth.Google;

public class GoogleAuthClient(HttpClient client, IOptions<GoogleAuthOptions> options)
{
    private readonly GoogleAuthOptions _options = options.Value;

    public async Task<GoogleTokenResponse> ExchangeTokenAsync(string code, string codeVerifier, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUrl,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        };

        var resp = await client.PostAsync("token",
            new FormUrlEncodedContent(form), ct);

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException("Google", await resp.Content.ReadAsStringAsync(ct));

        return (await resp.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct))!;
    }
}
