namespace Simbir.GO.BLL.Models;

public class Transport
{
    public int Id { get; set; }
    public bool CanBeRented { get; set; }

    /// <summary>
    /// Car, Bike, Scooter, etc.
    /// </summary>
    public string TransportType { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }

    /// <summary>
    /// License plate or identifier
    /// </summary>
    public string Identifier { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// price per minute
    /// </summary>
    public double? MinutePrice { get; set; }

    /// <summary>
    /// price per day
    /// </summary>
    public double? DayPrice { get; set; }
}
