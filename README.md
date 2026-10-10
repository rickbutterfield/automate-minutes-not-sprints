# Umbraco Automate: build your own triggers and actions

A working Umbraco 18 site demonstrating how to build a custom native trigger, a
custom native action, a custom external action, and a connection type for
[Umbraco Automate](https://docs.umbraco.com/umbraco-automate/readme.md) — built
for a webinar of the same name.

**The scenario:** a company submits a trip enquiry on the website. Automate checks
supplier availability. If available, it books the trip with the supplier and tells
sales in Slack at the same time. If not, it just tells sales to follow up by hand.

| Piece | Type |
|---|---|
| `TripEnquirySubmittedNotification` → **Trip Enquiry Submitted** trigger | custom, native |
| **Check Trip Availability** action | custom, native |
| **Create Supplier Booking** action | custom, external |
| **Global Travel Suppliers** connection type | custom |
| **Slack Send Message** | ships in `Umbraco.Automate.Slack` |

## Running the site

Requires the .NET 10 SDK.

```
cd src
dotnet run --launch-profile Webinar
```

Then open `https://localhost:44390/umbraco` (`admin@example.com` / see
`src/appsettings.Development.json` for the unattended-install password).

Fire the trigger either from the **Book a Trip** page the site ships with, or
directly from the browser address bar:

```
https://localhost:44390/trip-enquiry?email=you@example.com&company=Acme%20Ltd
```

### Slack

The demo automation includes two Slack Send Message steps. `src/appsettings.Local.json`
(gitignored, loaded only in `DEBUG` builds — see `src/Program.cs`) is where the real
Slack app's `ClientId`/`ClientSecret` live locally; it isn't checked in.

## Repo structure

```
.demo/                 Demo Time (VS Code extension) config for the code walkthrough:
                        1.trigger.json, 2.action.json, 3.flow.json — 16 beats total,
                        one per pausable step — plus .demo/notes/, the presenter
                        script for each beat.
src/                    The Umbraco site.
  Automate/             The custom trigger, actions, and connection type.
  Bookings/             Our own site code — the notification and the surface
                        controller behind the "Book a Trip" page.
  Supplier/, Fakes/     A stand-in for a real travel supplier SDK.
  Composers/            Registers the fake supplier client. Nothing else needs
                        registering — the trigger, action, and connection type
                        are discovered from their attributes at startup.
webinar-triggers-actions.slnx
```

## The talk demo site ("Automate in Minutes, Not Sprints")

The same site also carries the content and automations for the conference talk, set up
from `PROMPT.md`: a fictional "Wayfarer Travel Co." (Clean starter kit) with
destinations, trips, offers, two Umbraco Forms and these Automate workspaces:

| Workspace | What's in it | Allowed connections |
|---|---|---|
| **Live demo** | Empty on purpose: Demo 1 is built here on stage | none (Send Slack Message shows as unavailable until Slack is allowed) |
| **Demo backup** | "Member signed up: Welcome, notify, reward" (Demo 1, published) and a Run Script copy (draft) | Slack |
| **AI and Approvals** | "Enquiry reply: agent drafts, human decides" (Demo 2, published) and the media alt-text examples | Slack |
| **Marketing**, **Operations**, **Bookings** | The slide examples and the original trip-enquiry automation | as needed |

- **Demo 1** (Join the Travel Club form → Issue Gift Card £100 → Slack → welcome
  email). The Slack step posts to `#rick-test-channel`. Gift cards are issued in the
  "Wayfarer Travel Co." store (GBP).
- **Demo 2** (Plan My Trip form → AI agent draft → Request Approval → email or Slack).
  A run waiting for approval shows **Suspended**. Approve it from **Automation →
  Approvals → Review → Approve**; the run view itself has no approve button.

### Local email: Mailpit

Welcome and reply emails go to a local Mailpit inbox, not a real mail server:

```
docker run -d --name mailpit -p 8025:8025 -p 1025:1025 axllent/mailpit
```

`src/appsettings.Development.json` points Umbraco's SMTP at `localhost:1025`
(from `hello@wayfarer.example.com`). Restart the site after changing it. The inbox is
at `http://localhost:8025`.

### Commerce access

The Commerce section only lists a store if your backoffice user has access to it:
**Settings → Stores → Wayfarer Travel Co. → Permissions**, then turn on your user (or
its user group) and save. Gift cards are under **Commerce → Wayfarer Travel Co. →
Gift Cards** (newest first; the list doesn't refresh itself, so click another item such
as Discounts, then Gift Cards).

### Forms styling

`src/wwwroot/assets/css/forms.css` (linked from `src/Views/master.cshtml`) makes Umbraco
Forms match the site's Bootstrap forms (Book A Trip), shows the consent text once, and
shortens the page header on form pages so the Join form fits at 1920 × 1080.

### Screenshots and recordings

`captures/` holds the stills and screen recordings made for the slides and the Claude
Motion films, one folder per capture round (`round1-2/` … `round8/`), each with its
`manifest.json` (stills) or `<clip>.actions.json` (recordings). The briefs that produced
them are `CAPTURE-*.md`. They were made with the `record-it` Claude Code skill: 4K
(3840 × 2160) PNGs and H.264 MP4s from a headless Chrome screencast, recorded at human
pace with a log of every click and result. See `CLAUDE.md` for how that works here.

## Notes

- The `Umbraco.Automate` NuGet packages are versioned in `Directory.Packages.props`.
  This repo doesn't depend on any sibling checkout of the `Umbraco.Automate` source.
- `[Field(IsSensitive = true)]` masks a connection setting in the UI and encrypts it
  at rest, but does **not** by itself strip it from an `Umbraco.Deploy.Automate` export
  — that needs `Umbraco:Deploy:Automate:Connections:IgnoreSensitive` enabled too (off
  by default). Set in `src/appsettings.json` for this project.
- The Forms/Commerce/Workflow/Engage Automate provider packages referenced in the
  walkthrough notes (`.demo/notes/01-notification.md`) are not all shipped yet —
  check current status before repeating that claim publicly.
