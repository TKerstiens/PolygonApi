using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolygonApi.Rest.ResponseObjects.Stocks;

// See https://polygon.io/docs/stocks/get_v3_reference_tickers
public class Ticker
{
    [JsonPropertyName("active")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("cik")]
    public string? Cik { get; set; }

    [JsonPropertyName("composite_figi")]
    public string? CompositeFigi { get; set; }

    [JsonPropertyName("currency_name")]
    public string? CurrencyName { get; set; }

    [JsonPropertyName("last_updated_utc")]
    public DateTime? LastUpdatedUtc { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("market")]
    public string? Market { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("primary_exchange")]
    public string? PrimaryExchange { get; set; }

    [JsonPropertyName("share_class_figi")]
    public string? ShareClassFigi { get; set; }

    [JsonPropertyName("ticker")]
    public string? TickerSymbol { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}