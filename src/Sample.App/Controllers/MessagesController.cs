using Microsoft.AspNetCore.Mvc;
using kr.bbon.AspNetCore;
using kr.bbon.AspNetCore.Mvc;
using System.Threading.Tasks;
using Sample.Services;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json.Serialization;

namespace Sample.App.Controllers
{
    [ApiController]
    [Area(DefaultValues.AreaName)]
    [Route(DefaultValues.RouteTemplate)]
    public class MessagesController : ApiControllerBase
    {
        public MessagesController(
            GraphqlService graphqlService,
            IOptionsMonitor<JsonSerializerOptions> jsonSerializerOptionsAccessor,
            ILogger<MessagesController> logger)
        {
            this.graphqlService = graphqlService;
            this.logger = logger;
            jsonSerializerOptions = jsonSerializerOptionsAccessor.CurrentValue;
        }


        [HttpGet]
        public async Task<MessagesResponseModel> Get([FromQuery] int page = 1, int take = 10)
        {
            var offset = (page - 1) * take;
            
            var query = @"query MessagesList ($offset: Int, $limit: Int) {
    list: MessagesList(offset: $offset, limit: $limit, sort: ""-created_at"") {
    id
    title
    created_at
  }
  totalRecords: MessagesCount
}";
            
            var variables = new ListVariables
            {
                Offset = offset,
                Limit = take,
            };

            var requestModel = new GraphqlRequestModel<ListVariables>
            {
                Query = query,
                Variables = variables,
            };

            var result = await graphqlService.ExecuteAsync<GraphqlRequestModel<ListVariables>, MessagesResponseModel>(requestModel);

            return result;
        }

        [HttpPost]
        public async Task<MessageModel> Post([FromBody] MessagePostModel model)
        {
            var query = @"
mutation AddMessage($title: String) {
    item: MessagesCreate(data: {
        title: $title
    }){
        id
        title
        created_at
    }
}";
            var requestModel = new GraphqlRequestModel<AddMessageVaraibles>
            {
                Query = query,
                Variables = new AddMessageVaraibles
                {
                    Title = model.Title,
                }
            };

            var result = await graphqlService.ExecuteAsync<GraphqlRequestModel<AddMessageVaraibles>, MessageResponseModel>(requestModel);

            return result?.Item;
        }

        [HttpPatch("{id}")]
        public async Task<MessageModel> Patch(long id, [FromBody] MessagePostModel model)
        {
            var query = @"
mutation UpdateMessage($id: String, $title: String) {
  item: MessagesUpdate(id: $id, data:{title: $title}){
    id
    title
    created_at
  }
}
";
            var requestModel = new GraphqlRequestModel<UpdateMessageVariables>
            {
                Query = query,
                Variables = new UpdateMessageVariables
                {
                    Id = id.ToString(),
                    Title = model.Title,
                }
            };

            var result = await graphqlService.ExecuteAsync<GraphqlRequestModel<UpdateMessageVariables>, MessageResponseModel>(requestModel);

            return result?.Item;
        }

        [HttpDelete("{id}")]
        public async Task<DeleteMessageResponseModel> Delete(long id)
        {
            var query = @"
mutation DeleteMessage($id: String) {
	result: MessagesDelete(id: $id)
}";
            var requestModel = new GraphqlRequestModel<DeleteMessageVariables>
            {
                Query = query,
                Variables = new DeleteMessageVariables
                {
                    Id = id.ToString(),
                }
            };

            var result = await graphqlService.ExecuteAsync<GraphqlRequestModel<DeleteMessageVariables>, DeleteMessageResponseModel>(requestModel);

            return result;
        }

        private readonly GraphqlService graphqlService;
        private readonly ILogger logger;
        private readonly JsonSerializerOptions jsonSerializerOptions;
    }

    public class ListVariables
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
    }

    public class AddMessageVaraibles
    {
        public string Title { get; set; }
    }

    public class DeleteMessageVariables
    {
        public string Id { get; set; }
    }

    public class UpdateMessageVariables: AddMessageVaraibles
    {
        public string Id { get; set; }
    }

    public class MessageModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class MessagePostModel
    {
        public string Title { get; set; }
    }


    public class MessagesResponseModel
    {
        public IEnumerable<MessageModel> List { get; set; }

        public int TotalRecords { get; set; }
    }    

    public class MessageResponseModel
    {
        public MessageModel Item { get; set; }
    }

    public class DeleteMessageResponseModel
    {
        public int Result { get; set; }
    }
}
