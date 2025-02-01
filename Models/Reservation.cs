using CarRentalApp.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; } // ⚠️ Dodan `?` da bude nullable

    [Required]
    public int CarId { get; set; }

    [ForeignKey("CarId")]
    public virtual Car? Car { get; set; } // ⚠️ Dodan `?` da bude nullable

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public bool IsApproved { get; set; } = false;
}
