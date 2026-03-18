using System.Net.Http.Json;
using Infrastructure.Auth.ThirdPartyAuth.Line.Models;

namespace Infrastructure.Auth.ThirdPartyAuth.Line;

public class LineAuthClient(HttpClient client, IOptions<LineAuthOptions> options)
{
    private readonly LineAuthOptions _options = options.Value;

    public async Task<LineTokenResponse> ExchangeTokenAsync(string code, string? codeVerifier, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ChannelId,
            ["client_secret"] = _options.ChannelSecret
        };

        if (!string.IsNullOrWhiteSpace(codeVerifier))
            form["code_verifier"] = codeVerifier;

        var resp = await client.PostAsync("token",
            new FormUrlEncodedContent(form), ct);

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException("LINE", await resp.Content.ReadAsStringAsync(ct));

        return (await resp.Content.ReadFromJsonAsync<LineTokenResponse>(ct))!;
    }

    public async Task<LineIdTokenPayload> VerifyIdTokenAsync(string idToken, string? nonce, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["id_token"] = idToken,
            ["client_id"] = _options.ChannelId
        };

        if (!string.IsNullOrWhiteSpace(nonce))
            form["nonce"] = nonce;

        var resp = await client.PostAsync("verify",
            new FormUrlEncodedContent(form), ct);

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException("LINE", await resp.Content.ReadAsStringAsync(ct));

        return (await resp.Content.ReadFromJsonAsync<LineIdTokenPayload>(ct))!;
    }
}
