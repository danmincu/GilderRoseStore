using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GilderRoseStore.Models
{
    /// <summary>
    /// provides the inteface to record the action of buying an item
    /// </summary>
    public interface IPurchaseHistory
    {
        /// <summary>
        /// Adds a new entry recording the action of buying an item
        /// </summary>
        /// <param name="user">the current authenticated user</param>
        /// <param name="item">the purchase item</param>
        void AddEntry(string user, Item item);
    }
}