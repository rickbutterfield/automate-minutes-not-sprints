Finally, back to our Action, and to doing the actual work in the `ExecuteAsync` method.

Pull typed settings off the context, pull the connection's settings the same way, guard against a missing connection, then call `ITravelSupplierClient` with the API key and the bound values. To be clear, that client is a stand-in for a real vendor SDK — in your site this is wherever you'd call your supplier's own library.

After the call to the supplier, the action writes a line to the run log, so the editor sees the booking in the step's Logs tab.

Once a booking is made, we hand back a `Success` with the booking reference, or a `Failed` with an exception and a category if it goes wrong.

That's a complete custom action: attribute, settings, output, connection, and this method. Nothing else Automate needs from us.
