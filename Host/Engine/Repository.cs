using Microsoft.Extensions.Logging;

namespace Hosts.ShopBot.Engine
{
    public class Repository<T>
    {
        private ILogger<Repository<T>> Logger { get; set; }

        public Repository(ILogger<Repository<T>> logger)
        {
            Logger = logger;
        }
    }
}