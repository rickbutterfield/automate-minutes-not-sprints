This is where we're starting and it's currently nowhere near Automate, it's just an `INotification`. Note that the form I created is not an Umbraco Forms one, it's a custom form, and the notification is not an Umbraco Forms one either. It's just a simple class that implements `INotification`. Three properties — an enquiry ID, an email, a company name.

You can do this exact same thing with an Umbraco Forms notification, but I wanted to show you that Automate doesn't care about the source of the notification, it only cares about the fact that it's a notification. We publish add-ons for Forms, Commerce, Engage, Deploy, Workflow and UI Builder, and they bring their own triggers and actions for those products — Workflow provides triggers only, and Deploy is mostly triggers plus a few actions like adding to the deploy queue.

Nothing Automate-specific about it at all. No interface to implement for Automate's benefit, no base class to inherit, nothing to import from a package we didn't already have.
If you've written Umbraco code before, you've already written one of these, probably more than once.
Keep it in your mind for a minute — it's the thread that ties everything else in this walkthrough together.
