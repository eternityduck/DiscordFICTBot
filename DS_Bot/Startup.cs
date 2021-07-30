using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DS_Bot
{
    public class Startup
    {
        public IConfigurationRoot ConfigurationRoot { get; }

        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddYamlFile("_config.yaml");
            ConfigurationRoot = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.StartAsync();
        }

        private async Task StartAsync()
        {
            throw new NotImplementedException();
        }
        public async Task RunAsync(){}
        
    }
}