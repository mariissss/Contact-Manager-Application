using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContactManager.Core.Entities;
using ContactManager.Core.Interfaces;
using ContactManager.Infrastructure.Data;
using Dapper;

namespace ContactManager.Infrastructure.Repositories;

public class CsvFileRepository : ICsvFileRepository
{
    private readonly DapperContext _context;
    public CsvFileRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CsvFile>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM CsvFiles ORDER BY UploadDate DESC;";
        return await connection.QueryAsync<CsvFile>(sql);
    }

    public async Task<CsvFile?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM CsvFiles WHERE Id = @Id;";
        return await connection.QuerySingleOrDefaultAsync<CsvFile>(sql, new { Id = id });
    }

    public async Task<int> AddAsync(CsvFile file)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT INTO CsvFiles (FileName, UploadDate) 
                    VALUES (@FileName, @UploadDate);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, file);
    }

    public async Task<bool> UpdateNameAsync(int id, string newFileName)
    {
        using var connection = _context.CreateConnection();
        var sql = "UPDATE CsvFiles SET FileName = @NewFileName WHERE Id = @Id;";
        var affectedRows = await connection.ExecuteAsync(sql, new { NewFileName = newFileName, Id = id });
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM CsvFiles WHERE Id = @Id;";
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}
