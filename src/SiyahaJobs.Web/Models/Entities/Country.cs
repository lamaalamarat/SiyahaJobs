using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.Models.Entities;

public class Country
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(3)]
    public string? IsoCode { get; set; }

    public ICollection<City> Cities { get; set; } = new List<City>();
}
