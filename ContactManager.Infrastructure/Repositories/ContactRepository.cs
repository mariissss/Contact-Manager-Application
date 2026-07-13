using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data;
using ContactManager.Core.Entities;
using ContactManager.Core.Interfaces;
using ContactManager.Infrastructure.Data;
using Dapper;

namespace ContactManager.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly DapperContext _context;

    public ContactRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contact>> GetContactsAsync(
        int csvFileId, string? searchTerm, bool? isMarried, decimal? minSalary, decimal? maxSalary,
        DateTime? minDob, DateTime? maxDob, string? sortBy, bool isAscending, int pageNumber, int pageSize)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Contacts WHERE CsvFileId = @CsvFileId ";

        if (!string.IsNullOrWhiteSpace(searchTerm))
            sql += " AND (Name LIKE @SearchTerm OR Phone LIKE @SearchTerm) ";

        if (isMarried.HasValue)
            sql += " AND Married = @IsMarried ";

        if (minSalary.HasValue)
            sql += " AND Salary >= @MinSalary ";

        if (maxSalary.HasValue)
            sql += " AND Salary <= @MaxSalary ";

        if (minDob.HasValue)
            sql += " AND DateOfBirth >= @MinDob ";

        if (maxDob.HasValue)
            sql += " AND DateOfBirth <= @MaxDob ";

        var validColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Name", "DateOfBirth", "Married", "Phone", "Salary" };

        var sortColumn = !string.IsNullOrWhiteSpace(sortBy) && validColumns.Contains(sortBy) ? sortBy : "Id";
        var sortDirection = isAscending ? "ASC" : "DESC";

        sql += $" ORDER BY {sortColumn} {sortDirection} ";
        sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var offset = (pageNumber - 1) * pageSize;

        return await connection.QueryAsync<Contact>(sql, new
        {
            CsvFileId = csvFileId,
            SearchTerm = $"%{searchTerm}%",
            IsMarried = isMarried,
            MinSalary = minSalary,
            MaxSalary = maxSalary,
            MinDob = minDob,
            MaxDob = maxDob,
            Offset = offset,
            PageSize = pageSize
        });
    }

    public async Task<int> GetTotalCountAsync(
        int csvFileId, string? searchTerm, bool? isMarried, decimal? minSalary, decimal? maxSalary, DateTime? minDob, DateTime? maxDob)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT COUNT(*) FROM Contacts WHERE CsvFileId = @CsvFileId ";

        if (!string.IsNullOrWhiteSpace(searchTerm))
            sql += " AND (Name LIKE @SearchTerm OR Phone LIKE @SearchTerm) ";

        if (isMarried.HasValue)
            sql += " AND Married = @IsMarried ";

        if (minSalary.HasValue)
            sql += " AND Salary >= @MinSalary ";

        if (maxSalary.HasValue)
            sql += " AND Salary <= @MaxSalary ";

        if (minDob.HasValue)
            sql += " AND DateOfBirth >= @MinDob ";

        if (maxDob.HasValue)
            sql += " AND DateOfBirth <= @MaxDob ";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            CsvFileId = csvFileId,
            SearchTerm = $"%{searchTerm}%",
            IsMarried = isMarried,
            MinSalary = minSalary,
            MaxSalary = maxSalary,
            MinDob = minDob,
            MaxDob = maxDob
        });
    }
    public async Task<Contact?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Contacts WHERE Id = @Id;";
        return await connection.QuerySingleOrDefaultAsync<Contact>(sql, new { Id = id });
    }

    public async Task<int> AddRangeAsync(IEnumerable<Contact> contacts)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT INTO Contacts (CsvFileId, Name, DateOfBirth, Married, Phone, Salary) 
                    VALUES (@CsvFileId, @Name, @DateOfBirth, @Married, @Phone, @Salary);";

        return await connection.ExecuteAsync(sql, contacts);
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        using var connection = _context.CreateConnection();
        var sql = @"UPDATE Contacts 
                    SET Name = @Name, DateOfBirth = @DateOfBirth, Married = @Married, Phone = @Phone, Salary = @Salary 
                    WHERE Id = @Id;";

        var affectedRows = await connection.ExecuteAsync(sql, contact);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Contacts WHERE Id = @Id;";
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}