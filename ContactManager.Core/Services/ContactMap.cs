using ContactManager.Core.Entities;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public sealed class ContactMap : ClassMap<Contact>
{
    public ContactMap()
    {
        Map(m => m.Name).Name("Name");
        Map(m => m.DateOfBirth).Name("Date of birth");
        Map(m => m.Married).Name("Married");
        Map(m => m.Phone).Name("Phone");
        Map(m => m.Salary).Name("Salary");
    }
}
