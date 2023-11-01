using Gremlin.Net.Driver;

namespace GymApp.Infrastructure.Storage
{
    public static class GremlinWrapper
    {
        public static void Initialize(string key)
        {
            Key = key;
        }

        public static string Key { get; set; } = null!;

        public static async Task<ResultSet<dynamic>> CreateEntity(string entityType, string name)
        {
            var gremlinClient = CreateGremlinClient();
            var vertexLabel = entityType;
            var vertexType = entityType;
            var vertexName = name;
            return await gremlinClient.SubmitAsync<dynamic>($"g.addV('{vertexLabel}').property('entityType', '${vertexType}').property('name', '{vertexName}')");
        }

        private static GremlinClient CreateGremlinClient()
        {
            var gremlinServer = new GremlinServer(
                hostname: "gym-app-storage.gremlin.cosmosdb.azure.com",
                port: 443,
                enableSsl: true,
                username: "/dbs/gym-storage/colls/gym-data",
                password: Key);
            var gremlinClient = new GremlinClient(gremlinServer,
                new Gremlin.Net.Structure.IO.GraphSON.GraphSON2MessageSerializer());
            return gremlinClient;
        }

        public static async Task<ResultSet<dynamic>> LinkEntities(string firstEntity, string secondEntity, string relationshipName)
        {
            var gremlinClient = CreateGremlinClient();
            return await gremlinClient.SubmitAsync<dynamic>($"g.V().has('name', '{firstEntity}')" +
                                                     $".addE('{relationshipName}')" +
                                                     $".to(g.V().has('name', '{secondEntity}'))");
        }

        public static async Task<bool> VertexExistsAsync(string vertexName)
        {
            var gremlinClient = CreateGremlinClient();
            var query = $"g.V().has('name', '{vertexName}')";
            var resultSet = await gremlinClient.SubmitAsync<dynamic>(query);

            return resultSet.Count > 0;
        }

    }

}
