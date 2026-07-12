using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public async Task<IEnumerable<Contact>> GetContactsAsync(string? searchTerm, string? sortBy, bool isAscending, int pageNumber, int pageSize)
    {
        using var connection = _context.CreateConnection();

        var sql = "SELECT * FROM Contacts WHERE 1=1 ";

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += " AND (Name LIKE @SearchTerm OR Phone LIKE @SearchTerm) ";
        }

        var validColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Name", "DateOfBirth", "Married", "Phone", "Salary" };

        var sortColumn = !string.IsNullOrWhiteSpace(sortBy) && validColumns.Contains(sortBy) ? sortBy : "Id";
        var sortDirection = isAscending ? "ASC" : "DESC";

        sql += $" ORDER BY {sortColumn} {sortDirection} ";
        sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var offset = (pageNumber - 1) * pageSize;

        return await connection.QueryAsync<Contact>(sql, new
        {
            SearchTerm = $"%{searchTerm}%",
            Offset = offset,
            PageSize = pageSize
        });
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT COUNT(*) FROM Contacts WHERE 1=1 ";

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += " AND (Name LIKE @SearchTerm OR Phone LIKE @SearchTerm) ";
        }

        return await connection.ExecuteScalarAsync<int>(sql, new { SearchTerm = $"%{searchTerm}%" });
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
        var sql = @"INSERT INTO Contacts (Name, DateOfBirth, Married, Phone, Salary) 
                    VALUES (@Name, @DateOfBirth, @Married, @Phone, @Salary);";

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
