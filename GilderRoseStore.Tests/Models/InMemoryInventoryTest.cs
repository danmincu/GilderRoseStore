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


        [TestMethod]
        public void TestAsyncCallForBuyRespectInventory()
        {
            var imi = new InMemoryInventory();
            var mostNumerousStockItem = imi.Items.Where(itm => itm.Quantity < 20).OrderByDescending(itm => itm.Quantity).FirstOrDefault();
            Assert.IsNotNull(mostNumerousStockItem);
            var initialQuantity = mostNumerousStockItem.Quantity;
            List<Task<bool>> tasks = new List<Task<bool>>();

            for (int i = 0; i < 2 * initialQuantity; i++)
            {
                tasks.Add(Task.Run(() => { return imi.BuyItem(mostNumerousStockItem.Id); }));
            }
            Task.WhenAll(tasks).Wait();
            Assert.AreEqual(initialQuantity, tasks.Count(t => t.Result));
        }

    }
}
