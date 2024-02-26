using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codebelt.StatusMonitor;
using Cuemon.Extensions;
using Cuemon.Threading;

namespace Codebelt.StatusMonitorInMemory
{
    public class InMemoryStatusMonitorDataStore : IStatusMonitorDataStore
    {
        private readonly List<Operation> _operations = new();

        public Task CreateAsync(Operation dto, Action<AsyncOptions> setup = null)
        {
            _operations.Add(dto);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Operation dto, Action<AsyncOptions> setup = null)
        {
            var match = _operations.FindIndex(x => x.Id == dto.Id);
            _operations[match] = dto;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Operation dto, Action<AsyncOptions> setup = null)
        {
            var match = _operations.Find(x => x.Id == dto.Id);
            _operations.Remove(match);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Operation>> FindAllAsync(Action<QueryOptions<Operation>> setup = null)
        {
            var options = setup.Configure();
            return Task.FromResult(_operations.FindAll(operation => options.Filter(operation)).Take(options.MaxInclusiveResultCount));
        }
    }
}
