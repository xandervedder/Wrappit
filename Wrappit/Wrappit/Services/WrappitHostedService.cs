using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wrappit.Messaging;
using Wrappit.Routing;

namespace Wrappit.Services;

internal class WrappitHostedService : IHostedService
{
    private readonly IBasicReciever _receiver;
    private readonly IWrappitRouter _router;
    private readonly ILogger<WrappitHostedService> _logger;

    public WrappitHostedService(IBasicReciever receiver, IWrappitRouter router, ILogger<WrappitHostedService> logger)
    {
        _receiver = receiver;
        _router = router;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(@"
                          /\
                         //\\       /\
                        /'  \\     //\\
                             \\   //  `\
                              \\ //
                             .-'^'-.
                           .' a___a `.
                          ==  (___)  ==
                           '. ._I_. .'
                       ____/.`-----'.\____
                      [###(__)#####(__)###]
                       ~~|#############|~~
                         |#############|
                         ~~~~~~~~~~~~~~~
                                                                       
 _|          _|                                          _|    _|      
 _|          _|  _|  _|_|    _|_|_|  _|_|_|    _|_|_|        _|_|_|_|  
 _|    _|    _|  _|_|      _|    _|  _|    _|  _|    _|  _|    _|      
   _|  _|  _|    _|        _|    _|  _|    _|  _|    _|  _|    _|      
     _|  _|      _|          _|_|_|  _|_|_|    _|_|_|    _|      _|_|  
                                     _|        _|                      
                                     _|        _|    

    -- Built by Xander Vedder (Professional Nitpicker).
");

        if (!_router.Topics.Any())
        {
            _logger.LogInformation("No Listeners registered.");
            return Task.CompletedTask;
        }
        
        _receiver.SetUpQueue(_router.Topics);
        _receiver.StartRecieving(_router.Route);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Wrappit is shutting down...");
        _receiver.Dispose();
        return Task.CompletedTask;
    }
}
