using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Backend.Persistence.Model;
using PuppyAdoption.Backend.Persistence.Util;
using PuppyAdoption.Backend.Services;
using PuppyAdoption.Backend.Util;
using PuppyAdoption.Backend.Validators;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Backend.Controllers;

/// <summary>
///     API controller for managing puppies
/// </summary>
[Route("api")]
public sealed class PuppyController(
    ITransactionProvider transaction,
    IPuppyService service,
    ILogger<PuppyController> logger,
    IClock clock) : BaseController
{
    /// <summary>
    ///     Returns basic information about all puppies of a shelter
    /// </summary>
    /// <param name="shelterId">The shelter id from the route</param>
    [HttpGet]
    [Route("shelters/{shelterId:int}/puppies")]
    [ProducesResponseType<PuppyInformationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PuppyInformationResponse>> GetAllPuppies([FromRoute] int shelterId)
    {
        if (shelterId <= 0)
        {
            logger.LogWarning("Invalid shelter id {Id} given", shelterId);

            return BadRequest();
        }

        OneOf<Success<IReadOnlyCollection<PuppyInformation>>, NotFound> result
            = await service.GetAllPuppiesAsync(shelterId);

        return result.Match<ActionResult<PuppyInformationResponse>>(success =>
        {
            logger.LogTrace("Successfully fetched {Count} puppies of shelter {ShelterId}", success.Value.Count,
                            shelterId);

            return Ok(new PuppyInformationResponse(success.Value.Select(p => p.ToDto()).ToList()));
        }, notFound =>
        {
            logger.LogWarning("Shelter with id {Id} does not exist", shelterId);

            return NotFound();
        });
    }

    /// <summary>
    ///     Admits a new puppy to a shelter
    /// </summary>
    /// <param name="shelterId">The shelter id from the route</param>
    /// <param name="request">The request containing the values of the puppy</param>
    [HttpPost]
    [Route("shelters/{shelterId:int}/puppies")]
    [ProducesResponseType<PuppyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> AdmitPuppy([FromRoute] int shelterId, [FromBody] AdmitPuppyRequest request)
    {
        if (!ValidateRequest(request, new AdmitPuppyRequestValidator(clock), out string[]? validationErrors))
        {
            logger.LogWarning("AdmitPuppy request validation failed: {ValidationErrors}",
                              string.Join(", ", validationErrors ?? []));

            return BadRequest(validationErrors);
        }

        if (shelterId <= 0)
        {
            logger.LogWarning("Invalid shelter id {Id} given", shelterId);

            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success<Puppy>, NotFound> result = await service.AdmitPuppyAsync(shelterId, request.Name,
                                                                                   request.Breed,
                                                                                   request.DateOfBirth,
                                                                                   request.ImageData);

            return await result.Match<Task<IActionResult>>(async success =>
            {
                await transaction.CommitAsync();
                logger.LogInformation("Successfully admitted puppy with id {Id}", success.Value.Id);

                return CreatedAtAction(nameof(GetPuppyById), new { id = success.Value.Id }, success.Value.ToDto());
            }, async notFound =>
            {
                await transaction.RollbackAsync();
                logger.LogWarning("Shelter with id {Id} not found", shelterId);

                return NotFound();
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occured while trying to admit a puppy");

            return Problem();
        }
    }

    /// <summary>
    ///     Returns a puppy by its id
    /// </summary>
    /// <param name="id">The puppy id from the route</param>
    [HttpGet]
    [Route("puppies/{id:int}")]
    [ProducesResponseType<PuppyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PuppyDto>> GetPuppyById([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid puppy id {Id} given", id);

            return BadRequest();
        }

        OneOf<Puppy, NotFound> result = await service.GetPuppyByIdAsync(id);

        return result.Match<ActionResult<PuppyDto>>(puppy =>
        {
            logger.LogTrace("Successfully found puppy with id {Id}", id);

            return Ok(puppy.ToDto());
        }, notFound =>
        {
            logger.LogWarning("Didn't find puppy with id {Id}", id);

            return NotFound();
        });
    }

    /// <summary>
    ///     Deletes a puppy by its id
    /// </summary>
    /// <param name="id">The puppy id from the route</param>
    [HttpDelete]
    [Route("puppies/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> DeletePuppy([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid puppy id {Id} given", id);

            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success, NotFound> result = await service.DeletePuppyAsync(id);

            return await result.Match<Task<IActionResult>>(async success =>
            {
                await transaction.CommitAsync();
                logger.LogInformation("Successfully deleted puppy with id {Id}", id);

                return NoContent();
            }, async notFound =>
            {
                await transaction.RollbackAsync();
                logger.LogWarning("Didn't find puppy with id {Id}", id);

                return NotFound();
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occured while deleting a puppy");

            return Problem();
        }
    }

    /// <summary>
    ///     Adopts a puppy
    /// </summary>
    /// <param name="id">The puppy id from the route</param>
    /// <param name="request">The request containing the name of the adopter</param>
    [HttpPost]
    [Route("puppies/{id:int}/adoption")]
    [ProducesResponseType<AdoptPuppyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<ActionResult<AdoptPuppyResponse>> AdoptPuppy([FromRoute] int id,
                                                                        [FromBody] AdoptPuppyRequest request)
    {
        if (!ValidateRequest(request, new AdoptPuppyRequestValidator(), out string[]? validationErrors))
        {
            logger.LogWarning("AdoptPuppy request validation failed: {ValidationErrors}",
                              string.Join(", ", validationErrors ?? []));

            return BadRequest(validationErrors);
        }

        if (id <= 0)
        {
            logger.LogWarning("Invalid puppy id {Id} given", id);

            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success<LocalDate>, NotFound, IPuppyService.AlreadyAdopted> result
                = await service.AdoptPuppyAsync(id, request.AdopterName);

            return await result.Match<Task<ActionResult<AdoptPuppyResponse>>>(async success =>
            {
                await transaction.CommitAsync();
                logger.LogInformation("Successfully adopted puppy with id {Id} at {Date}", id, success.Value);

                return Ok(new AdoptPuppyResponse(success.Value));
            }, async notFound =>
            {
                await transaction.RollbackAsync();
                logger.LogWarning("Puppy with id {Id} not found", id);

                return NotFound();
            }, async alreadyAdopted =>
            {
                await transaction.RollbackAsync();
                logger.LogWarning("Puppy {Id} is already adopted", id);

                return Conflict();
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occured while trying to adopt a puppy");

            return Problem();
        }
    }
}
