namespace Pulsar.Services.Identity.Migrations.Models;

public class GrantStoreModel
{
    public ObjectId Id { get; set; }

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    /// <value>
    /// The key.
    /// </value>
    public required string Key { get; set; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public required string Type { get; set; }

    /// <summary>
    /// Gets the subject identifier.
    /// </summary>
    /// <value>
    /// The subject identifier.
    /// </value>
    public required string SubjectId { get; set; }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    /// <value>
    /// The session identifier.
    /// </value>
    public required string SessionId { get; set; }

    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets the description the user assigned to the device being authorized.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    /// <value>
    /// The creation time.
    /// </value>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the expiration.
    /// </summary>
    /// <value>
    /// The expiration.
    /// </value>
    public DateTime? Expiration { get; set; }

    /// <summary>
    /// Gets or sets the consumed time.
    /// </summary>
    /// <value>
    /// The consumed time.
    /// </value>
    public DateTime? ConsumedTime { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public required string Data { get; set; }
}
