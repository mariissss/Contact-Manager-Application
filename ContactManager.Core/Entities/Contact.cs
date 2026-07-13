using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactManager.Core.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public int CsvFileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } 
        public bool Married { get; set; }
        public string Phone { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }
}
