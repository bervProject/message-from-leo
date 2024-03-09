using MessageFromLeo.Models;
using MongoDB.Bson;

namespace MessageFromLeo.Mongo.Repositories;

public interface IMessageRepository
{
    Message Create(Message input);
    Message Update(Message input);
    void Delete(ObjectId objectId);
    List<Message> Read(string? to = null);
    Message ReadById(string id);
    string ReadByIdAndPass(ObjectId id, string pass);
    List<Message> ReadByTo(string to, bool includePassword = false);
    Task CreateIndexAsync();
}