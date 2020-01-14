using Microsoft.AspNetCore.Mvc;
using NetCore.Data;
using NetCore.Data.Abstractions;
using NetCore.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRepository<Item> _repository;

        public ValuesController(IRepository<Item> repository)
        {
            _repository = repository;
        }

        // GET: api/TodoItems/5
        [HttpGet("")]
        public async Task<ActionResult<Item>> GetItems()
        {
            var list = await _repository.ListAsync(SpecificationFactory.Create<Item>());
            return Ok(list);
        }

        [HttpGet("{partition}/{id}")]
        public async Task<ActionResult<Item>> AddItem(string partition, string id)
        {
            var list = await _repository.CreateAsync(new Item
            {
                Id = id,
                PartitionKey = partition
            });
            return Ok(list);
        }
    }
}
