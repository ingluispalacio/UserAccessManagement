using FluentAssertions;
using Moq;
using UserAccessManagement.Application.Users.Queries.GetUsers;
using UserAccessManagement.Domain.Users;
using UserAccessManagement.Domain.Users.Interfaces;
using UserAccessManagement.Domain.Users.ValueObjects;

namespace UserAccessManagement.Tests.Handlers
{
    public class GetUsersHandlerTests
    {
        [Fact]
        public async Task Handle_WhenUsersExist_ReturnsPagedResultWithMappedUsers()
        {
            // Arrange
            var request = new GetUsersQuery(PageNumber: 2, PageSize: 5);

            var users = new List<User>
            {
                new User("Alice", "A", "Addr1", Email.Create("alice@example.com"), "h1"),
                new User("Bob", "B", null, Email.Create("bob@example.com"), "h2")
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetAllAsync(request.PageNumber, request.PageSize))
                .ReturnsAsync(users);
            userRepositoryMock
                .Setup(r => r.CountAsync())
                .ReturnsAsync(users.Count);

            var handler = new GetUsersHandler(userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var paged = result.Value;
            paged.PageNumber.Should().Be(request.PageNumber);
            paged.PageSize.Should().Be(request.PageSize);
            paged.TotalCount.Should().Be(users.Count);
            paged.Items.Should().HaveCount(users.Count);

            var itemList = paged.Items.ToList();
            itemList[0].Name.Should().Be("Alice");
            itemList[0].Email.Should().Be("alice@example.com");
            itemList[1].Address.Should().Be("NR"); // bob.Address == null -> "NR"

            userRepositoryMock.Verify(r => r.GetAllAsync(request.PageNumber, request.PageSize), Times.Once);
            userRepositoryMock.Verify(r => r.CountAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenNoUsers_ReturnsEmptyPagedResult()
        {
            // Arrange
            var request = new GetUsersQuery(PageNumber: 1, PageSize: 10);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetAllAsync(request.PageNumber, request.PageSize))
                .ReturnsAsync(new List<User>());
            userRepositoryMock
                .Setup(r => r.CountAsync())
                .ReturnsAsync(0);

            var handler = new GetUsersHandler(userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            var paged = result.Value;
            paged.Items.Should().BeEmpty();
            paged.TotalCount.Should().Be(0);
            paged.PageNumber.Should().Be(request.PageNumber);
            paged.PageSize.Should().Be(request.PageSize);

            userRepositoryMock.Verify(r => r.GetAllAsync(request.PageNumber, request.PageSize), Times.Once);
            userRepositoryMock.Verify(r => r.CountAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ExceptionIsPropagated()
        {
            // Arrange
            var request = new GetUsersQuery(PageNumber: 1, PageSize: 10);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetAllAsync(request.PageNumber, request.PageSize))
                .ReturnsAsync(new List<User>());
            userRepositoryMock
                .Setup(r => r.CountAsync())
                .ThrowsAsync(new Exception("DB failure"));

            var handler = new GetUsersHandler(userRepositoryMock.Object);

            // Act
            Func<Task> act = () => handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("DB failure");

            userRepositoryMock.Verify(r => r.GetAllAsync(request.PageNumber, request.PageSize), Times.Once);
            userRepositoryMock.Verify(r => r.CountAsync(), Times.Once);
        }
    }
}