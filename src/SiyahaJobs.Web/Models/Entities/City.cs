using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.Models.Entities;

public class City
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public int CountryId { get; set; }
    public Country? Country { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Company> Companies { get; set; } = new List<Company>();
}
