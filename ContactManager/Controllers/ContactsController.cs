using ContactManager.Core.Entities;
using ContactManager.Core.Interfaces;
using ContactManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Controllers;

public class ContactsController : Controller
{
    private readonly IContactRepository _repository;
    private readonly ICsvService _csvService;

    public ContactsController(IContactRepository repository, ICsvService csvService)
    {
        _repository = repository;
        _csvService = csvService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetContactsTable(string? searchTerm, string? sortBy = "Id", bool isAsc = true, int page = 1, int pageSize = 10)
    {
        var contacts = await _repository.GetContactsAsync(searchTerm, sortBy, isAsc, page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(searchTerm);
        var viewModel = new ContactListViewModel
        {
            Contacts = contacts,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            SearchTerm = searchTerm,
            SortBy = sortBy,
            IsAscending = isAsc
        };

        return ViewComponent("ContactList", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        try
        {
            var contacts = await _csvService.ParseCsvAsync(file);
            await _repository.AddRangeAsync(contacts);
            TempData["SuccessMessage"] = "CSV file successfully uploaded and stored in DB!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateContact([FromBody] Contact contact)
    {
        if (contact == null || contact.Id <= 0)
        {
            return BadRequest("Invalid contact data.");
        }
        var result = await _repository.UpdateAsync(contact);
        if (!result)
        {
            return NotFound();
        }

        return Ok(new { message = "Updated successfully" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { message = "Deleted successfully" });
    }
}