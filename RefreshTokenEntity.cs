using Microsoft.Azure.Cosmos.Table;

public class RefreshTokenEntity : TableEntity
{
    public RefreshTokenEntity()
    {

    }
    public string RefreshToken { get; set; }

    public RefreshTokenEntity(string audience, string userId)
    {
        this.PartitionKey = audience;
        this.RowKey = userId;
    }
}