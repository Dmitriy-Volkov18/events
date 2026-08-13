using System;
using System.Threading.Tasks;
using Application.Comments;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;
        public ChatHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendComment(Create.Command command)
        {
            var comment = await _mediator.Send(command);

            await Clients.Group(command.ActivityId.ToString())
                .SendAsync("ReceiveComment", comment.Value);
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext == null)
            {
                throw new HubException("HTTP context is unavailable.");
            }

            var activityId = httpContext.Request.Query["activityId"].ToString();

            if (!Guid.TryParse(activityId, out var parsedActivityId))
            {
                throw new HubException("Invalid activityId.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, parsedActivityId.ToString());

            var result = await _mediator.Send(new List.Query { ActivityId = parsedActivityId });

            await Clients.Caller.SendAsync("LoadComments", result.Value);
        }
    }
}