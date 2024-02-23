using Microsoft.Extensions.Logging;

namespace PolygonApi.Rest;

public class Stocks : RestApiBase
{
	/// <summary>
	/// Initializes a new instance of the Stocks class, inheriting base initialization from <see cref="BaseClassName"/>.
	/// </summary>
	/// <param name="apiKey">The API key for authentication. See <see cref="RestApiBase"/> constructor for more details.</param>
	/// <param name="httpClient">The client for HTTP requests. See <see cref="RestApiBase"/> constructor for more details.</param>
	/// <param name="logger">The logger for this instance. See <see cref="RestApiBase"/> constructor for more details.</param>
	public Stocks(string apiKey, HttpClient httpClient, ILogger<RestApiBase> logger) : base(apiKey, httpClient, logger) {}

	/// <summary>
	/// Retrieves a list of all active tickers asynchronously.
	/// </summary>
	/// <returns>A <see cref="Task{List{PolygonApi.Neat.Stocks.Ticker}}"/> that represents the asynchronous operation, wrapping a list of all active tickers.</returns>
	/// <remarks>
	/// This method queries the Polygon.io Tickers API for all active tickers. It automatically handles pagination by checking the 'next_url' field and making subsequent requests until all pages have been retrieved. The method consolidates the results into a single list of tickers.
	/// Each ticker's symbol and name are extracted from the response, with defaults provided for any missing values. This method is intended to provide an easy way to fetch comprehensive ticker data in a single operation.
	/// For more details on the API and its response structure, refer to the official documentation at <a href="https://polygon.io/docs/stocks/get_v3_reference_tickers">Polygon's Stocks API</a>.
	/// </remarks>
	public async Task<List<PolygonApi.Neat.Stocks.Ticker>> GetAllTickersAsync()
	{
		List<PolygonApi.Neat.Stocks.Ticker> tickerList = new List<PolygonApi.Neat.Stocks.Ticker>();

		ResponseObjects.Stocks.Tickers tickersResponse = await GetTickerDataAsync(new RequestObjects.Stocks.Tickers() { Active = true } ); // Query all active Tickers
		bool lastPage = false;
		do
		{
			for(int i = 0; i < (tickersResponse.Results?.Count ?? 0); i++)
			{
				tickerList.Add(
					new Neat.Stocks.Ticker() 
					{ 
						Symbol = tickersResponse.Results?[i].TickerSymbol ?? "NO SYMBOL",
						Name = tickersResponse.Results?[i].Name ?? "NO NAME"
					}
				);
			}

			if(!string.IsNullOrWhiteSpace(tickersResponse.NextUrl))
			{
				tickersResponse = await GetTickerDataAsync(tickersResponse.NextUrl);
			}
			else
			{
				lastPage = true;
			}
		} while(!lastPage);

		return tickerList;
	}

	/// <summary>
	/// Retrieves ticker data asynchronously from the Tickers endpoint.
	/// </summary>
	/// <param name="parameters">The request parameters object containing query parameters, including an optional cursor for pagination.</param>
	/// <returns>A <see cref="Task{ResponseObjects.Stocks.Tickers}"/> that represents the asynchronous operation and wraps the response from the Tickers endpoint, containing a page of tickers.</returns>
	/// <remarks>
	/// The method makes an asynchronous request to the Polygon.io Tickers API. If the 'cursor' parameter is provided within the request parameters, it allows for the retrieval of subsequent pages of ticker data.
	/// For more details on the request parameters and response structure, see the official documentation at <a href="https://polygon.io/docs/stocks/get_v3_reference_tickers">Polygon's Stocks API</a>.
	/// </remarks>
	public async Task<ResponseObjects.Stocks.Tickers> GetTickerDataAsync(RequestObjects.Stocks.Tickers parameters)
	{
		return await GetRequestAsync<ResponseObjects.Stocks.Tickers, RequestObjects.Stocks.Tickers>("/v3/reference/tickers", parameters);
	}

	/// <summary>
	/// Retrieves ticker data asynchronously from the Tickers endpoint  Expects a full URI as returned by the Polygon API in the next_url field.  This method is intended for use only with pagination
	/// </summary>
	/// <param name="requestUri">The full uri including base, path, and parameters for the request.</param>
	/// <returns>A <see cref="Task{ResponseObjects.Stocks.Tickers}"/> that represents the asynchronous operation and wraps the response from the Tickers endpoint, containing a page of tickers.</returns>
	/// <remarks>
	/// The method makes an asynchronous request to the Polygon.io Tickers API. If the 'cursor' parameter is provided within the request parameters, it allows for the retrieval of subsequent pages of ticker data.
	/// For more details on the request parameters and response structure, see the official documentation at <a href="https://polygon.io/docs/stocks/get_v3_reference_tickers">Polygon's Stocks API</a>.
	/// </remarks>
	protected async Task<ResponseObjects.Stocks.Tickers> GetTickerDataAsync(string requestUri)
	{
		return await FullUriGetRequestAsync<ResponseObjects.Stocks.Tickers, RequestObjects.Stocks.Tickers>(requestUri);
	}
}