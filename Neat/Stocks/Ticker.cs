namespace PolygonApi.Neat.Stocks;

public class Ticker
{
	public required string Symbol { get; set; }

	private string? _name = null;
	public string Name 
	{ 
		get => _name ?? Symbol;
		set => _name = value;
	}
}