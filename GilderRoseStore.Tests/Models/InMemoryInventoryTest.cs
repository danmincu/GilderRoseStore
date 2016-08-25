using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GilderRoseStore.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GilderRoseStore.Tests.Models
{
    [TestClass]
    public class InMemoryInventoryTest
    {

        [TestMethod]
        public void Test_Constructor_RetrieveStock()
        {
            var imi = new InMemoryInventory();
            //assert constructor works
            Assert.IsNotNull(imi);
            Assert.IsNotNull(imi.Items);
            //assert the item count in inventory is greater than zero
            Assert.IsTrue(imi.Items.Any());
        }

        [TestMethod]
        public void Test_BuyAnItemUntilTheStockIsEmpty()
        {
            var imi = new InMemoryInventory();
            var firstLowStockItem = imi.Items.OrderBy(itm => itm.Quantity).FirstOrDefault();
            Assert.IsNotNull(firstLowStockItem);
            var initialQuanity = firstLowStockItem.Quantity;
            //iterate the BuyItem method until the stock is depleted
            for (var i = 0; i < initialQuanity ; i++)
            {
                //assert the purchase succeeds
                Assert.IsTrue(imi.BuyItem(firstLowStockItem.Id));
            }
            //assert that attempting to buy an item when stock depleted is not possible
            Assert.IsFalse(imi.BuyItem(firstLowStockItem.Id));
        }


        [TestMethod]
        public void Test_AsyncCallForBuyMethodRespectsInventoryCount()
        {
            var imi = new InMemoryInventory();
            //sample an item with a large amount of inventory stock;
            //capped to 20 as we will spawn a thread count based on this number
            var mostNumerousStockItem = imi.Items.Where(itm => itm.Quantity < 20).OrderByDescending(itm => itm.Quantity).FirstOrDefault();
            Assert.IsNotNull(mostNumerousStockItem);

            var initialQuantity = mostNumerousStockItem.Quantity;
            var tasks = new List<Task<bool>>();
            
            //spawn double the quantity amount of threads to insure some threads will attempt to buy after the stock depleted
            for (int i = 0; i < 2 * initialQuantity; i++)
            {
                tasks.Add(Task.Run(() => imi.BuyItem(mostNumerousStockItem.Id)));
            }
            Task.WhenAll(tasks).Wait();
            //assert that the amount of bought items matches the inventory count
            Assert.AreEqual(initialQuantity, tasks.Count(t => t.Result));
        }

    }
}
