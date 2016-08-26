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
```xml
  ..\api\store - retrieves the current inventory - list of items and the quantity
  ..\api\store\get\{id} - buys and item from the store and decreases the current stock. Id represents the Guid Id of the desired item.
```

My focus is on Authorization / Testability both unit test and integration test / Dependecy Injection / Ability to self host
and less focus on the business layer that would require a proper customer/orders/items/shopping cart/purchase history and a real database.

Using: Asp.net Web Api, Owin self hosting, Microsoft Identity, EF, OAuth 2.0 (despite being required for OAuth 20 I didn't include https for simplicity)

Running the code:
-  Run the unit tests;
-  Run the "against the host" integration tests. Make sure you run w/o debug the web app first (CTRL+F5)
-  Run the "self host" integration tests. Make sure you run the Visual Studio in Administrator mode otherwise it won't spawn the process.

what is happening in these tests? 
 - crazy code coverage
 - a "test user" is created & the processes are spawning LocalDb catalogs to hosts the Users.
 - auth_tokens are obtained befause making the Auth call. Proving that non-auth users are getting the boot. proving that auth users are getting the right informatiom.
 - proving that the mocked Inventory in memory database is thread safe etc..

what stuff I didn't do but it would be a nice addition?
  - paginating the call to get inventory. add some metadata to the return result with total count and a sample of items. subsequent calls will have to specify the range to get specific items.
  - auding and logging.
  - tracing for WebApi
  - HTTPS - kind of a must however nobody wants to install bogus CERTs. Actually I did it at home is not a big deal to add http. some classes need small changes e.g.  OAuthAuthorizationServerOptions.AllowInsecureHttp = false etc.. plenty of examples on how to do it. next iteration will have it!
  - full Customer/Orders/Items/Shopping cart. Biz objecst and everything stored into a proper database.
  - the "buy" end point should/could specify the quantity
  - a web page to exercise the end-points
  - an example of using postman to get the bearer auth tokend plus an example of how to post for purchasing an item.
  - a diagram of the project and the dataflow
  
I wrote all these needs and wants here beause hopefully the list will shrink as I add stuff to the project.

 
