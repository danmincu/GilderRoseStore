using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GilderRoseStore.Models
{
    public interface IInventory
    {
        IEnumerable<Item> Items { get; }
        bool BuyItem(Guid itemId, out Item item);
    }

}