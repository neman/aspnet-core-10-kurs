var builder = WebApplication.CreateBuilder(args);

// SOLUTION Task 2: factory reads Notifications:Channel and picks the sender.
builder.Services.AddScoped<INotificationSender>(services =>
{
    var config = services.GetRequiredService<IConfiguration>();

    if (config["Notifications:Channel"] == "Sms")
    {
        return new SmsSender();
    }

    return new EmailSender();
});
builder.Services.AddScoped<OrderService>();

// SOLUTION Task 4: InstanceMarker plus two transient consumers.
// Change only the InstanceMarker line and restart for each experiment.
builder.Services.AddScoped<InstanceMarker>();
builder.Services.AddTransient<FirstConsumer>();
builder.Services.AddTransient<SecondConsumer>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { Status = "OK" }));
app.MapGet("/orders/confirm", (OrderService service) =>
{
    service.ConfirmOrder();
    return Results.Ok("Order confirmed");
});
// SOLUTION Task 4: both consumers come from the same request scope.
app.MapGet("/di/lifetime", (FirstConsumer first, SecondConsumer second) =>
    Results.Ok(new
    {
        First = first.MarkerId,
        Second = second.MarkerId,
        Same = first.MarkerId == second.MarkerId
    }));

app.Run();

public interface INotificationSender
{
    void Send(string message);
}

public sealed class EmailSender : INotificationSender
{
    public void Send(string message) => Console.WriteLine($"Email: {message}");
}

// SOLUTION Task 1: second implementation; OrderService stays unchanged.
public sealed class SmsSender : INotificationSender
{
    public void Send(string message) => Console.WriteLine($"SMS: {message}");
}

public sealed class OrderService(INotificationSender sender)
{
    public void ConfirmOrder() => sender.Send("Order confirmed");
}

// SOLUTION Task 4: marker and two consumers with constructor injection.
public sealed class InstanceMarker
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class FirstConsumer(InstanceMarker marker)
{
    public Guid MarkerId => marker.Id;
}

public sealed class SecondConsumer(InstanceMarker marker)
{
    public Guid MarkerId => marker.Id;
}
