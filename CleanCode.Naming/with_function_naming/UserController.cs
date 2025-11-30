using System.Text.Json;

// Used for serialization/deserialization, replacing 'toJson()'

namespace CleanCode.Naming.with_function_naming;
// --- Data Transfer Objects (DTOs) and Domain Models ---

// C# 'record' is ideal for immutable data like DTOs
public record UserRequest(string? Name, string? Surname, string? Email, string? Password, string? PasswordConfirmation);

// C# 'record' for the HTTP response wrapper
public record Response(int StatusCode, string Body);

// A simplified C# User model (assuming it's defined elsewhere)
public class User
{
    // Nullable int for ID before saving
    public int? Id { get; set; }
    public string Name { get; }
    public string Surname { get; }
    public string Email { get; }
    public string HashedPassword { get; }

    public User(int? id, string name, string surname, string email, string hashedPassword)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Email = email;
        HashedPassword = hashedPassword;
    }
    
    // Helper to convert the user object to a JSON string
    public string ToJson()
    {
        // Use System.Text.Json for serialization
        return JsonSerializer.Serialize(this);
    }
}

// --- Interfaces for Dependency Injection ---

// Equivalent to Java's UserRepository interface
public interface IUserRepository
{
    User? ByEmail(string email);
    int Save(User user);
}

// Equivalent to Java's PasswordHasher interface
public interface IPasswordHasher
{
    string Hash(string password);
}

// --- Controller Implementation ---

public class UsersController
{
    // Use interfaces for dependency injection
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    // Constructor Injection
    public UsersController(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    // Renamed to Register to be more explicit about the action
    public Response Register(UserRequest userRequest)
    {
        try
        {
            // ** Request Validation **
            // C# uses null-conditional operators and string helper methods

            if (!IsValidRequest(userRequest))
            {
                // Use a more specific exception in real-world code
                throw new Exception("Invalid Request"); 
            }

            // ** Business Logic **
            if (_userRepository.ByEmail(userRequest.Email!) != null)
            {
                throw new Exception("User already exists");
            }

            // Create the new User object
            User newUser = new User(
                id: null, // ID is null until saved
                name: userRequest.Name!,
                surname: userRequest.Surname!,
                email: userRequest.Email!,
                hashedPassword: _passwordHasher.Hash(userRequest.Password!)
            );

            // Save and update the ID
            int userId = _userRepository.Save(newUser);
            newUser.Id = userId; // In C#, you can set a property if it has a setter
            
            // Return Success Response (201 Created)
            return new Response(201, newUser.ToJson());
        }
        catch (Exception e)
        {
            // ** Exception Handling **
            // Switch expression for cleaner conditional logic on the exception message
            return e.Message switch
            {
                "Invalid Request" => new Response(400, "Invalid Request"),
                "User already exists" => new Response(409, "User already exists"),
                _ => new Response(500, "Internal Server Error") // Default 500 error
            };
        }
    }

    private static bool IsValidRequest(UserRequest userRequest)
    {
        return IsValidName(userRequest) &&
               IsValidSurname(userRequest) &&
               IsValidEmail(userRequest) &&
               IsValidPassword(userRequest);
    }

    private static bool IsValidPassword(UserRequest userRequest)
    {
        return !string.IsNullOrWhiteSpace(userRequest.Password)&&
               userRequest.Password.Length >= 8 &&
               userRequest.Password.Equals(userRequest.PasswordConfirmation);
    }

    private static bool IsValidEmail(UserRequest userRequest)
    {
        return string.IsNullOrWhiteSpace(userRequest.Email);
    }

    private static bool IsValidSurname(UserRequest userRequest)
    {
        return !string.IsNullOrWhiteSpace(userRequest.Surname);
    }

    private static bool IsValidName(UserRequest userRequest)
    {
        return !string.IsNullOrWhiteSpace(userRequest.Name) &&
               userRequest.Email != null && 
               userRequest.Email.Contains('@');
    }
}