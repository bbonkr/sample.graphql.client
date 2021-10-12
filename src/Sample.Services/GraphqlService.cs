using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample.Services
{
    public class GraphqlService
    {
        public GraphqlService(HttpClient client, ILogger<GraphqlService> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public Task Execture(GraphqlRequestModel model)
        {
            return Task.CompletedTask;
        }    

        private readonly HttpClient client;
        private readonly ILogger logger;
    }

    public class GraphqlRequestModel
    {
        public GraphqlRequestModel()
        {
            RequestHeaders = new Dictionary<string, string>();
        }

        public string Payload { get; set; }

        public string Variables { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; }
    }

    public class GraphqlOptions
    {
        public const string Name = "Graphql";

        public string Endpoint { get; set; }
    }
}
