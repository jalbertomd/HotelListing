using AutoMapper;
using HotelListing.Contracts;
using HotelListing.Data;
using HotelListing.Models;
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
                
        [HttpGet("{id:int}", Name = "GetHotel")]
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

        [Authorize (Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO hotelDTO)
        {
            var location = GetControllerActionNames();

            if (!ModelState.IsValid)
            {
                _logger.LogError($"{location}: Invalid POST attempt");
                return BadRequest(ModelState);
            }

            try
            {
                var hotel = _mapper.Map<Hotel>(hotelDTO);
                await _unitOfWork.Hotels.Insert(hotel);
                await _unitOfWork.Save();

                return CreatedAtRoute("GetHotel", new { id = hotel.Id }, hotel);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [Authorize (Roles = "Administrator")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelDTO hotelDTO)
        {
            var location = GetControllerActionNames();

            if (!ModelState.IsValid || id < 1)
            {
                _logger.LogError($"{location}: Invalid UPDATE attempt");
                return BadRequest(ModelState);
            }

            try
            {
                var hotel = await _unitOfWork.Hotels.Get(h => h.Id == id);

                if (hotel == null)
                {
                    _logger.LogError($"{location}: Invalid UPDATE attempt");
                    return BadRequest("Hotel not found");
                }


                _mapper.Map(hotelDTO, hotel);

                _unitOfWork.Hotels.Update(hotel);
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
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var location = GetControllerActionNames();

            if (id < 1)
            {
                _logger.LogError($"{location}: Invalid DELETE attempt");
                return BadRequest(ModelState);
            }

            try
            {
                var hotel = await _unitOfWork.Hotels.Get(h => h.Id == id);

                if (hotel == null)
                {
                    _logger.LogError($"{location}: Invalid DELETE attempt");
                    return BadRequest("Hotel not found");
                }

                await _unitOfWork.Hotels.Delete(id);
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
