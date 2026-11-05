using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Backend.Persistence.Model;
using PuppyAdoption.Backend.Persistence.Util;
using PuppyAdoption.Backend.Services;
using PuppyAdoption.Backend.Util;
using PuppyAdoption.Shared;
using PuppyAdoption.Shared.Validators;

namespace PuppyAdoption.Backend.Controllers;

/// <summary>
///     API controller for managing shelters
/// </summary>
[Route("api/shelters")]
public sealed class ShelterController(
    ITransactionProvider transaction,
    IShelterService shelterService,
    ILogger<ShelterController> logger) : BaseController
{
    /// <summary>
    ///     Returns all shelters
    /// </summary>
    [HttpGet]
    [Route("")]
    [ProducesResponseType<ShelterListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<ShelterListResponse>> GetAllShelters()
    {
        IReadOnlyCollection<Shelter> shelters = await shelterService.GetAllSheltersAsync();
        logger.LogDebug("Received {Count} shelters", shelters.Count);

        return Ok(new ShelterListResponse(shelters.Select(s => s.ToDto()).ToList()));
    }

    /// <summary>
    ///     Returns a shelter by its id
    /// </summary>
    /// <param name="id">The shelter id from the route</param>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType<ShelterDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<ShelterDto>> GetShelterById([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid shelter id {Id} given", id);

            return BadRequest();
        }

        var result = await shelterService.GetShelterByIdAsync(id);

        return result.Match<ActionResult<ShelterDto>>(shelter =>
        {
            logger.LogTrace("Successfully got shelter with id {Id}", id);

            return Ok(shelter.ToDto());
        }, notFound =>
        {
            logger.LogWarning("Didn't find shelter with id {Id}", id);

            return NotFound();
        });
    }

    /// <summary>
    ///     Creates a new shelter
    /// </summary>
    /// <param name="request">The request containing the name of the shelter</param>
    [HttpPost]
    [Route("")]
    [ProducesResponseType<ShelterDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> CreateShelter([FromBody] CreateShelterRequest request)
    {
        if (!ValidateRequest<CreateShelterRequestValidator, CreateShelterRequest>(request,
                 out string[]? validationErrors))
        {
            logger.LogWarning("CreateShelter request validation failed: {ValidationErrors}",
                              string.Join(", ", validationErrors ?? []));

            return BadRequest(validationErrors);
        }

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success<Shelter>, IShelterService.NameAlreadyExists> result
                = await shelterService.CreateNewShelterAsync(request.Name);

            return await result.Match<Task<IActionResult>>(async success =>
            {
                await transaction.CommitAsync();
                logger.LogInformation("Successfully inserted shelter: \"{Name}\"", success.Value.Name);

                return CreatedAtAction(nameof(GetShelterById), new { id = success.Value.Id }, success.Value.ToDto());
            }, async nameAlreadyExists =>
            {
                await transaction.RollbackAsync();
                logger.LogWarning("Name {Name} already exists", request.Name);

                return Conflict();
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occured while trying to create a shelter");

            return Problem();
        }
    }
}
