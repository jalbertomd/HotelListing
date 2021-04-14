using AutoMapper;
using HotelListing.Contracts;
using HotelListing.Data;
using HotelListing.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CountryController> _logger;
        private readonly IMapper _mapper;

        public CountryController(IUnitOfWork unitOfWork, ILogger<CountryController> logger, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        //[ResponseCache(CacheProfileName = "120SecondsDuration")]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries([FromQuery] RequestParams requestParams)
        {
            var location = GetControllerActionNames();

            try
            {
                var countries = await _unitOfWork.Countries.GetPagedList(requestParams);
                var result = _mapper.Map<IList<CountryDTO>>(countries);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [HttpGet("{id:int}", Name = "GetCountry")]
        //[ResponseCache(CacheProfileName = "120SecondsDuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountry(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                var country = await _unitOfWork.Countries.Get(c => c.Id == id, new List<string> { "Hotels" });
                var result = _mapper.Map<CountryDTO>(country);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDTO countryDTO)
        {
            var location = GetControllerActionNames();

            if (!ModelState.IsValid)
            {
                _logger.LogError($"{location}: Invalid POST attempt");
                return BadRequest(ModelState);
            }

            try
            {
                var country = _mapper.Map<Country>(countryDTO);
                await _unitOfWork.Countries.Insert(country);
                await _unitOfWork.Save();

                return CreatedAtRoute("GetCountry", new { id = country.Id }, country);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDTO countryDTO)
        {
            var location = GetControllerActionNames();

            if (!ModelState.IsValid || id < 1)
            {
                _logger.LogError($"{location}: Invalid UPDATE attempt");
                return BadRequest(ModelState);
            }

            try
            {
                var country = await _unitOfWork.Countries.Get(c => c.Id == id);

                if (country == null)
                {
                    _logger.LogError($"{location}: Invalid UPDATE attempt");
                    return BadRequest("Country not found");
                }


                _mapper.Map(countryDTO, country);

                _unitOfWork.Countries.Update(country);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var location = GetControllerActionNames();

            if (id < 1)
            {
                _logger.LogError($"{location}: Invalid DELETE attempt");
                return BadRequest(ModelState);
            }

            try
            {
                var country = await _unitOfWork.Countries.Get(c => c.Id == id);

                if (country == null)
                {
                    _logger.LogError($"{location}: Invalid DELETE attempt");
                    return BadRequest("Country not found");
                }

                await _unitOfWork.Countries.Delete(id);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        private ObjectResult InternalError(string message, Exception ex = null)
        {
            if (ex != null)
            {
                _logger.LogError(ex, message);
                return StatusCode(500, ex.Message);
            }
            else
            {
                _logger.LogError(message);
                return StatusCode(500, message);
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

    }
}
