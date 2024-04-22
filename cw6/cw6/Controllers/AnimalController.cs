using System.Data.SqlClient;
using cw6.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace cw6.Controllers;

[ApiController]
[Route("api/animals")]
public class AnimalController : ControllerBase
{

    private IConfiguration _configuration;
    
    public AnimalController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAnimals()
    {
        var animals = new List<GetAnimalsRsponse>();
        using (var con = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("SELECT * FROM Animals", con);
            sqlCommand.Connection.Open();
            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                animals.Add(new GetAnimalsRsponse(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4)
                    ));
            }
        }
        return Ok(animals);
    }
}