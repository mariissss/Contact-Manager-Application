using ContactManager.Core.Entities;
namespace ContactManager.Models;

public class ContactListViewModel
{
    public int FileId { get; set; } 
    public IEnumerable<Contact> Contacts { get; set; } = new List<Contact>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool IsAscending { get; set; }
}