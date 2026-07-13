using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContactManager.Core.Entities;

namespace ContactManager.Core.Interfaces
{
    public interface ICsvFileRepository
    {
        Task<IEnumerable<CsvFile>> GetAllAsync();
        Task<CsvFile?> GetByIdAsync(int id);
        Task<int> AddAsync(CsvFile file); //  поверне id нового запису
        Task<bool> UpdateNameAsync(int id, string newFileName);
        Task<bool> DeleteAsync(int id);
    }
}
