using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductionPlant.Application;
using ProductionPlant.Domain;

namespace ProductionPlant.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class EvaluateController : ControllerBase
{
    private readonly EvaluationService _service;

    public EvaluateController(EvaluationService service) => _service = service;

    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(EvaluateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EvaluateOrderResponse>> Evaluate(
        [FromBody] JsonElement request,
        CancellationToken cancellationToken)
    {
        if (request.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return BadRequest(new { message = "Order JSON is required." });

        Order? order;
        try
        {
            order = JsonSerializer.Deserialize<Order>(
                request.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            return BadRequest(new { message = "Invalid order JSON.", detail = ex.Message });
        }

        if (order is null)
            return BadRequest(new { message = "Order JSON could not be parsed." });

        var validationErrors = Validate(order);
        if (validationErrors.Count > 0)
            return BadRequest(new { message = "Order validation failed.", errors = validationErrors });

        var result = await _service.EvaluateAsync(
            order,
            request.GetRawText(),
            cancellationToken);

        return Ok(result);
    }

    private static List<string> Validate(Order order)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(order.OrderId)) errors.Add("orderId is required.");
        if (string.IsNullOrWhiteSpace(order.PublisherNumber)) errors.Add("publisherNumber is required.");
        if (string.IsNullOrWhiteSpace(order.OrderMethod)) errors.Add("orderMethod is required.");
        if (order.Items.Count == 0) errors.Add("At least one item is required.");
        return errors;
    }
}
