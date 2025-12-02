

using CleanCode.NamingClasses._01_classes_naming;
using Moq;

namespace CleanCode.NamingClasses.Test;

public class UsersPostControllerTest
{
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IPasswordHasher> _passwordHasherMock;
        
  private readonly UsersPostController _usersPostController;

  public UsersPostControllerTest()
  {
    _userRepositoryMock = new Mock<IUserRepository>();
    _passwordHasherMock = new Mock<IPasswordHasher>();
            
    _usersPostController = new UsersPostController(
      _userRepositoryMock.Object, 
      _passwordHasherMock.Object
    );
  }

  [Fact]
  public void ShouldReturn400WithAnInvalidRequest()
  {
    var invalidRequest = new CreateUserRequest("", "Gridman", "Tiago@example.com", "password", "password");

    Response response = _usersPostController.Post(invalidRequest);

    Assert.Equal(400, response.StatusCode);
    Assert.Equal("Invalid Request", response.Body);
  }

  [Fact]
  public void ShouldReturn409IfTheUserAlreadyExists()
  {
    var existingUserRequest = new CreateUserRequest("Tiago", "Gridman", "Tiago@example.com", "password123", "password123");
            
    _userRepositoryMock
      .Setup(repo => repo.ByEmail("Tiago@example.com"))
      .Returns(new User(1, "Tiago", "Gridman", "Tiago@example.com", "hashedPassword"));

    Response response = _usersPostController.Post(existingUserRequest);

    Assert.Equal(409, response.StatusCode);
    Assert.Equal("User already exists", response.Body);
  }

  [Fact]
  public void ShouldCreateAValidUser()
  {
    var validRequest = new CreateUserRequest("Tiago", "Gridman", "Tiago@example.com", "password123", "password123");
    const int expectedId = 1;
    const string expectedHashedPassword = "hashedPassword";

    _userRepositoryMock
      .Setup(repo => repo.ByEmail("Tiago@example.com"))
      .Returns((User)null!); 
            
    _passwordHasherMock
      .Setup(hasher => hasher.Hash("password123"))
      .Returns(expectedHashedPassword);
                
    _userRepositoryMock
      .Setup(repo => repo.Save(It.IsAny<User>()))
      .Returns(expectedId);

    Response response = _usersPostController.Post(validRequest);

    Assert.Equal(201, response.StatusCode);
            
    string expectedJson = $"{{\"Id\":{expectedId},\"Name\":\"Tiago\",\"Surname\":\"Gridman\",\"Email\":\"Tiago@example.com\",\"HashedPassword\":\"{expectedHashedPassword}\"}}";
    Assert.Equal(expectedJson, response.Body);


    _userRepositoryMock.Verify(repo => repo.Save(
        It.Is<User>(user => 
          user.Name == "Tiago" &&
          user.Surname == "Gridman" &&
          user.Email == "Tiago@example.com" &&
          user.HashedPassword == expectedHashedPassword
        )), 
      Times.Once());
  }
}