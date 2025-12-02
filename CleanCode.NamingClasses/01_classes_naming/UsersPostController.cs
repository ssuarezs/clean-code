
namespace CleanCode.NamingClasses._01_classes_naming;

public class UsersPostController(IUserRepository repository, IPasswordHasher passwordHasher)
{
  private readonly UserCreator _creator = new(repository, passwordHasher);
  
  public Response Post(CreateUserRequest request)
    {
        try
        { 
            EnsureRequestIsValid(request);

            var user = _creator.Create(request);

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


  private static void EnsureRequestIsValid(CreateUserRequest request)
    {
      if (!IsValidRequest(request))
      {
        throw new Exception("Invalid Request");
      }
    }


  private static bool IsValidRequest(CreateUserRequest request)
  {
    return IsValidName(request) &&
           IsValidSurname(request) && 
           IsValidEmail(request) &&
           IsValidPassword(request);
  }


  private static bool IsValidPassword(CreateUserRequest request)
  {
    return !string.IsNullOrWhiteSpace(request.Password) &&
           request.Password.Length >= 8 &&
           request.Password.Equals(request.PasswordConfirmation);
  }


  private static bool IsValidEmail(CreateUserRequest request)
  {
    return !string.IsNullOrWhiteSpace(request.Email) && 
           request.Email.Contains('@');
  }


  private static bool IsValidSurname(CreateUserRequest request)
  {
    return !string.IsNullOrWhiteSpace(request.Surname);
  }


  private static bool IsValidName(CreateUserRequest request)
    {
      return !string.IsNullOrWhiteSpace(request.Name);
    }
}