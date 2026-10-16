This is just a straightforward service that lets us communicate with our booking system. We're saving the enquiry, sending the confirmation email, all the stuff we already had *(fold if asked)*.

Then, right at the end, it publishes that notification through the event aggregator.

One line: `_eventAggregator.PublishAsync`.

This isn't new code we added for Automate — most Umbraco sites already publish notifications like this for their own reasons, logging, caching, whatever it might be.

That's the only line in our entire website that Automate cares about.
