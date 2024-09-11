using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SagaManagementController : ControllerBase
{
    private readonly ISagaManagementService _sagaManagementService;

    public SagaManagementController(ISagaManagementService sagaManagementService)
    {
        _sagaManagementService = sagaManagementService;
    }

    [HttpGet("{correlationId}")]
    public async Task<IActionResult> GetSagaState(Guid correlationId)
    {
        OrderProcessState sagaState = await _sagaManagementService.GetSagaStateByCorrelationIdAsync(correlationId);

        if (sagaState == null)
        {
            return NotFound(new { Message = $"Saga state with CorrelationId {correlationId} not found." });
        }

        return Ok(sagaState);
    }
}