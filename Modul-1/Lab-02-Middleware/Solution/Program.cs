// SOLUTION Task A: namespace of the UseRequestLogging() extension.
using CourseApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INotificationSender>(services =>
{
    var config = services.GetRequiredService<IConfiguration>();

    if (config["Notifications:Channel"] == "Sms")
        return new SmsSender();

    return new EmailSender();
});
builder.Services.AddScoped<OrderService>();

// Change only this line and restart for each experiment.
builder.Services.AddScoped<InstanceMarker>();
builder.Services.AddTransient<FirstConsumer>();
builder.Services.AddTransient<SecondConsumer>();

var app = builder.Build();

// SOLUTION Task A: outer, class-based middleware, registered once via the extension.
app.UseRequestLogging();

// SOLUTION Task B: inner inline middleware; compare the order before and after next().
app.Use(async (context, next) =>
{
    app.Logger.LogInformation("Inner before");
    await next(context);
    app.Logger.LogInformation("Inner after");
});

app.MapGet("/health", () => Results.Ok(new { Status = "OK" }));
app.MapGet("/pipeline-demo", () =>
{
    app.Logger.LogInformation("Endpoint");
    return Results.Ok(new { Message = "Pipeline demo" });
});
app.MapGet("/orders/confirm", (OrderService service) =>
{
    service.ConfirmOrder();
    return Results.Ok("Order confirmed");
});
app.MapGet("/orders/{id:int}/confirm", (int id, OrderService service) =>
{
    service.ConfirmOrder(id);
    return Results.Ok($"Order {id} confirmed");
});
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

public sealed class SmsSender : INotificationSender
{
    public void Send(string message) => Console.WriteLine($"SMS: {message}");
}

public sealed class OrderService(
    INotificationSender sender,
    ILogger<OrderService> logger)
{
    // Preserve the first lab endpoint and its behavior.
    public void ConfirmOrder()
    {
        logger.LogInformation("Order confirmation requested");
        sender.Send("Order confirmed");
    }

    // Used only to demonstrate a structured message template on slide 17.
    public void ConfirmOrder(int orderId)
    {
        logger.LogInformation("Order {OrderId} confirmed", orderId);
        sender.Send($"Order {orderId} confirmed");
    }
}

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
