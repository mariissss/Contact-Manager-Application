using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContactManager.Core.Entities;

namespace ContactManager.Core.Interfaces
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetContactsAsync(
        int csvFileId,
        string? searchTerm,
        bool? isMarried,
        decimal? minSalary,
        decimal? maxSalary,
        DateTime? minDob,
        DateTime? maxDob,
        string? sortBy,
        bool isAscending,
        int pageNumber,
        int pageSize);

        Task<int> GetTotalCountAsync(
            int csvFileId,
            string? searchTerm,
            bool? isMarried,
            decimal? minSalary,
            decimal? maxSalary,
            DateTime? minDob,
            DateTime? maxDob);

        Task<Contact?> GetByIdAsync(int id);
        Task<int> AddRangeAsync(IEnumerable<Contact> contacts);
        Task<bool> UpdateAsync(Contact contact);
        Task<bool> DeleteAsync(int id);

    }
}
