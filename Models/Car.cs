using System.ComponentModel.DataAnnotations;
namespace CarRentalApp.Models;
public class Car
{
    public int Id { get; set; }

    [Required]
    public string Brand { get; set; }

    [Required]
    public string Model { get; set; }

    [Required]
    public decimal PricePerDay { get; set; }
}
