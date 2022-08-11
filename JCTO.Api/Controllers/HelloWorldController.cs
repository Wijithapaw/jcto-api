using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JCTO.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloWorldController : ControllerBase
    {
        private readonly ILogger<HelloWorldController> _logger;
        private readonly IUserService _userService;

        public HelloWorldController(ILogger<HelloWorldController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("{name}")]
        public string Get(string name)
        {
            if (name == "1") throw new JCTOValidationException("Validation Error");

            if (name == "2") throw new JCTOConcurrencyException("Order");

            return "Hello World! - " + name;
        }
    }
}