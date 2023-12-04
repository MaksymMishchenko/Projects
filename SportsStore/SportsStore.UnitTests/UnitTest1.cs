using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;
using SportsStore.WebUI.Controllers;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(p => p.Products).Returns(new Product[] {
                new Product { ProductId = 1, Name = "P1" },
                new Product { ProductId = 2, Name = "P2" },
                new Product { ProductId = 3, Name = "P3" },
                new Product { ProductId = 4, Name = "P4" },
                new Product { ProductId = 5, Name = "P5" }
            }.AsQueryable());

            var controller = new ProductController(null!, mock.Object);
            controller.PageSize = 3;

            //act            
            var getViewResult = controller.List(2).Result as ViewResult;
            var result = (IEnumerable<Product>?)getViewResult?.ViewData.Model;

            //assert
            Product[] prodArray = result!.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");
        }

        //public void Process_ShouldGenerateCorrectHtml()
        //{
        //    // Arrange
        //    var tagHelper = new PaginationTagHelper
        //    {
        //        PageModel = new PagingInfo { TotalItems = 10, ItemsPerPage = 5, CurrentPage = 2 },
        //        PageAction = "List"
        //    };
        //
        //    var output = new TagHelperOutput("div", new TagHelperAttributeList(), (useCachedResult, encoder) => new TagHelperContext());
        //
        //    // Create a ViewContext with a mock HttpContext and HtmlEncoder
        //    var viewContext = new ViewContext
        //    {
        //        HttpContext = new DefaultHttpContext(),
        //        Writer = new StringWriter(),
        //        ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        //    };
        //
        //    tagHelper.ViewContext = viewContext;
        //
        //    // Act
        //    tagHelper.Process(null, output);
        //
        //    // Assert
        //    var expectedHtml = "<div><a href=\"Index?page=1\">1</a><a href=\"Index?page=2\" class=\"selected\">2</a></div>";
        //    Assert.AreEqual(expectedHtml, output.Content.GetContent());
        //}
    }
}