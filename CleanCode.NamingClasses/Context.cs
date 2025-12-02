using System.Text.Json;

namespace CleanCode.NamingClasses;

public sealed record CreateUserRequest(string? Name, string? Surname, string? Email, string? Password, string? PasswordConfirmation);

public sealed record Response(int StatusCode, string Body);

public class User(int? id, string name, string surname, string email, string hashedPassword)
{
  public int? Id { get; private set; } = id;
  public string Name { get; } = name;
  public string Surname { get; } = surname;
  public string Email { get; } = email;
  public string HashedPassword { get; } = hashedPassword;

  public string ToJson()
  {
    return JsonSerializer.Serialize(this);
  }


  public void SetId(int? id)
  {
    Id = id;
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