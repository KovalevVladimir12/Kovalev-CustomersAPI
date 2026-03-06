using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kovalev.Models;

[Table("customers")]
public class Customer
{
    [Key]
    [Column("id")]
    public int CustomerId { get; set; }

    [Column("name")]  
    public string? FirstName { get; set; }

    [NotMapped]
    public string? LastName { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("address")]  
    public string? Address { get; set; }

    [NotMapped]
    public string? Phone { get; set; }

    [NotMapped]
    public DateTime CreatedAt { get; set; }
}