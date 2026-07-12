using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ContactManager.Infrastructure.Data;

public class DapperContext
{
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;

	public DapperContext(IConfiguration configuration)
	{
		_configuration = configuration;
		// Беремо рядок підключення з налаштувань (ми його додамо в наступних етапах)
		_connectionString = _configuration.GetConnectionString("DefaultConnection")
							?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
	}

	public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
