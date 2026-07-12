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
        Task<IEnumerable<Contact>> GetContactsAsync(string? searchTerm, string? sortBy, bool isAscending, int pageNumber, int pageSize);

        Task<int> GetTotalCountAsync(string? searchTerm);

        Task<Contact?> GetByIdAsync(int id);

        // велике додавання з CSV 
        Task<int> AddRangeAsync(IEnumerable<Contact> contacts);

        Task<bool> UpdateAsync(Contact contact);

        Task<bool> DeleteAsync(int id);
    }
}
