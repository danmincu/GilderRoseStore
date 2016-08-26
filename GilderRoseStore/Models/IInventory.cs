using System;
using System.Collections.Generic;

namespace GilderRoseStore.Models
{
    /// <summary>
    /// Provides the inteface for retrieving the inventory stock 
    /// and ability to buy a specific item in the quantity of 1
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// Returns the current inventory list
        /// </summary>
        IEnumerable<Item> Items { get; }
        
        /// <summary>
        /// Method to invoke the purchase of an item x1
        /// </summary>
        /// <param name="itemId">the Guid for the wanted item</param>
        /// <param name="item">out parameter gets the value of the item from inventory
        /// that corresponds to the itemId. *null* if cannot be found</param>
        /// <returns>returns true when the item was succesfully purchased and the quantity in the stock was decreased by 1</returns>
        bool BuyItem(Guid itemId, out Item item);
    }
}