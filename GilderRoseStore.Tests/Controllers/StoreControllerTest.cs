using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GilderRoseStore.Controllers;
using System.Collections.Generic;
using GilderRoseStore.Models;
using Rhino.Mocks;
using System.Linq;

namespace GilderRoseStore.Tests.Controllers
{
    [TestClass]
    public class StoreControllerTest
    {

        [TestMethod]
        public void Test_Store_Controller_Constructor()
        {
            var inventory = MockRepository.Mock<IInventory>();
            var purchaseHistory = MockRepository.Mock<IPurchaseHistory>();
            // Arrange
            StoreController controller = new StoreController(inventory, purchaseHistory);
            // Assert
            Assert.IsNotNull(controller);
        }

        [TestMethod]
        public void Test_Store_Controller_GetInventory()
        {
            var inventory = MockRepository.Mock<IInventory>();
            var purchaseHistory = MockRepository.Mock<IPurchaseHistory>();
            var items = new List<Item>() { new Item { Name = "1" }, new Item { Name = "2" } };

            inventory.Expect(i => i.Items).Return(items).Repeat.Any();

            // Arrange
            StoreController controller = new StoreController(inventory, purchaseHistory);

            // Act
            IEnumerable<Item> result = controller.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("1", result.ElementAt(0).Name);
            Assert.AreEqual("2", result.ElementAt(1).Name);

            purchaseHistory.AssertWasNotCalled(p => p.AddEntry(null, null));
        }

        [TestMethod]
        public void Test_Store_Controller_BuyAnItem()
        {
            var inventory = MockRepository.Mock<IInventory>();
            var purchaseHistory = MockRepository.Mock<IPurchaseHistory>();
            var items = new List<Item>() { new Item { Name = "1", Quantity = 1 }, new Item { Name = "2" } };
            var firstItem = items.FirstOrDefault();
            Assert.IsNotNull(firstItem);

            inventory.Expect(i => i.Items).Return(items).Repeat.Any();
            Item itm;
            inventory.Expect(i => i.BuyItem(firstItem.Id, out itm)).OutRef(firstItem).Return(true).Repeat.Any();
            

            // Arrange
            StoreController controller = new StoreController(inventory, purchaseHistory);

            // Act
            var result = controller.Get(firstItem.Id);

            // Assert
            Assert.IsTrue(result);

            purchaseHistory.AssertWasCalled(p => p.AddEntry(Arg<string>.Is.Anything, Arg<Item>.Is.Anything));
            var args = purchaseHistory.GetArgumentsForCallsMadeOn<IPurchaseHistory>( p => p.AddEntry(Arg<string>.Is.Anything, Arg<Item>.Is.Anything));
            Assert.IsTrue(args[0].Arguments[1] is Item);
            Assert.AreEqual(((Item)args[0].Arguments[1]).Id, firstItem.Id);
        }

        [TestMethod]

        public void Test_Store_Controller_NoStock_Cannot_BuyAnItem()
        {
            var inventory = MockRepository.Mock<IInventory>();
            var purchaseHistory = MockRepository.Mock<IPurchaseHistory>();
            var items = new List<Item>() { new Item { Name = "1", Quantity = 0 }, new Item { Name = "2" } };
            var firstItem = items.FirstOrDefault();
            Assert.IsNotNull(firstItem);

            inventory.Expect(i => i.Items).Return(items).Repeat.Any();
            Item itm;
            inventory.Expect(i => i.BuyItem(firstItem.Id, out itm)).OutRef(firstItem).Return(false).Repeat.Any();


            // Arrange
            StoreController controller = new StoreController(inventory, purchaseHistory);

            // Act
            var result = controller.Get(firstItem.Id);

            // Assert
            Assert.IsFalse(result);
            
            //insure there is no history when the item was not succesfully bought
            purchaseHistory.AssertWasNotCalled(p => p.AddEntry(Arg<string>.Is.Anything, Arg<Item>.Is.Anything));          
        }

    }
}
