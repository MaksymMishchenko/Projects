using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;
using SportsStore.WebUI.Controllers;

namespace SportsStore.UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Can_Paginate()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product { ProductId = 1, Name = "P1" },
                new Product { ProductId = 2, Name = "P2" },
                new Product { ProductId = 3, Name = "P3" },
                new Product { ProductId = 4, Name = "P4" },
                new Product { ProductId = 5, Name = "P5" }
            }.AsQueryable());

            // Act
            var controller = new ProductController(null!, mock.Object);
            controller.PageSize = 3;

            var getViewResult = controller.List(2) as ViewResult;
            var productsEnum = (IEnumerable<Product>?)getViewResult?.ViewData.Model;

            // Assert
            var prodArray = productsEnum?.ToArray();
            Assert.That(prodArray?.Length == 2);
            Assert.That(prodArray[0].Name, Is.EqualTo("P4"));
            Assert.That(prodArray[1].Name, Is.EqualTo("P5"));
        }
    }
}