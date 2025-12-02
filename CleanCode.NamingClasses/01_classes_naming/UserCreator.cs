namespace CleanCode.NamingClasses._01_classes_naming;

public class UserCreator(IUserRepository repository, IPasswordHasher passwordHasher)
{
  public User Create(CreateUserRequest request)
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
  
  private void EnsureUserDoesNotExist(CreateUserRequest userRequest)
  {
    if (repository.ByEmail(userRequest.Email!) != null)
    {
      throw new Exception("User already exists");
    }
  }
}