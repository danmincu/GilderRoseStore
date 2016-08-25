# GilderRoseStore
Story behind - The Gilded Rose Expands
As you may know, the Gilded Rose* is a small inn in a prominent city that buys and sells only the finest items. The shopkeeper, Allison, is looking to expand by providing merchants in other cities with access to the shop's inventory via a public HTTP-accessible API.
API requirements
- Retrieve the current inventory (i.e. list of items)
- Buy an item (user must be authenticated)

Here is the definition for an item:
```xml
class Item
{
  public Guid Id { get; set;}
  public string Name { get; set; }
  public string Description { get; set; }
  public int Price { get; set; }
  public int Quantity { get; set;}
}
```
The app exposes two endpoints
  ..\api\store - retrieves the current inventory - list of items and the quantity
  ..\api\store\get\{id} - buys and item from the store and decreases the current stock. Id represents the Guid Id of the desired item.

I didn't focus on the business layer to add customer/orders/purchase history
  
Focus on Authorization / Testability / Dependecy Injection / Ability to self host
