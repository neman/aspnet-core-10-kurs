using Microsoft.AspNetCore.SignalR;

namespace HeroRegistry.Hubs;

// No methods: only the server sends messages, through IHubContext<HeroHub>.
public sealed class HeroHub : Hub;
