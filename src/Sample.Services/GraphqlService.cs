using kr.bbon.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Services
{
    public class GraphqlService
    {
        public GraphqlService(
            HttpClient client,
            IOptionsMonitor<GraphqlOptions> graphqlOptionsAccessor,
            IOptionsMonitor<JsonSerializerOptions> jsonSerializerOptionsAccessor,
            ILogger<GraphqlService> logger)
        {
            this.client = client;
            this.jsonSerializerOptions= jsonSerializerOptionsAccessor.CurrentValue;
            this.graphqlOptions = graphqlOptionsAccessor.CurrentValue;
            this.logger = logger;
        }

        public async Task<TResponseModel> ExecuteAsync<TRequestModel, TResponseModel>(
            TRequestModel model,
            Dictionary<string, string> requestHeaders = null,
            CancellationToken cancellationToken = default)
            where TRequestModel : GraphqlRequestModel 
            where TResponseModel : class
        {
            TResponseModel responseModel = default;
            var authorizationHeaderKey = "Authorization";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(graphqlOptions.Endpoint));
            if (requestHeaders != null && requestHeaders.ContainsKey(authorizationHeaderKey))
            {
                request.Headers.Add(authorizationHeaderKey, requestHeaders[authorizationHeaderKey]);
            }
            else
            {
                request.Headers.Add("xc-token", string.Join(" ", new[] {
                    graphqlOptions.AccessTokenScheme,
                    graphqlOptions.AccessToken
                }).Trim());
            }

            if (requestHeaders != null)
            {
                foreach (var item in requestHeaders.Where(x => x.Key != authorizationHeaderKey))
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            var requestStringContent = JsonSerializer.Serialize(model, jsonSerializerOptions);
            
            request.Content = new StringContent(requestStringContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            var json = string.Empty;

            if (response.Content != null)
            {
                json = await response.Content.ReadAsStringAsync();
            }

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var graphqlResult = JsonSerializer.Deserialize<GraphqlResult<TResponseModel>>(json, jsonSerializerOptions);

                    responseModel = graphqlResult.Data;
                }
            }
            else
            {
                throw new ApiException(System.Net.HttpStatusCode.BadRequest, json);
            }

            return responseModel;
        }

        private readonly HttpClient client;
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly GraphqlOptions graphqlOptions;
        private readonly ILogger logger;
    }

    public class GraphqlResult<TModel>
    {
        public TModel Data { get; set; }
    }
}
