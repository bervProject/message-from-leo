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
  List<Message> ReadByTo(string to);
  Task CreateIndexAsync();
}