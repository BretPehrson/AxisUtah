namespace AxisUtah.Api.Models;

public class Address
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AddressId { get; set; }
    public string UnparsedAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = "UT";
    public string PostalCode { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}