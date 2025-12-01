using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PCBTracker.Services
{
    public class MaraHollyOrdersODataClient : IMaraHollyOrdersODataClient
    {
        private readonly HttpClient _httpClient;

        // Hard-coded for now, you’ll hide these later.
        private const string Username = "wjabour@MaraTech";
        private const string Password = "Wajood13524@@@";

        public MaraHollyOrdersODataClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IReadOnlyList<MaraHollyODataRow>> GetMaraHollyOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            // Prepare request to the specific GI
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "MaraHollyOrders?$format=json");

            // Basic Auth header
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{Username}:{Password}"));

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var root = await JsonSerializer.DeserializeAsync<MaraHollyOrdersODataResponse>(
                stream,
                options,
                cancellationToken);

            return root?.Value ?? new List<MaraHollyODataRow>();
        }
    }
}
