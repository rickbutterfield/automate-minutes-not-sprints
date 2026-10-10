You're setting up an Umbraco demo site for a 45-minute conference talk called
"Automate in Minutes, Not Sprints: Introducing Umbraco Automate". You have four MCP
servers: the Umbraco CMS MCP, the Umbraco Forms MCP, the Umbraco Automate MCP and
the Umbraco Engage MCP. The site has to support two live demos, a code walkthrough
and a few example automations that the slides refer to.

## Existing project context (read this first)

You're working in the repo rickbutterfield/umbraco-automate-webinar. It's an
Umbraco 18 site using the Clean starter kit, with Automate, Forms, Commerce,
Engage and Umbraco AI installed. The site runs at https://localhost:44390.

Already present. Don't modify, rename or delete any of these:
- The Automate workspace "Bookings" (alias `sales`), with the connections "Slack"
  and "Global Travel Suppliers"
- The automation "Trip Enquiry Submitted → Check Availability → Book or Notify".
  It uses the trigger `travel.tripEnquirySubmitted` and the actions
  `travel.checkTripAvailability`, `umbracoAutomate.if`, `umbracoAutomate.parallel`,
  `travel.createSupplierBooking` and `slack.sendMessage`.
- The "Book A Trip" document type (`bookTrip`), its page and the custom enquiry
  form behind it, plus the `/trip-enquiry` endpoint. This form must stay a custom,
  non-Forms form, because the code walkthrough depends on that.
- The Clean starter kit's document types, templates and content. There's one
  exception: you may add new allowed child types to Clean's `home` type (see
  section 1). Report that change.

Umbraco Deploy writes schema and Automate changes to `src/umbraco/Deploy/Revision`
as `.uda` files. Don't edit those files directly.

## 0. Upgrade Automate first

The repo pins pre-release and older Automate packages. Before any MCP work, update
these entries in `Directory.Packages.props` to the current releases, and nothing else:

| Package | From | To |
| --- | --- | --- |
| `Umbraco.Automate` | 18.3.1--preview.19.gd2f80a6 | 18.5.0 |
| `Umbraco.Automate.Startup` | 18.3.1--preview.19.gd2f80a6 | 18.5.0 |
| `Umbraco.Automate.OpenIddict` | 18.1.3 | 18.1.5 |
| `Umbraco.Automate.Slack` | 18.0.4 | 18.0.6 |
| `Umbraco.Deploy.Automate` | 18.0.4 | 18.0.5 |

- Check each package's dependency on `Umbraco.Cms` (pinned at 18.1.1). If any of them
  needs a newer CMS version, stop and ask before changing `Umbraco.Cms` or anything
  else.
- Build, run the site and let the migrations complete. Confirm that the custom
  trigger, action and connection type in `src/Automate` still compile unchanged. Don't
  edit them, apart from the single line in section 7c: the code walkthrough slides
  show this code.
- Report the package diff and any migration output.

### What 18.5 changes for this brief

- **Service accounts must be API users (#428).** Every workspace needs a service
  account that is an Umbraco API user. If a suitable API user exists, use it. If not,
  stop and ask. Don't create client credentials or secrets for any user.
- **Actions that need a connection show as unavailable (#325).** If a workspace's
  Allowed Connections doesn't include a matching connection, the action is listed as
  disabled ("Needs a Slack connection in this workspace's Allowed Connections") and
  can't be selected. Plan workspace connections around this (see section 5).
- **Publish-time settings validation (#337).** Publishing fails if a step's required
  settings are empty. Saving a draft still works. Report any automation that can't be
  published and why.
- **Run Script fetch is off by default (#355).** Leave it off. None of the scripts in
  this brief need `fetch()`.
- **Run now only appears on published automations.** Publish before running on demand.
- **Run view shows step input, output, trigger data and log entries (#376, #379).**
  Use this to verify runs in section 7.
- **Request Approval suspends the run while it waits (#418).** A run waiting for
  approval reports Suspended, not Running.
- **Rate limits.** Defaults are 60 runs per automation per minute and 10 concurrent
  runs per automation (`Umbraco:Automate:RateLimiting`), and 100 webhook requests per
  automation per minute. Over-limit runs are skipped and logged rather than failed
  (#415). Don't change these settings. Don't trigger any automation more than a few
  times a minute while testing, and report any skipped runs.

## Ground rules

- Discover before you build. Start by listing the tools each MCP server exposes. For
  Automate, also list the available trigger types, action types, control-flow steps
  and connection types, with their aliases and settings schemas. That includes the
  ones from the Forms, Commerce, Engage and AI provider packages. Never invent a tool
  name, trigger alias, action alias or binding path. If something in this brief
  doesn't exist in this install, say so and suggest the closest real equivalent.
- Before using the Engage MCP, call one read-only tool, such as the Engage
  configuration or package metadata, to confirm it works against this Engage 18
  install. If calls fail or the responses don't match the install, stop using it
  and report the error. Don't read or export visitor profile data.
- Bindings use `${ ... }` syntax, and output property names are camelCase (for
  example `${ trigger.contentName }` or `${ steps.<stepAlias>.<property> }`). Read the
  actual output property names from each trigger's and action's schema before you
  write a binding. Give every step an explicit, readable alias so the bindings are
  easy to read on stage. Use `slack.sendMessage` for every Slack step.
- Don't create connections. Don't enter, request or store any credentials, API keys,
  OAuth tokens or SMTP passwords. You may add the existing "Slack" connection to a
  workspace's Allowed Connections where section 5 says so. Otherwise leave connection
  slots empty and list them in your final report.
- Don't delete or overwrite anything that already exists. If something with the same
  name exists, stop and ask.
- Use only fictional data: example.com email addresses, made-up names and no real
  personal data. The site is Danish/EU-context. Use DD-MM-YYYY dates, metric units,
  DKK or EUR prices and the Europe/Copenhagen time zone, and add a GDPR consent field
  to every form that collects personal data.
- Checkpoint: after discovery, show me a short plan that maps each item below to the
  real tools and aliases you found, including any gaps. Wait for my go-ahead before
  creating anything.

## Theme

A fictional travel company, "Wayfarer Travel Co." (placeholder name, easy to
rename). This matches the code walkthrough's travel company taking trip enquiries.

## 1. Content model (CMS MCP)

Reuse these Clean types instead of creating new ones:
- `home` as Home
- `articleList` as News, and `article` as News Article
- `content` as the Standard Page for the Join the Travel Club, Plan My Trip and
  Privacy pages. Keep Clean's existing contact page as it is.

Create these new types. Use the same compositions `bookTrip` uses, and give each one
a minimal template that uses Clean's master layout, so the pages render properly on
stage:
- Destination List (`destinationList`): the listing page for destinations
- Destination: country, region, summary, body (rich text), hero image, average
  flight time (hours), best months
- Trip: destination (picker), duration in days, price from, departure dates,
  description
- Offer List (`offerList`): the listing page for offers
- Offer: title, discount description, start date, end date, linked trips

Allowed child types:
- Add `destinationList` and `offerList` to Clean's `home`. This is the only change
  allowed to a Clean type.
- `destinationList` > `destination` > `trip`
- `offerList` > `offer`
- Confirm that `articleList` already allows `article`.

Create a "Travel Club Member" member type with first name, preferred destination
and marketing consent.

## 2. Sample content (CMS MCP)

- Under Home: a "Destinations" page (Destination List) with 6 destinations (Nordic
  and European), each with 1–2 trips
- 3 travel news articles under Clean's existing News/Blog page, next to the Clean
  sample articles. Leave the sample articles alone.
- An "Offers" page (Offer List) with 3 offers: one that's active, one that ends
  today and one that expired last week, so the scheduled cleanup automation has
  something to act on
- Join the Travel Club, Plan My Trip and Privacy pages, using `content`
- Media folders: Destinations, Trips, News and Uploads (unsorted). If the MCP can
  upload media, add a few placeholder images to "Uploads (unsorted)" and leave their
  alt text empty, because the media alt-text automation needs them. Use only
  generic placeholder images, not AI-generated depictions of real places.
- Publish all new content.

## 3. Forms (Forms MCP)

- "Join the Travel Club" (sign-up form for Demo 1): first name, email, preferred
  destination (dropdown of the destinations) and a required GDPR consent checkbox.
  Put it on the Join the Travel Club page.
- "Plan My Trip" (enquiry form for Demo 2): name, email, destination, travel month,
  number of travellers, free-text message and consent. Put it on the Plan My Trip
  page.
- Don't add Forms workflows that send email. Automate should handle all of that, so
  the demos aren't confused by double emails.

## 4. Engage (Engage MCP)

- Personas: "Nordic Explorer", "City Breaker" and "Adventure Seeker". Score the
  Destination and Trip pages against them using content scoring. For example, the
  Norway and Iceland pages score high for Nordic Explorer.
- Goals: "Joined the Travel Club" (a Join the Travel Club form submission) and
  "Started a trip enquiry" (a Plan My Trip submission or a visit to the Book A Trip
  page)
- A campaign group, "Autumn Travel Club drive", for the sign-up goal
- If the main switch or cookie-consent setup blocks tracking on localhost, report
  the setting that needs changing. Don't change it yourself.
- Leave A/B tests and heatmaps alone. They need real traffic and won't show
  anything useful on stage.

## 5. Automate workspaces (Automate MCP)

Because of #325, a workspace without Slack in its Allowed Connections can't hold a
Send Slack Message step at all. So the empty workspace for the live build is separate
from the workspaces that hold Slack drafts.

- **Live demo**: an empty workspace for building Demo 1 live on stage. Leave its
  Allowed Connections empty on purpose. On stage, Send Slack Message shows as
  unavailable until the speaker creates the Slack connection and allows it here. Don't
  create any automations in it.
- **Marketing**: owns the content-notification and Engage automations (C and I). Add
  the existing "Slack" connection to its Allowed Connections.
- **Operations**: owns the scheduled and webhook automations (F and G). Add the
  existing "Slack" connection only if G uses a Slack step.
- **AI & Approvals**: owns the Demo 2 and media automations (D and E). Add the
  existing "Slack" connection, because D's rejected path uses it.
- **Demo backup**: holds the finished reference copy of Demo 1 (A and B), which is the
  live-demo fallback and the source for the fallback screenshot. Add the existing
  "Slack" connection so the fallback can actually run.

Adding an existing connection to a workspace's Allowed Connections is allowed. Don't
change the connection itself, and don't create new connections. Give every workspace
an API user as its service account (see section 0).

Assign sensible user groups if the MCP supports it (Marketing to Editors, Operations
to Administrators), and report what you set.

## 6. Automations (Automate MCP)

Give every automation a description that says which slide or demo it supports.
Publish only automations that need no external connection. Leave anything that sends
Slack or email, or calls an AI agent, as a saved draft until the connections exist,
and tell me which ones those are.

**A. "Member signed up: Welcome, notify, reward"** (Demo backup workspace)
This is the finished version of Demo 1. It has one trigger and three actions:
1. Trigger: Form Submitted, on "Join the Travel Club"
2. Action: generate a discount code, using the Umbraco Commerce discount/code action.
   Report its exact display name and alias, because this is an open item in the
   deck. If Commerce has no store, stop this step, leave a clearly named
   placeholder and report it. Don't create a store yourself.
3. Action: send the welcome email to the submitter's email, with the first name and
   the discount code bound in
4. Action: Send Slack Message to #sales, with the name, preferred destination and
   code

Use exactly these four steps, in this order, because the "One automation, end to end"
slide maps them one to one (Form submitted, discount created, sales notified, email
sent, run logged). Report the backoffice URL of the finished automation so the speaker
can take the canvas screenshot for the fallback and "Minutes, not sprints" slides.

**B. "Member signed up (with Run Script)"** (Demo backup workspace)
A copy of A, with a Run Script step after the trigger. The script tidies the first
name to title case and calculates an offer expiry date 30 days out in DD-MM-YYYY
format. Set its output schema so the email step can bind to both values. Leave
AllowFetch off. This supports the "and when you do need a bit of logic" moment.

**C. "News published: tell the team"** (Marketing workspace)
Trigger: Content Published. An If step checks whether the document type alias is
`article`. On the then branch, Send Slack Message to #content with the headline and
URL. This matches the control-flow slide's spoken example.

**D. "Enquiry reply: agent drafts, human decides"** (AI & Approvals workspace)
This is Demo 2.
1. Trigger: Form Submitted, on "Plan My Trip"
2. Umbraco AI agent step: draft a friendly, personalised reply suggesting 2–3 trips
   from the chosen destination
3. Request Approval: assigned to the Editors group, with the draft shown to the
   approver
4. Approved path: send the drafted reply to the traveller by email
5. Rejected path: Slack #sales with "Needs a human reply", the enquiry details and the
   rejected draft

While a run waits at step 3 it should report Suspended (#418). Demo 2 shows that state
in the run list, so confirm the step is configured to wait for a decision rather than
time out quickly.

If no AI agent action or agent exists, list exactly what needs installing or
configuring.

**E. "New media: alt text and tags"** (AI & Approvals workspace)
Trigger: media created or saved, in "Uploads (unsorted)". An agent step generates
alt text and an SEO caption, and later steps write them back to the media item using
the Get Media and media update actions. If Automate supports exposing an automation
as a tool to the Umbraco AI agent, enable that and tell me how. This is the
"automations as tools" example.

**F. "Tidy up expired offers"** (Operations workspace)
Scheduled trigger: daily at 06:00 Europe/Copenhagen. Find Offer content whose end
date has passed, then use For Each to unpublish each one. It needs no external
connection, so publish it and run it once on demand to create a real run in the run
history.

**G. "Booking system webhook"** (Operations workspace, optional)
A Webhook trigger with a minimal action, such as writing the payload summary to a
Slack draft or a log step. Report the webhook URL and whether a test request is
available.

**H. Existing trip-enquiry automation** (Bookings workspace, read-only)
Don't rebuild or edit "Trip Enquiry Submitted → Check Availability → Book or
Notify". After the 18.5 upgrade, confirm that it loads, that it has no unconnected
steps or unresolved bindings, that its trigger and actions are still registered, and
that `travel.createSupplierBooking` isn't shown as unavailable (it needs "Global
Travel Suppliers" in the Bookings workspace's Allowed Connections). Confirm the
Bookings workspace's service account is an API user. Then list it in the report.

**I. "High-intent visitor: tell sales"** (Marketing workspace)
Using the Engage triggers and actions you found during discovery, build the closest
match to this:
- Trigger: an Engage event, such as a goal reached, a persona assigned or a segment
  entered, for the "Started a trip enquiry" goal or the Nordic Explorer persona
- Action: Send Slack Message to #sales with the persona and the pages viewed. Bind
  only to fields the trigger actually outputs, and never include personal data
  beyond what the form already collects.
- If Engage only provides actions, build the reverse instead: Form Submitted on
  "Join the Travel Club", then an Engage action that tags or scores the visitor.

Leave it as a draft, because it needs a Slack connection.

## 7. Verify

- Run every published, connection-free automation once (on demand or by triggering
  it) and confirm each one has a run whose run view shows step inputs, outputs and
  trigger data, plus any log entries. If you can't submit a form through the MCPs, say
  so and give me the exact test submission to make by hand.
- Check that no saved automation has unconnected steps or unresolved bindings, and
  report which drafts would fail publish-time validation (#337) and why.
- Report any run skipped by the rate limits.

## 7b. Code walkthrough notes

The DemoTime beats in `.demo/` stay as they are. Only edit the speaker notes in
`.demo/notes/`, and make only these changes:

- `01-notification.md`: remove the "Check before saying this live" flag about the
  add-ons. The Automate docs now publish add-ons for Forms, Commerce, Engage, Deploy,
  Workflow and UI Builder. Add that Deploy and Workflow provide triggers only.
- `02-publish.md`: fix "let's us" to "lets us".
- `03-attribute.md`: say "this kind of trigger" instead of "any trigger", because
  scheduled and webhook triggers use other base classes.
- `07-mapevent.md`: say the idempotency key "should" be included, not "has to", and
  finish the half-written sentence about retries.
- `09-action-attribute.md` and `12-connection.md`: explain `ConnectionTypeAlias`.
  Until the workspace allows a matching connection, the action is listed in the step
  picker as unavailable, with the reason shown, and can't be selected (#325).
- `13-execute.md`: say plainly that the supplier client is a stand-in for a real
  vendor SDK.

Don't change any `.cs` file except as described in 7c, and report each note you
edited.

## 7c. Optional: log the supplier booking

This makes the custom action write to the run log, which the "You stay in control"
slide shows and the run view displays in the step's Logs tab (new in 18.5).

In `src/Automate/CreateSupplierBookingAction.cs`, add exactly this line in
`ExecuteAsync`, directly after the `CreateBookingAsync(...)` call and before
`return Success(...)`:

```csharp
context.LogInfo($"Booked with Global Travel Suppliers: {bookingReference}");
```

- Don't change anything else in the file. Don't log the email address, company name,
  API key or any other settings value.
- The DemoTime beat "13. Do the work" highlights by start and end placeholder, so it
  picks up the new line without a JSON change. Confirm that.
- Add one sentence to `.demo/notes/13-execute.md` after the call to the supplier: the
  action writes a line to the run log, so the editor sees the booking in the step's
  Logs tab.
- Build again, then trigger the existing trip-enquiry automation once through the
  `/trip-enquiry` endpoint with fictional data, and confirm the log entry appears in
  the Create Supplier Booking step's Logs tab. If the supplier connection isn't
  configured for a real call, say so instead of forcing a run.

## 8. Final report

Reply with:
1. A table of everything you created: name, type, key or ID, workspace and status
   (published or draft)
2. The real alias of each trigger and action you used, and every binding expression
   in automations A–E and I, so they can be checked against the slides
3. The exact name of the Commerce discount-code action
4. The Engage MCP version check result, and everything Engage provides to Automate
   (triggers and actions, with aliases)
5. How to generate test activity that fires automation I, for example "browse three
   Nordic destination pages, then submit Plan My Trip"
6. The change you made to Clean's `home` type, and every new `.uda` file in
   `src/umbraco/Deploy/Revision`, so they can be reviewed before committing
7. A manual-setup checklist: the connections to create and where (email/SMTP and the
   AI provider, plus Slack for the Live demo workspace, which the speaker creates on
   stage), anything needing installation or a Commerce store, and which drafts to
   publish once the connections exist
8. Anything from this brief that you couldn't build, and why
9. The package upgrade diff, the service account (API user) set on each workspace,
   and the Allowed Connections on each workspace
10. The backoffice URL of automation A, for the fallback screenshot
11. The `.demo/notes` files you edited
12. Whether you made the optional 7c change, the diff, and whether the log entry
    appeared in the run view
---

## Learnings since this brief (October 2026)

What the build and the capture rounds (`CAPTURE-3.md` to `CAPTURE-8.md`) found. Read this
before re-running the brief or rebuilding Demo 1.

**Commerce and Demo 1**
- There's no Commerce "discount code" action. Demo 1 uses **Issue Gift Card**
  (`umbracoCommerce.issueGiftCard`); its output `code` is the "discount code" in the email
  and Slack message. Its settings are Store ID and Currency ID (plain text, GUIDs),
  Amount, Code and Expiry Date. There's no recipient field.
- The store "Wayfarer Travel Co." now exists (base currency GBP, so prices on the site and
  slides are in £, not DKK/EUR). Demo 1 issues £100 cards. Issued cards show as Inactive.
- A backoffice user only sees the store in the Commerce section once it has access under
  Settings → Stores → Wayfarer Travel Co. → Permissions.
- Demo 1's description still says step 2 is a placeholder until a store exists; that's
  out of date.
- Demo 1 (Demo backup) is published on the Join the Travel Club form. Anything else
  published on that form fires too, so a live build in Live demo doubles the gift card,
  email and Slack message unless one of them is unpublished.

**Email, Slack and Forms**
- SMTP points at a local Mailpit (`localhost:1025`, inbox `http://localhost:8025`); see
  `README.md`.
- Every Slack step posts to `#rick-test-channel` in the Umbraco Slack workspace; there's
  no separate demo workspace yet.
- The Forms default theme looked unstyled next to the Clean site; `forms.css` fixes it
  (see `README.md`).

**Building Demo 1 live in the backoffice**
- Steps are added with the "+" handle under a node. Insert each binding with the
  property's "…" → Insert binding; it lands at the end of the field. Pick the entry under
  the step's own name, not "Previous (…)", to get `${ steps.generateDiscountCode.code }`.
- The step alias is generated from the name ("Generate a discount code" gives
  `generateADiscountCode`); unlock it to type `generateDiscountCode`.
- Step dialogs keep settings only on **Save**. A new automation is lost if you leave the
  page before the footer Save, so save a draft before opening workspace settings to allow
  Slack.
- Clean up afterwards: delete the automation from Live demo and remove Slack from its
  Allowed Connections, so it's empty again for the talk.

**Runs and approvals**
- A Request Approval run shows Suspended. Approve it from Automation → Approvals → Review
  → Approve (not from the run view).
- The run panel lists steps by action type ("Issue Gift Card"); the canvas shows step
  names ("Generate a discount code").
- The run canvas has no link from the run panel; open
  `/umbraco/section/automation/workspace/ua:run/edit/<runId>/view/canvas`.

See `CLAUDE.md` for the agent-facing details and the capture method.
