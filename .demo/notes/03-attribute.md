Now we cross into Automate and start building the trigger.

This kind of trigger is a class that implements `NotificationTriggerBase` which accepts three parameters: `Settings` and `Output` which we'll come to, plus the `TripEnquirySubmittedNotification` we created before. The trigger is decorated with a `TriggerAttribute` which is what registers it with Automate.

This attribute is the whole registration we need for a trigger: an alias Automate uses internally, a display name and description for the picker, a group so it sits under 'Bookings' instead of a long flat list, and an icon.

**There is no registration step and no config file. The attribute is the registration.**

Automate scans the assembly at startup, finds this attribute, and the trigger exists — ready to pick in the canvas.
