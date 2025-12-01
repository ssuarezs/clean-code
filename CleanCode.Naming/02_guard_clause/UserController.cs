
namespace CleanCode.Naming._02_guard_clause;

public class UsersController(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    public Response Post(UserRequest userRequest)
    {
        try
        {
            EnsureRequestIsValid(userRequest);

            var user = CreateUser(userRequest);

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


    private User CreateUser(UserRequest userRequest)
    {
      EnsureUserDoesNotExist(userRequest);

      User user = new User(
        null, 
        userRequest.Name!, 
        userRequest.Surname!, 
        userRequest.Email!, 
        passwordHasher.Hash(userRequest.Password!)
      );
      int id = userRepository.Save(user);
      user.SetId(id);
      
      return user;
    }


    private void EnsureUserDoesNotExist(UserRequest userRequest)
    {
      if (userRepository.ByEmail(userRequest.Email!) != null)
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