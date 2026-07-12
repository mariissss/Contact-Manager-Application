using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContactManager.Core.Entities;
using Microsoft.AspNetCore.Http;
using ContactManager.Core.Services;

namespace ContactManager.Core.Interfaces;

public interface ICsvService
{
    Task<IEnumerable<Contact>> ParseCsvAsync(IFormFile file);
}
