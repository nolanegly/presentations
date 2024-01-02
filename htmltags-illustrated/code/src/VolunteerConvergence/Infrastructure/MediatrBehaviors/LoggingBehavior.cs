using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace VolunteerConvergence.Infrastructure.MediatrBehaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public LoggingBehavior(ILogger<TRequest> logger)
            => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            using (_logger.BeginScope(request))
            {
                var requestType = request.GetType().FullName ?? "(null)";
                _logger.LogDebug("Received Mediatr request {MediatrRequestType} as {@MediatrRequest}",
                    requestType, request);

                _logger.LogDebug("Calling Mediatr handler...");
                var response = await next();
                _logger.LogDebug("Returning Mediatr response {@MediatrResponse}", response);
                return response;
            }
        }
    }
}
