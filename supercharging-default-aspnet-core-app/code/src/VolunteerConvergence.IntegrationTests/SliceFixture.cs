using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Respawn;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.IntegrationTests
{
    static class SliceFixture
    {
        private static readonly IConfigurationRoot _configuration;
        private static readonly IServiceScopeFactory _scopeFactory;
        private static readonly ServiceCollection _baseServices;
        private static readonly Checkpoint _checkpoint;

        static SliceFixture()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            var startup = new Startup(_configuration);
            _baseServices = new ServiceCollection();
            _baseServices.AddLogging();

            startup.ConfigureServices(_baseServices);

            // override default service configuration after call to ConfigureServices and before building the provider

            var provider = _baseServices.BuildServiceProvider();
            _scopeFactory = provider.GetService<IServiceScopeFactory>();

            // Delete all data out of the test database before starting a test run.
            _checkpoint = new Checkpoint { TablesToIgnore = new string[0] };
            _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
        }

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<VolConContext>();

            try
            {
                await dbContext.BeginTransactionAsync().ConfigureAwait(false);

                await action(scope.ServiceProvider).ConfigureAwait(false);

                await dbContext.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                dbContext.RollbackTransaction();
                throw;
            }
        }

        public static async Task<T> ExecuteScopeAsync<T>(Action<ServiceCollection> initCustomServicesAction, Func<IServiceProvider, Task<T>> action)
        {
            var scopeServices = new ServiceCollection();
            foreach (var service in _baseServices)
            {
                scopeServices.Add(service);
            }

            initCustomServicesAction(scopeServices);
            var provider = scopeServices.BuildServiceProvider();

            using var scope = provider.GetService<IServiceScopeFactory>().CreateScope();
            return await ExecuteScopeAsync(scope, action);
        }

        public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            return await ExecuteScopeAsync(scope, action);
        }

        private static async Task<T> ExecuteScopeAsync<T>(IServiceScope scope, Func<IServiceProvider, Task<T>> action)
        {
            var dbContext = scope.ServiceProvider.GetService<VolConContext>();

            try
            {
                await dbContext.BeginTransactionAsync().ConfigureAwait(false);

                var result = await action(scope.ServiceProvider).ConfigureAwait(false);

                await dbContext.CommitTransactionAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception)
            {
                dbContext.RollbackTransaction();
                throw;
            }
        }

        public static Task ExecuteDbContextAsync(Func<VolConContext, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>()));

        public static Task ExecuteDbContextAsync(Func<VolConContext, ValueTask> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>()).AsTask());

        public static Task ExecuteDbContextAsync(Func<VolConContext, IMediator, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>(), sp.GetService<IMediator>()));

        public static Task ExecuteDbContextAsync(Func<VolConContext, IMediator, IServiceProvider, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>(), sp.GetService<IMediator>(), sp));

        public static Task<T> ExecuteDbContextAsync<T>(Func<VolConContext, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<VolConContext, ValueTask<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>()).AsTask());

        public static Task<T> ExecuteDbContextAsync<T>(Func<VolConContext, IMediator, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<VolConContext>(), sp.GetService<IMediator>()));

        public static Task InsertAsync<T>(params T[] entities) where T : class
        {
            return ExecuteDbContextAsync(db =>
            {
                foreach (var entity in entities)
                {
                    db.Set<T>().Add(entity);
                }
                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
            where TEntity : class
            where TEntity2 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2, TEntity3 entity3, params TEntity4[] entityType4Collection)
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
            where TEntity4 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);

                foreach (var entityType4 in entityType4Collection)
                {
                    db.Set<TEntity4>().Add(entityType4);
                }

                return db.SaveChangesAsync();
            });
        }

        public static Task<T> FindAsync<T>(Guid id)
            where T : class, IEntityWithId
        {
            return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
        }

        public static Task<List<T>> GetAllAsync<T>()
            where T : class, IEntityWithId
        {
            return ExecuteDbContextAsync(db => db.Set<T>().ToListAsync());
        }

        public static Task<TResponse> SendAsync<TResponse>(Action<ServiceCollection> initCustomServicesAction, IRequest<TResponse> request)
        {
            return ExecuteScopeAsync(initCustomServicesAction, sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        public static Task SendAsync(IRequest request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }
    }
}
