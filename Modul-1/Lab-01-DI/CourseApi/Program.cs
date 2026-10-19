var builder = WebApplication.CreateBuilder(args);

// TODO Task 2: Replace this mapping with a factory that reads Notifications:Channel.
builder.Services.AddScoped<INotificationSender, EmailSender>();
builder.Services.AddScoped<OrderService>();

// TODO Task 4: Register InstanceMarker (choose one lifetime), FirstConsumer and SecondConsumer.

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { Status = "OK" }));
app.MapGet("/orders/confirm", (OrderService service) =>
{
    service.ConfirmOrder();
    return Results.Ok("Order confirmed");
});

// TODO Task 4: Add GET /di/lifetime with two consumer parameters and compare their MarkerId.

app.Run();

public interface INotificationSender
{
    void Send(string message);
}

public sealed class EmailSender : INotificationSender
{
    public void Send(string message) => Console.WriteLine($"Email: {message}");
}

// TODO Task 1: Add SmsSender implementing INotificationSender.

public sealed class OrderService(INotificationSender sender)
{
    public void ConfirmOrder() => sender.Send("Order confirmed");
}

// TODO Task 4: Add InstanceMarker, FirstConsumer and SecondConsumer (constructor injection).
