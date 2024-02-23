using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using PolygonApi.Rest.ResponseObjects.Stocks;
using Microsoft.Extensions.Logging;

namespace PolygonApi.Rest;

public class RestApiBase
{
	private readonly string _apiUrlBase = "https://api.polygon.io";

	private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestApiBase> _logger;

    /// <summary>
	/// Initializes a new instance of the <see cref="Stocks"/> class for interacting with stock data.
	/// </summary>
	/// <param name="apiKey">The API key used for authenticating requests to the Polygon.io API.</param>
	/// <param name="httpClient">The <see cref="System.Net.Http.HttpClient"/> used for making HTTP requests.</param>
	/// <param name="logger">The logger used for logging messages within the Stocks class.</param>
	/// <remarks>
	/// This constructor initializes the Stocks class with necessary dependencies for making API calls and logging.
	/// The <paramref name="httpClient"/> should be configured with any necessary headers or settings for use with the Polygon.io API.
	/// </remarks>
    public RestApiBase(string apiKey, HttpClient httpClient, ILogger<RestApiBase>? logger = null)
    {
    	_apiKey = apiKey;

        _httpClient = httpClient;
    	_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

    	if(logger == null)
    	{
    		using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
			{
			    builder
			        .AddConsole();
			});

			_logger = loggerFactory.CreateLogger<RestApiBase>();
    	}
    	else
    	{
    		_logger = logger as ILogger<RestApiBase>;
    	}
    	
    }

    /// <summary>
	/// Makes a GET request to the specified URI with query parameters derived from the request object.
	/// </summary>
	/// <typeparam name="TResponse">The expected response object type.</typeparam>
	/// <typeparam name="TRequest">The type of the request object containing query parameters.</typeparam>
	/// <param name="requestUri">The path portion of the URI including root ("/")</param>
	/// <param name="requestParameters">The request object containing the data for query parameters.  Will be appended at the end of the provided <paramref name="requestUri"> including ?.</param>
	/// <returns>The deserialized response object of type <typeparamref name="TResponse"/>.</returns>
	/// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
	/// <exception cref="InvalidOperationException">Thrown when JSON cannot be deserialized into <typeparamref name="TResponse"/>.</exception>
	protected async Task<TResponse> GetRequestAsync<TResponse, TRequest>(string requestUri, TRequest requestParameters)
	{
	    // Build the query string from requestParameters
	    string queryString = BuildQueryString(requestParameters);
	    string fullUri = $"{_apiUrlBase}{requestUri}";
	    if(!string.IsNullOrWhiteSpace(queryString))
	    {
	    	fullUri += $"?{queryString}";
	    }
	    _logger.LogTrace($"Request to: {fullUri}");

	    try
	    {
	        HttpResponseMessage response = await _httpClient.GetAsync(fullUri);
	        response.EnsureSuccessStatusCode();

	        string jsonString = await response.Content.ReadAsStringAsync();
	        TResponse responseObject = JsonSerializer.Deserialize<TResponse>(jsonString, new JsonSerializerOptions())
	            ?? throw new InvalidOperationException("Failed to deserialize the response to the specified type.");

	        return responseObject;
	    }
	    catch (HttpRequestException e)
	    {
	    	_logger.LogError($"Request exception: {e.Message}");
	        throw;
	    }
	}

	/// <summary>
	/// Makes a GET request to the fully specified URI, expecting a JSON response to be deserialized into the specified response object type.
	/// </summary>
	/// <typeparam name="TResponse">The expected response object type. This should match the structure of the JSON response.</typeparam>
	/// <typeparam name="TRequest">This type parameter is not used in this method but is included for consistency with sister methods. It may be ignored or set to <c>object</c> if calling without specific request parameters.</typeparam>
	/// <param name="requestUri">The full URI to which the request is made, including the base path, any resource paths, and query parameters.</param>
	/// <returns>A task that represents the asynchronous operation and yields the deserialized response object of type <typeparamref name="TResponse"/> upon completion.</returns>
	/// <exception cref="HttpRequestException">Thrown when the HTTP request fails. This includes scenarios such as network errors, non-success status codes, etc.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the method fails to deserialize the JSON response into the specified <typeparamref name="TResponse"/> type. This can occur if the JSON structure does not match the expected response object structure.</exception>
	/// <remarks>
	/// This method directly uses the provided <paramref name="requestUri"/> to make the HTTP GET request, bypassing the need to construct a query string from a request object. This is particularly useful for making paginated requests where the next page's URI is provided by the API response.
	/// </remarks>
	protected async Task<TResponse> FullUriGetRequestAsync<TResponse, TRequest>(string requestUri)
	{
	    _logger.LogTrace($"Request to: {requestUri}");

		try
	    {
	        HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
	        response.EnsureSuccessStatusCode();

	        string jsonString = await response.Content.ReadAsStringAsync();
	        TResponse responseObject = JsonSerializer.Deserialize<TResponse>(jsonString, new JsonSerializerOptions())
	            ?? throw new InvalidOperationException("Failed to deserialize the response to the specified type.");

	        return responseObject;
	    }
	    catch (HttpRequestException e)
	    {
	    	_logger.LogError($"Request exception: {e.Message}");
	        throw;
	    }
	}

	/// <summary>
	/// Constructs a query string for a URI based on the properties of a request parameters object.
	/// </summary>
	/// <typeparam name="TRequest">The type of the request parameters object. This method uses reflection to iterate through its properties.</typeparam>
	/// <param name="requestParameters">The request parameters object from which to construct the query string. Each property contributes to a key-value pair in the query string.</param>
	/// <returns>A string representing the constructed query string. This includes all non-null properties of the <paramref name="requestParameters"/> object, URL-encoded and concatenated with '&amp;'. Properties annotated with <see cref="PolygonIO.Attributes.QueryParameter"/> use the specified name as the key; otherwise, the property name is used.</returns>
	/// <remarks>
	/// This method dynamically builds a query string by inspecting the properties of the provided <typeparamref name="TRequest"/> object. It checks for the presence of the <see cref="PolygonIO.Attributes.QueryParameter"/> to use a custom query parameter name if provided. Properties with null values are excluded from the resulting query string. The values of the properties are URL-encoded to ensure the validity of the query string.
	/// </remarks>
	private static string BuildQueryString<TRequest>(TRequest requestParameters)
	{
	    IEnumerable<string> queryParams = typeof(TRequest).GetProperties()
	        .Where(prop => prop.GetValue(requestParameters) != null) // Ensure the property value is not null
		    .Select(prop =>
		    {
		        // Attempt to retrieve the QueryParameter attribute, if any
		        Attributes.QueryParameter? attribute = prop.GetCustomAttribute<Attributes.QueryParameter>();
		        
		        // Determine the key: Use the attribute name if available; otherwise, use the lowercase property name
		        string key = attribute?.Name ?? prop.Name.ToLower();
		        key = Uri.EscapeDataString(key);
		        
		        // Safely get the property value, ensuring it's not null before calling ToString()
		        var propertyValue = prop.GetValue(requestParameters);
		        if(propertyValue == null)
		        {
		            // Handle the case where the property value is unexpectedly null, if necessary
		            // For now, we'll skip adding this property to the queryParams collection
		            return null; // Using null here, will filter out these null entries in a subsequent step
		        }
		        
		        // Escape the value to ensure it's safe for inclusion in a URI
		        if(propertyValue is string)
		        {
		        	string value = Uri.EscapeDataString(propertyValue.ToString()!); // Null and string check above, confident ToString will not return null

		        	return $"{key}={value}";
		        }
		        
		        return null;
		    })
		    .OfType<string>()
		    .ToList();

	    return string.Join("&", queryParams);
	}

    // /// <summary>
	// /// Makes a GET request to the specified URI and deserializes the response into the expected response object type.
	// /// </summary>
	// /// <typeparam name="ResponseObject">The expected response object type.</typeparam>
	// /// <param name="apiUri">The address, including query parameters, of the resource to be fetched. This URI is appended to the base API URI.</param>
	// /// <returns>The deserialized response object of type <typeparamref name="ResponseObject"/>.</returns>
	// /// <exception cref="HttpRequestException">Thrown when the request fails due to an underlying issue with the HTTP request.</exception>
	// /// <exception cref="InvalidOperationException">Thrown when the response JSON cannot be deserialized into the specified <typeparamref name="ResponseObject"/> type.</exception>
	// /// <seealso cref="PolygonIO.Rest.ResponseObjects"/>
    // protected async Task<ResponseObject> GetRequestAsync<ResponseObject>(string apiUri)
    // {
    // 	apiUri = _apiUrlBase + apiUri;

    // 	try
    //     {
    //         // Make the GET request to the external API
    //         HttpResponseMessage response = await _httpClient.GetAsync(apiUri);
    //         response.EnsureSuccessStatusCode();

    //         // Deserialize the JSON response into the TickersResponse object
    //         string jsonString = await response.Content.ReadAsStringAsync();
    //         ResponseObject? responseObject = JsonSerializer.Deserialize<ResponseObject>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
	//         if (responseObject == null)
	//         {
	//             throw new InvalidOperationException("Failed to deserialize the response to the specified type.");
	//         }

	//         return responseObject;
    //     }
    //     catch (HttpRequestException e)
    //     {
    //         Console.WriteLine($"Request exception: {e.Message}");
    //         throw;
    //     }
    // }

    // /// <summary>
	// /// Builds a URI with query parameters based on the properties of the given request parameters object.
	// /// </summary>
	// /// <typeparam name="T">The type of the request parameters object. The properties of this type annotated with <see cref="QueryParamNameAttribute"/> will be used to build the query string.</typeparam>
	// /// <param name="requestParameters">The request parameters object containing the data to be included in the query string. Each property marked with <see cref="QueryParamNameAttribute"/> contributes to a query parameter, where the attribute's Name defines the parameter name.</param>
	// /// <param name="baseUri">The base URI to which the query string will be appended.</param>
	// /// <returns>A string representing the full URI with the query parameters appended.</returns>
	// /// <remarks>
	// /// This method inspects the properties of the <typeparamref name="T"/> instance to construct the query string. 
	// /// Properties without <see cref="QueryParamNameAttribute"/> or with null values are ignored.
	// /// Values are URL-encoded to ensure the generated URI is valid.
	// /// </remarks>
    // protected string BuildUriWithQueryParameters<T>(T requestParameters, string baseUri)
	// {
	//     List<string> queryParams = new List<string>();

	//     System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();
	//     foreach (System.Reflection.PropertyInfo property in properties)
	//     {
	//         QueryParamNameAttribute? attribute = property.GetCustomAttribute<QueryParamNameAttribute>();
	//         if (attribute != null)
	//         {
	//             object? value = property.GetValue(requestParameters);
	//             if (value != null)
	//             {
	//                 // Ensuring a meaningful ToString() representation for strings and other objects, do not include lists and arrays, etc.
	//                 if (value is string || !(value is IEnumerable<object>)) // Check if it's not a collection
	//                 {
	//                     string encodedValue = Uri.EscapeDataString(value.ToString()!);
	//                     queryParams.Add($"{attribute.Name}={encodedValue}");
	//                 }
	//             }
	//         }
	//     }

	//     string queryString = string.Join("&", queryParams);
	//     return $"{baseUri}?{queryString}";
	// }
}
