using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

public class AzureStorageClient
{
    private readonly CloudTable refreshTokenTable;

    public AzureStorageClient(string connectionString)
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

        this.refreshTokenTable = tableClient.GetTableReference("refreshTokens");
        this.refreshTokenTable.CreateIfNotExists();
    }

    public async Task<string> GetRefreshToken(string clientId, string userId)
    {
        TableResult tableResult = await this.refreshTokenTable.ExecuteAsync(TableOperation.Retrieve<RefreshTokenEntity>(clientId, userId, new List<string>() { "RefreshToken" }));
        if (tableResult != null)
            return ((RefreshTokenEntity)tableResult.Result).RefreshToken;
        else
            return string.Empty;
    }

    public async Task<RefreshTokenEntity> AddOrUpdateRefreshToken(string audience, string userId, string refreshToken)
    {
        RefreshTokenEntity tokenEntity = new RefreshTokenEntity(audience, userId)
        {
            RefreshToken = refreshToken
        };

        TableResult tableResult = await this.refreshTokenTable.ExecuteAsync(TableOperation.InsertOrReplace(tokenEntity));

        return tableResult.Result as RefreshTokenEntity;
    }
}