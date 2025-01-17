﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps")]
    [ApiVersion("2.0")]
    [ApiController]
    public class Camps2Controller : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public Camps2Controller(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsAsync(includeTalks);
                var result = new
                {
                    Count = results.Count(),
                    Results = mapper.Map<CampModel[]>(results)
                };


                return Ok(result);
            }
            catch
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")] // this string is an extension of the route defined above in the class. The name used here has to match the method argument
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get11(string moniker)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker, true);

                if (result == null) return NotFound();

                return mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks)
        {
            try
            {
                var results = await repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existing = await repository.GetCampAsync(model.Moniker);

                if (existing != null)
                {
                    return BadRequest("Moniker in use");
                }

                var location = linkGenerator.GetPathByAction("Get", // action
                    "Camps", // controller
                    new
                    {
                        moniker = model.Moniker
                    });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = mapper.Map<Camp>(model);

                repository.Add(camp);

                if (await repository.SaveChangesAsync())
                {
                    return Created("", mapper.Map<CampModel>(camp));
                }

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await repository.GetCampAsync(moniker);

                if (oldCamp == null) return NotFound($"Could not find camp with moniker of {moniker}");

                mapper.Map(model, oldCamp);

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await repository.GetCampAsync(moniker);

                if (oldCamp == null) return NotFound($"Could not find camp with moniker of {moniker}");

                repository.Delete(oldCamp);

                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest("Failed to delete the camp");
        }
    }
}