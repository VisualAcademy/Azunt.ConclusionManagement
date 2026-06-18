using Azunt.ConclusionManagement;
using Microsoft.AspNetCore.Mvc;

namespace Azunt.Web.Components.Pages.Conclusions.Apis;

[ApiController]
[Route("api/[controller]")]
public class ConclusionApiController : ControllerBase
{
    private readonly IConclusionRepository _repository;

    public ConclusionApiController(IConclusionRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conclusion>>> GetConclusions()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Conclusion>> GetConclusion(long id)
    {
        var model = await _repository.GetByIdAsync(id);

        if (model.Id == 0)
        {
            return NotFound();
        }

        return Ok(model);
    }

    [HttpPost]
    public async Task<ActionResult<Conclusion>> PostConclusion(Conclusion model)
    {
        model.CreatedAt = DateTimeOffset.UtcNow;
        var result = await _repository.AddAsync(model);
        return CreatedAtAction(nameof(GetConclusion), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutConclusion(long id, Conclusion model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var success = await _repository.UpdateAsync(model);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteConclusion(long id)
    {
        var success = await _repository.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
