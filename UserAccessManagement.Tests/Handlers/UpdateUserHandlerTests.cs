using FluentAssertions;
using Moq;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Application.Users.Commands.UpdateUser;
using UserAccessManagement.Domain.Users;
using UserAccessManagement.Domain.Users.ValueObjects;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Tests.Handlers
{
    public class UpdateUserHandlerTests
    {
        [Fact]
        public async Task Handle_WhenUserExists_UpdatesAndSavesSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User("OldName", "OldLast", "OldAddr", Email.Create("old@example.com"), "hash");
            var command = new UpdateUserCommand(
                Id: userId,
                Name: "NewName",
                Lastname: "NewLast",
                Email: "New.Email@Example.com ",
                Address: null // should default to "NR"
            );

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Usuario actualizado exitosamente.");
            // Verify the domain entity was updated before saving
            existingUser.Name.Should().Be(command.Name);
            existingUser.Lastname.Should().Be(command.Lastname);
            existingUser.Email.Value.Should().Be(command.Email.ToLower().Trim());
            existingUser.Address.Should().Be("NR");

            userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ReturnsFailureAndDoesNotSave()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserCommand(
                Id: userId,
                Name: "Name",
                Lastname: "Last",
                Email: "test@example.com",
                Address: "Addr"
            );

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var handler = new UpdateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Usuario no encontrado.");

            userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEmailIsInvalid_ThrowsArgumentExceptionAndDoesNotSave()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User("Name", "Last", "Addr", Email.Create("valid@example.com"), "hash");
            var invalidEmail = "invalid-email";
            var command = new UpdateUserCommand(
                Id: userId,
                Name: "Name",
                Lastname: "Last",
                Email: invalidEmail,
                Address: null
            );

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var handler = new UpdateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("El formato del email no es valido.*");

            userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUnitOfWorkThrows_ExceptionIsPropagated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User("Old", "OldLast", "OldAddr", Email.Create("old@example.com"), "hash");
            var command = new UpdateUserCommand(
                Id: userId,
                Name: "Updated",
                Lastname: "UpdatedLast",
                Email: "updated@example.com",
                Address: "Addr"
            );

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB failure"));

            var handler = new UpdateUserHandler(
                userRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("DB failure");

            // The update occurred on the aggregate before SaveChanges was called
            existingUser.Name.Should().Be(command.Name);
            existingUser.Email.Value.Should().Be(command.Email.ToLower().Trim());

            userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}