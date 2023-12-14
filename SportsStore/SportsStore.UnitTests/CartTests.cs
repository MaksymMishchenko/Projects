using SportsStore.Domain.Entities;

namespace SportsStore.UnitTests
{
    internal class CartTests
    {
        [Test]
        public void Can_Add_New_Lines()
        {
            // Arrange
            var p1 = new Product { ProductId = 1, Name = "P1" };
            var p2 = new Product { ProductId = 2, Name = "P2" };

            var target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            CartLine[] result = target.Lines.ToArray();

            // Assert
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Product, Is.EqualTo(p1));
            Assert.That(result[1].Product, Is.EqualTo(p2));

        }

        [Test]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            // Arrange
            var p1 = new Product { ProductId = 1, Name = "P1" };
            var p2 = new Product { ProductId = 2, Name = "P2" };

            var target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);
            CartLine[] result = target.Lines.OrderBy(p => p.Product?.ProductId).ToArray();

            // Assert
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Quantity, Is.EqualTo(11));
            Assert.That(result[1].Quantity, Is.EqualTo(1));
        }

        [Test]
        public void Can_Remove_Line()
        {
            // Arrange
            var p1 = new Product { ProductId = 1, Name = "P1" };
            var p2 = new Product { ProductId = 2, Name = "P2" };
            var p3 = new Product { ProductId = 3, Name = "P3" };

            var target = new Cart();

            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            // Act
            target.RemoveLine(p2);

            // Assert
            Assert.That(target.Lines.Where(c => c.Product == p2).Count(), Is.EqualTo(0));
            Assert.That(target.Lines.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Calculate_Cart_Total()
        {
            // Arrange
            var p1 = new Product { ProductId = 1, Name = "P1", Price = 100M };
            var p2 = new Product { ProductId = 2, Name = "P2", Price = 50M };

            var target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal? result = target.ComputeTotalValue();

            // Assert
            Assert.That(result, Is.EqualTo(450M));
        }

        [Test]
        public void Can_Clear_Contents()
        {
            // Arrange
            var p1 = new Product { ProductId = 1, Name = "P1", Price = 100M };
            var p2 = new Product { ProductId = 2, Name = "P2", Price = 50M };

            var target = new Cart();
            target.AddItem(p1, 1);
            target.AddItem(p2, 2);

            // Act
            target.Clear();

            // Assert
            Assert.That(target.Lines.Count(), Is.EqualTo(0));
        }
    }
}
