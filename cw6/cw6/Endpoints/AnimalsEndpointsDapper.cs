using System.Data.SqlClient;
using cw6.DTOs;
using Dapper;
using FluentValidation;

namespace cw6.Endpoints;

public static class AnimalsEndpointsDapper
{
    public static void RegisterAnimalsDapperEnpoints(this WebApplication app)
    {
        var animals = app.MapGroup("minimal-animals-dapper");
        animals.MapGet("", GetAnimals);
        animals.MapGet("{id:int}", GetAnimal);
        animals.MapPost("", CreateAnimal);
        animals.MapPut("{id:int}", ReplaceAnimal);
        animals.MapDelete("{id:int}", RemoveAnimal);
    }

    private static IResult GetAnimals(IConfiguration configuration)
    {
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var animals = con.Query<GetAnimalsResponse>("SELECT * FROM Animal");
            return Results.Ok(animals);
        }
    }

    private static IResult GetAnimal(IConfiguration configuration, int id)
    {
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var animal = con.QuerySingleOrDefault<GetAnimalDetailsResponse>(
                "SELECT * FROM Animal WHERE IdAnimal = @Id",
                new { Id = id });

            return animal is null ? Results.NotFound() : Results.Ok(animal);
        }
    }

    private static IResult ReplaceAnimal(IConfiguration configuration, IValidator<ReplaceAnimalRequest> validator,
        int id, ReplaceAnimalRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var affected = con.Execute(
                "UPDATE Animal SET Name = @name, Descripton = @description, Category = @category, Area = @area WHERE IdAnimal = @Id",
                new
                {
                    name = request.Name,
                    description = request.Description,
                    category = request.Category,
                    area = request.Area,
                    Id = id
                });
            return affected == 0 ? Results.NotFound() : Results.NoContent();
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
            var id = con.ExecuteScalar<int>(
                "INSERT INTO  Animal(Name, Descripton, Category, Area) VALUES (@name,@description,@category,@area)",
                new
                {
                    name = request.Name,
                    description = request.Description,
                    category = request.Category,
                    area = request.Area,
                });
            return Results.Created($"-minimal-animals-dapper/{id}", new CreateAnimalResponse(id, request));
        }
    }

    private static IResult RemoveAnimal(IConfiguration configuration, int id)
    {
        using (var con = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var affected = con.Execute("DELETE FROM Animal WHERE IdAnimal = @Id",
                new { Id = id }
                );
            return affected == 0 ? Results.NotFound() : Results.NoContent();
        }
    }
}