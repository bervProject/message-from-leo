

using MessageFromLeo.Mongo.Repositories;

namespace MessageFromLeo.Web;

public class ManageIndex : IHostedService
{
  private IMessageRepository _messageRepository;
  public ManageIndex(IMessageRepository messageRepository)
  {
    _messageRepository = messageRepository;
  }
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await _messageRepository.CreateIndexAsync();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}