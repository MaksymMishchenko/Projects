using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.Models;
using SportsStore.WebUI.TagHelpers;

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

            var getViewResult = controller.List(null!, 2) as ViewResult;
            var productsEnum = (ProductsListViewModel?)getViewResult?.ViewData.Model;

            // Assert
            var prodArray = productsEnum?.Products?.ToArray();
            Assert.That(prodArray?.Length == 2);
            Assert.That(prodArray[0].Name, Is.EqualTo("P4"));
            Assert.That(prodArray[1].Name, Is.EqualTo("P5"));
        }

        [Test]
        public void Can_Generate_Page_Links()
        {
            // Arrange
            var tagHelper = new PaginationTagHelper
            {
                PageModel = new PagingInfo
                {
                    TotalItems = 10,
                    ItemsPerPage = 2,
                    CurrentPage = 1
                }
            };

            var tagHelperContext = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput(
                "div",
                new TagHelperAttributeList(),
                (_, __) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(tagHelperContext, tagHelperOutput);

            // Assert
            var content = tagHelperOutput.Content.GetContent();
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Contains.Substring("<a class=\"selected\" href=\"?page=1\">1</a>"));
            Assert.That(content, Contains.Substring("<a href=\"?page=2\">2</a>"));
        }

        [Test]
        public void Can_Send_Pagination_ViewModel()
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

            var controller = new ProductController(null!, mock.Object);
            controller.PageSize = 3;

            // Act
            var result = controller.List(null!, 2) as ViewResult;
            var data = (ProductsListViewModel?)result?.ViewData.Model;

            // Assert
            var pageInfo = data?.PagingInfo;
            Assert.That(pageInfo?.CurrentPage, Is.EqualTo(2));
            Assert.That(pageInfo?.ItemsPerPage, Is.EqualTo(3));
            Assert.That(pageInfo?.TotalItems, Is.EqualTo(5));
            Assert.That(pageInfo?.TotalPages, Is.EqualTo(2));
        }

        [Test]
        public void Can_Filter_Products()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product { ProductId = 1, Name = "P1", Category = "Cat1" },
                new Product { ProductId = 2, Name = "P2", Category = "Cat2" },
                new Product { ProductId = 3, Name = "P3", Category = "Cat1" },
                new Product { ProductId = 4, Name = "P4", Category = "Cat2" },
                new Product { ProductId = 5, Name = "P5", Category = "Cat3" }
            }.AsQueryable());

            var controller = new ProductController(null!, mock.Object);
            controller.PageSize = 3;

            // Action

            var result = controller.List("Cat2", 1) as ViewResult;
            var prodArray = ((ProductsListViewModel?)result?.ViewData.Model)?.Products?.ToArray();

            // Assert
            Assert.That(prodArray?.Length == 2);
            Assert.That(prodArray[0].Name == "P2" && prodArray[0].Category == "Cat2");
            Assert.That(prodArray[1].Name == "P4" && prodArray[1].Category == "Cat2");
        }
    }
}