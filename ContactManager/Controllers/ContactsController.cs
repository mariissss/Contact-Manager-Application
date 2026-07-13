using ContactManager.Core.Entities;
using ContactManager.Core.Interfaces;
using ContactManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ContactManager.Controllers;

public class ContactsController : Controller
{
    private readonly IContactRepository _contactRepository;
    private readonly ICsvFileRepository _fileRepository;
    private readonly ICsvService _csvService;

    public ContactsController(IContactRepository contactRepository, ICsvFileRepository fileRepository, ICsvService csvService)
    {
        _contactRepository = contactRepository;
        _fileRepository = fileRepository;
        _csvService = csvService;
    }

    public async Task<IActionResult> Index()
    {
        var files = await _fileRepository.GetAllAsync();
        return View(files);
    }

    [HttpPost]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        try
        {
            var contacts = (await _csvService.ParseCsvAsync(file)).ToList();

            var csvFile = new CsvFile
            {
                FileName = file.FileName,
                UploadDate = DateTime.Now
            };
            int fileId = await _fileRepository.AddAsync(csvFile);

            foreach (var contact in contacts)
            {
                contact.CsvFileId = fileId;
            }

            await _contactRepository.AddRangeAsync(contacts);

            TempData["SuccessMessage"] = $"File '{file.FileName}' successfully uploaded with {contacts.Count} contacts!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RenameFile([FromBody] RenameFileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewName))
            return BadRequest("File name cannot be empty.");

        var cleanName = dto.NewName.Trim();

        if (cleanName.All(c => c == '.'))
            return BadRequest("File name cannot consist only of dots.");

        if (cleanName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return BadRequest("File name contains invalid characters.");

        if (!cleanName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            cleanName += ".csv";
        }

        var result = await _fileRepository.UpdateNameAsync(dto.Id, cleanName);
        return result ? Ok(new { message = "Renamed successfully" }) : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var result = await _fileRepository.DeleteAsync(id);
        return result ? Ok(new { message = "Deleted successfully" }) : NotFound();
    }


    public async Task<IActionResult> Details(int id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null) return NotFound();

        ViewBag.FileId = id;
        ViewBag.FileName = file.FileName;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetContactsTable(
        int fileId, string? searchTerm, bool? isMarried, decimal? minSalary, decimal? maxSalary,
        DateTime? minDob, DateTime? maxDob, string? sortBy = "Id", bool isAsc = true, int page = 1, int pageSize = 10)
    {
        var contacts = await _contactRepository.GetContactsAsync(fileId, searchTerm, isMarried, minSalary, maxSalary, minDob, maxDob, sortBy, isAsc, page, pageSize);
        var totalCount = await _contactRepository.GetTotalCountAsync(fileId, searchTerm, isMarried, minSalary, maxSalary, minDob, maxDob);

        var viewModel = new ContactListViewModel
        {
            FileId = fileId,
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
    public async Task<IActionResult> UpdateContact([FromBody] Contact contact)
    {
        if (contact == null || contact.Id <= 0)
            return BadRequest("Invalid contact data.");

        if (contact.DateOfBirth > DateTime.Today)
            return BadRequest("Date of birth cannot be in the future.");

        if (!Regex.IsMatch(contact.Phone, @"^[\d\s()+-]+$"))
            return BadRequest("Phone number contains invalid characters (letters are not allowed).");

        if (contact.Salary < 0)
            return BadRequest("Salary cannot be negative.");

        var result = await _contactRepository.UpdateAsync(contact);
        return result ? Ok(new { message = "Updated successfully" }) : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var result = await _contactRepository.DeleteAsync(id);
        return result ? Ok(new { message = "Deleted successfully" }) : NotFound();
    }
    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] Contact contact)
    {
        if (contact == null || contact.CsvFileId <= 0)
            return BadRequest("Invalid contact data or missing file reference.");

        if (string.IsNullOrWhiteSpace(contact.Name))
            return BadRequest("Name cannot be empty.");

        if (contact.DateOfBirth > DateTime.Today)
            return BadRequest("Date of birth cannot be in the future.");

        if (!Regex.IsMatch(contact.Phone, @"^[\d\s()+-]+$"))
            return BadRequest("Phone number contains invalid characters.");

        if (contact.Salary < 0)
            return BadRequest("Salary cannot be negative.");

        await _contactRepository.AddRangeAsync(new[] { contact });
        return Ok(new { message = "Created successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCsv(int id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null) return NotFound();

        var contacts = await _contactRepository.GetContactsAsync(id, null, null, null, null, null, null, "Id", true, 1, 1000000);

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Name,Date of birth,Married,Phone,Salary");

        foreach (var c in contacts)
        {
            var salaryStr = c.Salary.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var dobStr = c.DateOfBirth.ToString("yyyy-MM-dd");
            var marriedStr = c.Married.ToString().ToLower();

            var nameStr = c.Name.Contains(',') ? $"\"{c.Name}\"" : c.Name;

            builder.AppendLine($"{nameStr},{dobStr},{marriedStr},{c.Phone},{salaryStr}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
        return File(bytes, "text/csv", file.FileName);
    }
}