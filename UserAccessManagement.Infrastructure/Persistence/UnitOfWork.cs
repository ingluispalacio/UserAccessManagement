using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Infrastructure.Persistence.Context;
using UserAccessManagement.Infrastructure.Persistence.Repositories;

namespace UserAccessManagement.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly UserAccessDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        private readonly ILogger<UserRepository> _logger;

        public UnitOfWork(
            UserAccessDbContext context,
           ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = await _context.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "SaveChangesAsync took {Elapsed} ms",
                stopwatch.Elapsed.TotalMilliseconds);
            return await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
