using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Application.Users.Commands.CreateUser;
using UserAccessManagement.Application.Users.DTOs;
using UserAccessManagement.Domain.Users;
using UserAccessManagement.Domain.Users.ValueObjects;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Tests.Handlers
{
    public class CreateUserHandlerTests
    {
        [Fact]
        public async Task Handle_WhenCommandIsValid_CreatesUserAndReturnsResponse()
        {
            // Arrange
            CreateUserCommand command = new CreateUserCommand(
                Name: "John",
                Lastname: "Doe",
                Email: "John.Doe@Example.com ",
                Password: "P@ssw0rd",
                Address: null);
            var normalizedEmail = command.Email.Trim().ToLower();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByEmailAsync(It.Is<string>(s => s == normalizedEmail)))
                .ReturnsAsync((User?)null);

            userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock
                .Setup(p => p.Hash(command.Password))
                .Returns("hashed-password");

            Mock<ILogger<CreateUserHandler>> loggerMock = new Mock<ILogger<CreateUserHandler>>();

            CreateUserHandler handler = new CreateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeOfType<UserResponse>();
            var response = result.Value;
            response.Email.Should().Be(normalizedEmail);
            response.Name.Should().Be(command.Name);
            response.Lastname.Should().Be(command.Lastname);
            response.Address.Should().Be("NR"); 

            userRepositoryMock.Verify(r => r.GetByEmailAsync(normalizedEmail), Times.Once);
            userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Name == command.Name &&
                u.Lastname == command.Lastname &&
                u.Email.Value == normalizedEmail)), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            passwordHasherMock.Verify(p => p.Hash(command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmailAlreadyExists_ReturnsFailureAndDoesNotCreate()
        {
            // Arrange
            var email = "existing@example.com";
            var command = new CreateUserCommand(
                Name: "Jane",
                Lastname: "Smith",
                Email: email,
                Password: "secret",
                Address: "Somewhere");

            var existingUser = new User("Jane", "Smith", "Somewhere", Email.Create(email), "existing-hash");

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(existingUser);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var loggerMock = new Mock<ILogger<CreateUserHandler>>();

            var handler = new CreateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be($"El correo {email} ya está en uso.");

            userRepositoryMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
            userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEmailIsInvalid_ReturnsFailureAndDoesNotPersist()
        {
            // Arrange
            var invalidEmail = "invalid-email";
            var command = new CreateUserCommand(
                Name: "Invalid",
                Lastname: "Email",
                Email: invalidEmail,
                Password: "pw",
                Address: null);

            var userRepositoryMock = new Mock<IUserRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var loggerMock = new Mock<ILogger<CreateUserHandler>>();

            var handler = new CreateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            // The handler wraps the exception in the Failure message; the Email VO throws "El formato del email no es valido."
            result.Message.Should().Contain("El formato del email no es valido.");

            userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUnitOfWorkThrows_ReturnsFailureAndLogsException()
        {
            // Arrange
            var command = new CreateUserCommand(
                Name: "Will",
                Lastname: "Fail",
                Email: "will.fail@example.com",
                Password: "pw",
                Address: "Addr");

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByEmailAsync(command.Email))
                .ReturnsAsync((User?)null);
            userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB failure"));

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock.Setup(p => p.Hash(command.Password)).Returns("hashed");

            var loggerMock = new Mock<ILogger<CreateUserHandler>>();

            var handler = new CreateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object); 

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("DB failure");

            userRepositoryMock.Verify(r => r.GetByEmailAsync(command.Email), Times.Once);
            userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            passwordHasherMock.Verify(p => p.Hash(command.Password), Times.Once);
        }
    }
}