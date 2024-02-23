using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolygonApi.Rest.ResponseObjects.Stocks;

// See https://polygon.io/docs/stocks/get_v3_reference_tickers
public class Tickers
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("next_url")]
    public string? NextUrl { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("results")]
    public List<Ticker>? Results { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}