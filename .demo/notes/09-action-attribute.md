We have the exact same pattern for `Actions` on the other side, so it should look familiar by now.

Every action is a class that implements `ActionBase` which has only two parameters this time: `Settings` and `Output`. The action is decorated with an `ActionAttribute` which registers it with Automate, and we add an alias, display name, description, group and icon.

There's one more property on this attribute: `ConnectionTypeAlias = "travel.supplierPortal"`. That says this action needs a Global Travel Suppliers connection. Until the workspace has a matching connection in its Allowed Connections, the action is still listed in the step picker, but as unavailable — with the reason shown — and it can't be selected. We'll build that connection in a minute.

This is everything we need to register an action. No config file, no registration step, no extra code. The attribute is the registration, the same as the trigger. Automate scans the assembly at startup, finds this attribute, and the action exists — ready to pick in the canvas.
