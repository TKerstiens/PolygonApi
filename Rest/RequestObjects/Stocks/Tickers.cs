using PolygonApi.Attributes;

namespace PolygonApi.Rest.RequestObjects.Stocks;

// See https://polygon.io/docs/stocks/get_v3_reference_tickers
public class Tickers
{
	// Specify a ticker symbol. Defaults to empty string which queries all tickers.
	[QueryParameter("ticker")]
	public string? Ticker { get; set; } = null;

	// Specify the type of the tickers. Find the types that we support via our Ticker Types API. Defaults to empty string which queries all types.
	[QueryParameter("type")]
	public string? Type { get; set; } = null;

	// Filter by market type. By default all markets are included.
	[QueryParameter("market")]
	public string? Market { get; set; } = null;

	// Specify the primary exchange of the asset in the ISO code format.
	// Find more information about the ISO codes at the ISO org website. Defaults to empty string which queries all exchanges.
	[QueryParameter("exchange")]
	public string? Exchange { get; set; } = null;

	// Specify the CUSIP code of the asset you want to search for.
	// Find more information about CUSIP codes at their website. Defaults to empty string which queries all CUSIPs.
	[QueryParameter("cusip")]
	public string? Cusip { get; set; } = null;

	// Specify the CIK of the asset you want to search for.
	// Find more information about CIK codes at their website. Defaults to empty string which queries all CIKs.
	[QueryParameter("cik")]
	public string? Cik { get; set; } = null;

	// Specify a point in time to retrieve tickers available on that date. Defaults to the most recent available date.
	[QueryParameter("date")]
	public DateTime? Date { get; set; } = null;

	// Search for terms within the ticker and/or company name.
	[QueryParameter("search")]
	public string? Search { get; set; } = null;

	// Specify if the tickers returned should be actively traded on the queried date. Default is true.
	[QueryParameter("active")]
	public bool? Active { get; set; } = null;

	// Order results based on the sort field.
	[QueryParameter("order")]
	public string? Order { get; set; } = null;

	// Limit the number of results returned, default is 100 and max is 1000.
	[QueryParameter("limit")]
	public int? Limit { get; set; } = null;

	// Sort field used for ordering.
	[QueryParameter("sort")]
	public string? Sort { get; set; } = null;

	// Next Page - Not generally used since the entire url is returned by Polygon when a cursor is used
	[QueryParameter("cursor")]
	public string? Cursor { get; set; } = null;
}