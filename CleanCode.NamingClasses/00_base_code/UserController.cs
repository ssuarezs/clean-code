
namespace CleanCode.NamingClasses._00_base_code;

public class UsersController(IUserRepository repository, IPasswordHasher passwordHasher)
{
    public Response Post(UserRequest request)
    {
        try
        {
            EnsureRequestIsValid(request);

            var user = Create(request);

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


    private User Create(UserRequest request)
    {
      EnsureUserDoesNotExist(request);

      var user = new User(
        null, 
        request.Name!, 
        request.Surname!, 
        request.Email!, 
        passwordHasher.Hash(request.Password!)
      );
      var id = repository.Save(user);
      user.SetId(id);
      
      return user;
    }


    private void EnsureUserDoesNotExist(UserRequest userRequest)
    {
      if (repository.ByEmail(userRequest.Email!) != null)
      {
        throw new Exception("User already exists");
      }
    }


    private static void EnsureRequestIsValid(UserRequest userRequest)
    {
      if (!IsValidRequest(userRequest))
      {
        throw new Exception("Invalid Request");
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
      return !string.IsNullOrWhiteSpace(userRequest.Password) &&
             userRequest.Password.Length >= 8 &&
             userRequest.Password.Equals(userRequest.PasswordConfirmation);
    }


    private static bool IsValidEmail(UserRequest userRequest)
    {
      return !string.IsNullOrWhiteSpace(userRequest.Email) && 
             userRequest.Email.Contains('@');
    }


    private static bool IsValidSurname(UserRequest userRequest)
    {
      return !string.IsNullOrWhiteSpace(userRequest.Surname);
    }


    private static bool IsValidName(UserRequest userRequest)
    {
      return !string.IsNullOrWhiteSpace(userRequest.Name);
    }
}