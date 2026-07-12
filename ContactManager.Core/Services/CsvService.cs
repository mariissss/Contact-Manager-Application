using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using ContactManager.Core.Entities;
using ContactManager.Core.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;

namespace ContactManager.Core.Services
{
    public class CsvService: ICsvService
    {
        public async Task<IEnumerable<Contact>> ParseCsvAsync(IFormFile file)
        {
            if ( file == null )
            {
                throw new ArgumentException("File was not found");
            }

            if ( file.Length == 0 )
            {
                throw new ArgumentException("File is empty");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("File is not a CSV file");
            }

            var contacts = new List<Contact>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<ContactMap>();

            while (await csv.ReadAsync())
            {
                try
                {
                    var contact = csv.GetRecord<Contact>();

                    if (contact != null && !string.IsNullOrWhiteSpace(contact.Name) && !string.IsNullOrWhiteSpace(contact.Phone))
                    {
                        contacts.Add(contact);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row: {ex.Message}");
                }
            }

            return contacts;


        }
    }
}
