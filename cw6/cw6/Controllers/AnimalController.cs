using System.Data;
using System.Data.SqlClient;
using cw6.DTOs;
using cw6.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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

    private bool isSafeOrderBy(string orderBy)
    {
        orderBy = orderBy.ToLower();
        return orderBy.Equals("name") ||
               orderBy.Equals("description") ||
               orderBy.Equals("area") ||
               orderBy.Equals("category");
    }
    [HttpGet]
    public IActionResult GetAnimals(string orderBy = "name")
    {
        var animals = new List<GetAnimalsResponse>();
        using (var con = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            if (!isSafeOrderBy(orderBy)) return BadRequest();
            
            var sqlCommand = new SqlCommand("SELECT * FROM Animal ORDER BY " + orderBy, con);
            sqlCommand.Connection.Open();
            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                animals.Add(new GetAnimalsResponse(
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

    [HttpGet("{id:int}")]
    public IActionResult GetAnimalDetails(int id)
    {
        using (var con = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("SELECT * FROM Animal WHERE IdAnimal = @1", con);
            sqlCommand.Parameters.AddWithValue("@1", id);
            sqlCommand.Connection.Open();
            var reader = sqlCommand.ExecuteReader();
            if (!reader.Read()) return NotFound();
            return Ok(new GetAnimalDetailsResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4)
            ));
        }
    }

    [HttpPost]
    public IActionResult CreateAnimal(CreateAnimalRequest request)
    {
        using (var con = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("INSERT INTO Animal(Name, Description, Category, Area) " +
                                            "VALUES (@1, @2, @3, @4); SELECT CAST(SCOPE_IDENTITY() as INT)", con);
            sqlCommand.Parameters.AddWithValue("@1", request.Name);
            sqlCommand.Parameters.AddWithValue("@2", request.Description);
            sqlCommand.Parameters.AddWithValue("@3", request.Category);
            sqlCommand.Parameters.AddWithValue("@4", request.Area);
            sqlCommand.Connection.Open();
            var idAnimal = sqlCommand.ExecuteScalar();
            return Created($"animals/{idAnimal}", new CreateAnimalResponse((int)idAnimal, request));
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult RemoveAnimal(int id)
    {
        using (var con = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("DELETE FROM Animal WHERE IdAnimal = @1", con);
            sqlCommand.Parameters.AddWithValue("@1", id);
            sqlCommand.Connection.Open();
            var affected = sqlCommand.ExecuteNonQuery();
            return affected == 0 ? NotFound() : NoContent();
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult ReplaceAnimal(int id, ReplaceAnimalRequest request)
    {
        using (var con = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("UPDATE Animal SET Name = @1," +
                                            "Description = @2," +
                                            "Category = @3," +
                                            " Area = @4 " +
                                            "WHERE IdAnimal = @5", con);
            sqlCommand.Parameters.AddWithValue("@1", request.Name);
            sqlCommand.Parameters.AddWithValue("@2", request.Description);
            sqlCommand.Parameters.AddWithValue("@3", request.Category);
            sqlCommand.Parameters.AddWithValue("@4", request.Area);
            sqlCommand.Parameters.AddWithValue("@5", id);
            sqlCommand.Connection.Open();

            var affected = sqlCommand.ExecuteNonQuery();
            return affected == 0 ? NotFound() : NoContent();
        }
    }
}