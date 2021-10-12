using System.Collections.Generic;

namespace Sample.Services
{
    public class GraphqlRequestModel
    {
        public GraphqlRequestModel()
        {

        }

        public string Query { get; set; }
    }

    public class GraphqlRequestModel<TVariables>: GraphqlRequestModel where TVariables : class
    {
        public TVariables Variables { get; set; }
    }
}
