using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;
using SportsStore.WebUI.Components;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.HtmlHelpers;
using SportsStore.WebUI.Models;

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
            var controller = new ProductController(mock.Object);
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
            var htmlHelper = new Mock<IHtmlHelper>();
            // Arrange
            PagingInfo info = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            Func<int, string> pageUrl = x => $"/Page/{x}";

            // Act
            var result = PagingHelpers.PagingLinks(htmlHelper.Object, info, pageUrl);

            // Assert

            Assert.That(result != null);
            Assert.AreEqual("<div><a href='/Page/1'>1</a><a href='/Page/2' class='selected'>2</a><a href='/Page/3'>3</a></div>", result.ToString());
            //Assert.That(result.ToString(), Contains.Substring("@\"<div>\" + @\"<a href=\"\"Page1\"\">1</a>\"\r\n               + @\"<a class=\"\"selected\"\" href=\"\"Page2\"\">2</a>\"\r\n               + @\"<a href=\"\"Page3\"\">3</a>\" + @\"</div>\""));
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

            var controller = new ProductController(mock.Object);
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

            var controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // Action

            var result = controller.List("Cat2", 1) as ViewResult;
            var prodArray = ((ProductsListViewModel?)result?.ViewData.Model)?.Products?.ToArray();

            // Assert
            Assert.That(prodArray?.Length == 2);
            Assert.That(prodArray[0].Name == "P2" && prodArray[0].Category == "Cat2");
            Assert.That(prodArray[1].Name == "P4" && prodArray[1].Category == "Cat2");
        }

        [Test]
        public void Can_Create_Categories()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                 new Product { ProductId = 1, Name = "P1", Category = "Apples"},
                 new Product { ProductId = 2, Name = "P2", Category = "Apples" },
                 new Product { ProductId = 3, Name = "P3", Category = "Plums" },
                 new Product { ProductId = 4, Name = "P4", Category = "Oranges" },
            }.AsQueryable());

            var target = new NavPartialViewComponent(mock.Object);

            // Act
            var result = target.Invoke() as ViewViewComponentResult;
            var categories = (result?.ViewData?.Model as IEnumerable<string>)?.ToArray();

            // Assert
            Assert.IsNotNull(categories);
            Assert.That(result?.ViewName, Is.EqualTo("_Menu"));
            Assert.That(categories?.Count() == 3);
            Assert.That(categories[0], Is.EqualTo("Apples"));
            Assert.That(categories[1], Is.EqualTo("Oranges"));
            Assert.That(categories[2], Is.EqualTo("Plums"));
        }

        [Test]
        public void Generate_Category_Specific_Product_Count()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
            new Product{ ProductId = 1, Name = "P1", Category = "Cat1" },
            new Product{ ProductId = 2, Name = "P2", Category = "Cat2" },
            new Product{ ProductId = 3, Name = "P3", Category = "Cat1" },
            new Product{ ProductId = 4, Name = "P4", Category = "Cat2" },
            new Product{ ProductId = 5, Name = "P5", Category = "Cat3" },
            }.AsQueryable());

            var controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // Act
            var data1 = controller.List("Cat1") as ViewResult;
            var res1 = ((ProductsListViewModel)data1!.ViewData.Model!).PagingInfo!.TotalItems;

            var data2 = controller.List("Cat2") as ViewResult;
            var res2 = ((ProductsListViewModel)data2!.ViewData.Model!).PagingInfo!.TotalItems;

            var data3 = controller.List("Cat3") as ViewResult;
            var res3 = ((ProductsListViewModel)data3!.ViewData.Model!).PagingInfo!.TotalItems;

            var dataAll = controller.List(null!) as ViewResult;
            var resAll = ((ProductsListViewModel)dataAll!.ViewData.Model!).PagingInfo!.TotalItems;

            // Assert
            Assert.That(res1, Is.EqualTo(2));
            Assert.That(res2, Is.EqualTo(2));
            Assert.That(res3, Is.EqualTo(1));
            Assert.That(resAll, Is.EqualTo(5));
        }
    }
}