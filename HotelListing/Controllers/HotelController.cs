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
    public class HotelController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HotelController> _logger;
        private readonly IMapper _mapper;

        public HotelController(IUnitOfWork unitOfWork, ILogger<HotelController> logger, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotels()
        {
            var location = GetControllerActionNames();

            try
            {
                var hotels = await _unitOfWork.Hotels.GetAll();
                var result = _mapper.Map<IList<HotelDTO>>(hotels);

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
        public async Task<IActionResult> GetHotel(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                var hotel = await _unitOfWork.Hotels.Get(c => c.Id == id, new List<string> { "Country" });
                var result = _mapper.Map<HotelDTO>(hotel);

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
