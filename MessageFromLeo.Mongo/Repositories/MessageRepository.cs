using MessageFromLeo.Models;
using MessageFromLeo.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MessageFromLeo.Mongo.Repositories;

public class MessageRepository : IMessageRepository
{
    private IMongoCollection<Message> _messages;
    private ILogger<MessageRepository> _logger;
    public MessageRepository(MongoClient client, ILogger<MessageRepository> logger)
    {
        _messages = client.GetDatabase("mfl").GetCollection<Message>("messages");
        _logger = logger;
    }

    public Message Create(Message input)
    {
        var createdDate = DateTime.UtcNow;
        input.CreatedAt = createdDate;
        input.UpdatedAt = createdDate;
        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            var taskEncrypt = Crypto.EncryptAsync(input.Password, input.Content);
            taskEncrypt.Wait();
            input.Content = BitConverter.ToString(taskEncrypt.Result);
            input.Password = Crypto.Hash(input.Password);
        }
        _messages.InsertOne(input);
        _logger.LogDebug("Message created with id {}", input.Id);
        return input;
    }

    public Message Update(Message input)
    {
        var updatedDate = DateTime.UtcNow;
        var filterDefinition = Builders<Message>.Filter;
        var filter = filterDefinition.Eq(data => data.Id, input.Id) & filterDefinition.Eq(data => data.To, input.To);
        var updateDefinition = Builders<Message>.Update
          .Set(data => data.UpdatedAt, updatedDate);
        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            // not recommended
            var taskEncrypt = Crypto.EncryptAsync(input.Password, input.Content);
            taskEncrypt.Wait();
            var newContent = BitConverter.ToString(taskEncrypt.Result);
            var newPassword = Crypto.Hash(input.Password);
            updateDefinition.Set(data => data.Content, newContent);
            updateDefinition.Set(data => data.Password, newPassword);
        }
        else
        {
            updateDefinition.Set(data => data.Content, input.Content);
        }
        _logger.LogDebug("Updating {}.", input.Id);
        var result = _messages.UpdateOne(filter, updateDefinition);
        _logger.LogDebug("Executed to update {}. Total matched: {}.", input.Id, result.MatchedCount);
        return input;
    }

    public void Delete(ObjectId objectId)
    {
        var filterDefinition = Builders<Message>.Filter.Eq(data => data.Id, objectId);
        _logger.LogDebug("Deleting {}.", objectId);
        var deleteResult = _messages.DeleteOne(filterDefinition);
        _logger.LogDebug("Message {} deleted. Deleted count: {}", objectId, deleteResult.DeletedCount);
    }

    public List<Message> Read(string? to = null)
    {
        var filters = Builders<Message>.Filter;
        var sorts = Builders<Message>.Sort.Descending(data => data.CreatedAt);
        FilterDefinition<Message>? filter;
        if (!string.IsNullOrWhiteSpace(to))
        {
            filter = filters.Text(to);
        }
        else
        {
            filter = filters.Empty;
        }
        return _messages.Find(filter)
                .Sort(sorts)
                .ToList();
    }

    public Message ReadById(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<Message>.Filter.Eq(data => data.Id, objectId);
        _logger.LogDebug("Reading {}.", id);
        return _messages.Find(filter).SingleOrDefault();
    }

    public string ReadByIdAndPass(ObjectId id, string pass)
    {
        try
        {
            var filter = Builders<Message>.Filter.Eq(data => data.Id, id);
            _logger.LogDebug("Reading {}.", id);
            var data = _messages.Find(filter).SingleOrDefault();
            if (data != null)
            {
                var verify = Crypto.Verify(pass, data.Password);
                if (!verify)
                {
                    return string.Empty;
                }
                var dataEncrypted = data.Content.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
                var decryptTask = Crypto.DecryptAsync(dataEncrypted, pass);
                decryptTask.Wait();
                return decryptTask.Result;
            }
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }

    }

    public List<Message> ReadByTo(string to, bool includePassword = false)
    {
        var filter = Builders<Message>.Filter;
        var basic = filter.Eq(data => data.To, to);
        if (!includePassword)
        {
            basic = filter.And(basic, filter.Where(x => string.IsNullOrEmpty(x.Password)));
        }
        _logger.LogDebug("Reading {}.", to);
        return _messages.Find(basic).ToList();
    }

    public async Task CreateIndexAsync()
    {
        var indexKeysDefinition = Builders<Message>.IndexKeys.Ascending(data => data.To);
        var createIndexModel = new CreateIndexModel<Message>(indexKeysDefinition);
        var result = await _messages.Indexes.CreateOneAsync(createIndexModel);
        _logger.LogDebug("Index created: {}", result);
    }
}

