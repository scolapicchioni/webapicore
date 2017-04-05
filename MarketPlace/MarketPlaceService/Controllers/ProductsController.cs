using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MarketPlaceService.Models;
using MarketPlaceService.Data;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;

namespace MarketPlaceService.Controllers {
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : Controller {
        private readonly IProductsRepository _ProductsRepository;
        IAuthorizationService _authorizationService;

        public ProductsController(IProductsRepository ProductsRepository, IAuthorizationService authorizationService) {
            _ProductsRepository = ProductsRepository;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        ///requires using Microsoft.AspNetCore.Http;
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        //requires using Swashbuckle.AspNetCore.SwaggerGen;
        [SwaggerOperation("getProducts")]
        public IEnumerable<Product> GetAll() {
            return _ProductsRepository.GetAll();
        }

        [HttpGet("{id}", Name = "GetProduct")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [SwaggerOperation("getProduct")]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id) {
            var item = _ProductsRepository.Find(id);
            if (item == null) {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [SwaggerOperation("createProduct")]
        //requires using Microsoft.AspNetCore.Authorization;
        [Authorize]
        public IActionResult Create([FromBody] Product product) {
            if (product == null || product.UserName != User.Identity.Name) {
                return BadRequest();
            }

            _ProductsRepository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [SwaggerOperation("updateProduct")]
        public IActionResult Update(int id, [FromBody] Product product) {
            if (product == null || product.Id != id) {
                return BadRequest();
            }

            var original = _ProductsRepository.Find(id);
            if (original == null) {
                return NotFound();
            }

            if (!_authorizationService.AuthorizeAsync(User, product, "ProductOwner").Result) {
                return new ChallengeResult();
            }

            original.Name = product.Name;
            original.Description = product.Description;
            original.Price = product.Price;

            _ProductsRepository.Update(original);
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [SwaggerOperation("deleteProduct")]
        public IActionResult Delete(int id) {
            var product = _ProductsRepository.Find(id);
            if (product == null) {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, product, "ProductOwner").Result) {
                return new ChallengeResult();
            }

            _ProductsRepository.Remove(id);
            return new NoContentResult();
        }
    }
}