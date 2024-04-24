using System.Data.SqlClient;
using cw6.DTOs;
using cw6.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace cw6.Endpoints;

public static class AnimalsEnpoints
{
    public static void RegisterAnimalsEndpoints(this WebApplication app)
    {
        var animals = app.MapGroup("minimal-animals");
        animals.MapGet("", GetAnimals);
        animals.MapGet("{id:int}", GetAnimal);
        animals.MapPost("", CreateAnimal);
        animals.MapPut("{id:int}", ReplaceAnimal);
        animals.MapDelete("{id:int}", DeleteAnimal);
    }

    private static bool isSafeOrderBy(string orderBy)
    {
        orderBy = orderBy.ToLower();
        return orderBy.Equals("name") ||
               orderBy.Equals("description") ||
               orderBy.Equals("area") ||
               orderBy.Equals("category");
    }

    private static IResult GetAnimals(IConfiguration configuration, string orderBy = "name")
    {
        var animals = new List<GetAnimalsResponse>();
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            if (!isSafeOrderBy(orderBy)) return Results.BadRequest();

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

        return Results.Ok(animals);
    }

    private static IResult GetAnimal(IConfiguration configuration, int id)
    {
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("SELECT * FROM Animal WHERE IdAnimal = @1", con);
            sqlCommand.Parameters.AddWithValue("@1", id);
            sqlCommand.Connection.Open();
            var reader = sqlCommand.ExecuteReader();
            if (!reader.Read()) return Results.NotFound();
            return Results.Ok(new GetAnimalDetailsResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4)
            ));
        }
    }

    private static IResult CreateAnimal(IConfiguration configuration, IValidator<CreateAnimalRequest> validator,
        CreateAnimalRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }
        
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("INSERT INTO Animal(Name, Description, Category, Area)" +
                                            " VALUES (@1,@2,@3,@4); SELECT CAST(SCOPE_IDENTITY() as INT)", con);
            sqlCommand.Parameters.AddWithValue("@1", request.Name);
            sqlCommand.Parameters.AddWithValue("@2", request.Description);
            sqlCommand.Parameters.AddWithValue("@3", request.Category);
            sqlCommand.Parameters.AddWithValue("@4", request.Area);
            sqlCommand.Connection.Open();
            
            var id = sqlCommand.ExecuteScalar();
            return Results.Created($"animals/{id}", new CreateAnimalResponse((int)id, request));
        }
    }
    
    private static IResult ReplaceAnimal(IConfiguration configuration, IValidator<ReplaceAnimalRequest> validator,
        int id,
        ReplaceAnimalRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }
        
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("UPDATE Animal SET Name = @1," +
                                            "Description = @2," +
                                            "Category = @3," +
                                            "Area = @4," +
                                            "WHERE IdAnimal = @5"
                                            ,con);
            sqlCommand.Parameters.AddWithValue("@1", request.Name);
            sqlCommand.Parameters.AddWithValue("@2", request.Description);
            sqlCommand.Parameters.AddWithValue("@3", request.Category);
            sqlCommand.Parameters.AddWithValue("@4", request.Area);
            sqlCommand.Parameters.AddWithValue("@5", id);
            sqlCommand.Connection.Open();

            var affected = sqlCommand.ExecuteNonQuery();
            return affected == 0 ? Results.NotFound() : Results.NoContent();
        }
    }

    private static IResult DeleteAnimal(IConfiguration configuration, int id)
    {
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("DELETE FROM Animal WHERE IdAnimal = @1", con);
            sqlCommand.Parameters.AddWithValue("@1", id);
            sqlCommand.Connection.Open();

            var affected = sqlCommand.ExecuteNonQuery();
            return affected == 0 ? Results.NotFound() : Results.NoContent();
        }
    }
}