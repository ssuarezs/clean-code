
namespace CleanCode.Naming._00_base_code;

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