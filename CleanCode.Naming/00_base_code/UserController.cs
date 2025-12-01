using System;
using System.Text.Json;

namespace CleanCode.Naming._00_base_code;

public sealed record UserRequest(string? Name, string? Surname, string? Email, string? Password, string? PasswordConfirmation);

public sealed record Response(int StatusCode, string Body);

public class User(int? id, string name, string surname, string email, string hashedPassword)
{
    public int? Id { get; set; } = id;
    public string Name { get; } = name;
    public string Surname { get; } = surname;
    public string Email { get; } = email;
    public string HashedPassword { get; } = hashedPassword;

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}

public interface IPasswordHasher
{
    string Hash(string password);
}

public interface IUserRepository
{
    User? ByEmail(string email);
    int Save(User user);
}

public class UsersController(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    public Response Post(UserRequest userRequest)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userRequest.Name) || string.IsNullOrWhiteSpace(userRequest.Surname)
                || string.IsNullOrWhiteSpace(userRequest.Email) || !userRequest.Email.Contains("@")
                || string.IsNullOrWhiteSpace(userRequest.Password) || userRequest.Password.Length < 8
                || !userRequest.Password.Equals(userRequest.PasswordConfirmation))
            {
                throw new Exception("Invalid Request");
            }

            if (userRepository.ByEmail(userRequest.Email) != null)
            {
                throw new Exception("User already exists");
            }

            User user = new User(
                null, 
                userRequest.Name!, 
                userRequest.Surname!, 
                userRequest.Email!, 
                passwordHasher.Hash(userRequest.Password!)
            );
            
            int userId = userRepository.Save(user);
            user.Id = userId;

            return new Response(201, user.ToJson());
        }
        catch (Exception e)
        {
            if (e.Message.Equals("Invalid Request"))
            {
                return new Response(400, "Invalid Request");
            }

            if (e.Message.Equals("User already exists"))
            {
                return new Response(409, "User already exists");
            }

            return new Response(500, "Internal Server Error");
        }
    }
}