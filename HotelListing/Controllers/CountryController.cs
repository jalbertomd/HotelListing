using AutoMapper;
using HotelListing.Contracts;
using HotelListing.Models;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries()
        {
            var location = GetControllerActionNames();

            try
            {
                var countries = await _unitOfWork.Countries.GetAll();
                var result = _mapper.Map<IList<CountryDTO>>(countries);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [HttpGet("{id:int}")]
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
