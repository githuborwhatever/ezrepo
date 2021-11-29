using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hosts.Repository.Models;
using Microsoft.Extensions.Logging;

namespace Hosts.Repository.Engine
{
    public class Repository<T> where T : BaseModel
    {
        private ILogger<Repository<T>> Logger { get; set; }

        public string FilePath { get; private set; }

        private SemaphoreSlim Semaphore { get; set; }

        public Repository(ILogger<Repository<T>> logger, string dirPath) 
        {
            Logger = logger;

            FilePath = Path.Combine(dirPath, typeof(T).Name);

            Semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task QueueTask(Func<Task> task)
        {
            await Semaphore.WaitAsync();

            try
            {
                await task();
            }
            finally
            {
                Semaphore.Release(1);
            }
        }

        public async Task<string> EnsureFileStore()
        {
            List<T> model;
            if (!File.Exists(FilePath))
            {
                model = new List<T>();

                var modelString = JsonSerializer.Serialize(model);

                await QueueTask(async () => {
                    await File.WriteAllTextAsync(FilePath, modelString);
                });
            }

            return FilePath;
        }

        public async Task Save(T model)
        {
            var oldState = await Get();

            var alreadySavedModel = oldState.FirstOrDefault(x => x.Id == model.Id);

            var newState = oldState.ToList();
            if (alreadySavedModel != null)
            {
                newState.Remove(alreadySavedModel);
            }

            newState.Add(model);

            var newStateString = JsonSerializer.Serialize(newState);

            await QueueTask(async () => {
                await File.WriteAllTextAsync(FilePath, newStateString);
            });
        }

        public async Task<IEnumerable<T>> Get()
        {
            string fileString = string.Empty;
            await QueueTask(async () => {
                fileString = await File.ReadAllTextAsync(FilePath);
            });

            var result = JsonSerializer.Deserialize<List<T>>(fileString);

            return result;
        }

        public async Task<T> GetById(string id)
        {
            var allModels = await Get();

            var model = allModels.FirstOrDefault(x => x.Id == id);

            return model;
        }

        public async Task Delete(T model)
        {
            var oldState = await Get();

            var alreadySavedModel = oldState.FirstOrDefault(x => x.Id == model.Id);

            if (alreadySavedModel == null)
            { 
                return;
            }

            var newState = oldState.ToList();

            newState.Remove(alreadySavedModel);

            var newStateString = JsonSerializer.Serialize(newState);

            await QueueTask(async () => {
                await File.WriteAllTextAsync(FilePath, newStateString);
            });
        }
    }
}