using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GilderRoseStore.Models;
using System.Linq;

namespace GilderRoseStore.Tests.Models
{
    [TestClass]
    public class InMemoryInventoryTest
    {
        [TestMethod]
        public void TestBuyAnItemUntilTheStockIsEmpty()
        {
            var imi = new InMemoryInventory();
            var firstLowStockItem = imi.Items.OrderBy(itm => itm.Quantity).FirstOrDefault();
            Assert.IsNotNull(firstLowStockItem);
            var initialQuanity = firstLowStockItem.Quantity;
            for (int i = 0; i < initialQuanity ; i++)
            {
                Assert.IsTrue(imi.BuyItem(firstLowStockItem.Id));
            }
            Assert.IsFalse(imi.BuyItem(firstLowStockItem.Id));
        }
    }
}
