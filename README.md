# Automate in Minutes, Not Sprints

The demo site for **"Automate in Minutes, Not Sprints: Introducing Umbraco Automate"**, a
talk unveiling [Umbraco Automate](https://docs.umbraco.com/umbraco-automate/readme.md).
It's a working Umbraco 18 site for a fictional travel company, Wayfarer Travel Co., with
everything the talk shows on stage.

> Most Umbraco projects are full of the same quiet, repetitive work: publish this
> content, notify that person, sync to and from this system... and until now, automating
> it meant using a third-party platform or a developer's time. Now, you can automate in
> minutes, not sprints.
>
> Automate is your drag-and-drop automation engine, built right into Umbraco. It lets
> marketing teams set up automations based on triggers and actions to automate customer
> journeys in minutes, gives developers full control to extend it and keeps your data
> where it belongs.
>
> In this session we'll unveil Automate and show it in action: building a real
> automation, connecting your favourite tools, and watching it run with a full audit
> trail. We'll explore where it fits across the Umbraco ecosystem, how teams can extend
> it with their own triggers and actions, and how AI agents—with human approval built
> in—open up a whole new kind of automation.

## What the talk shows, and where it is

| In the talk | In this repo |
|---|---|
| Building a real automation | **Demo 1**, built live in the **Live demo** workspace: Join the Travel Club form → Issue Gift Card → Slack → welcome email. A finished copy is in **Demo backup**. |
| Connecting your favourite tools | The **Slack** connection, Umbraco Forms, Umbraco Commerce gift cards, and email to a local Mailpit inbox |
| Watching it run with a full audit trail | Each automation's **Runs** view: every run, and each step's status and duration |
| Where it fits across the Umbraco ecosystem | The **Marketing** and **Operations** examples: Engage personas, content published, scheduled clean-up, an inbound webhook |
| Extending it with your own triggers and actions | The custom **Trip Enquiry Submitted** trigger, **Check Trip Availability** and **Create Supplier Booking** actions, and **Global Travel Suppliers** connection type in `src/Automate/` (see [Build your own triggers and actions](#build-your-own-triggers-and-actions)) |
| AI agents, with human approval built in | **Demo 2** in **AI and Approvals**: Plan My Trip form → AI agent drafts a reply → Request Approval → email, or Slack if rejected |

The site started as the demo for an earlier webinar on building your own triggers and
actions; that code is still here and is now the talk's extensibility section.

## Build your own triggers and actions

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

Requires the .NET 10 SDK and PowerShell 7. To start everything the demo needs (LocalDB,
Mailpit and the site) and open the backoffice, Mailpit and the Join the Travel Club
page, run this from the repo root:

```
./start-demo.ps1
```

It starts only what isn't already running, so it's safe to run again. Mailpit and the
site run in their own windows; close those to stop them. Add `-NoBrowser` to skip
opening the pages. To start just the site by hand:

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

### Local builds

Three products run from local, unreleased builds in `local-packages/` (gitignored), set
in `NuGet.config` and `Directory.Packages.props`:

| Package | Built from | Shows |
|---|---|---|
| Umbraco.Automate, .Startup | PR #445 merged with 18.5.1: branch `local/pr-445-on-18.5.1` @ `185c39b` in Umbraco.Automate | Named outcome exits on steps |
| Umbraco.Commerce.Automate | Uncommitted work on `feature/generate-discount-code-action` | Store and Currency pickers on Issue Gift Card; Generate Discount Code action |
| Umbraco.AI, .Agent, .Automate, .Deploy, .TypeSafe | `v18/dev` @ `b534718` | Decisions API (Jev) actions, behind `Umbraco:AI:Experimental:Decision` |

The Currency picker's editor isn't packaged yet, so a draft copy lives in
`src/wwwroot/App_Plugins/UmbracoCommerceAutomate/`. Decisions need a TypeSafe AI
connection (API key) and a Decision profile, set up in the backoffice.

To go back to released packages, restore the version numbers in
`Directory.Packages.props`, remove the `Umbraco.AI.TypeSafe` reference and the
`local-packages` source, and restore the database from
`backups/db-12-10-2026-before-local-builds/` (the local builds added database
migrations that the released versions don't know about).

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

## The talk demo site

The content and automations for the talk were set up from `PROMPT.md`: a fictional "Wayfarer Travel Co." (Clean starter kit) with
destinations, trips, offers, two Umbraco Forms and these Automate workspaces:

| Workspace | What's in it | Allowed connections |
|---|---|---|
| **Live demo** | Empty on purpose: Demo 1 is built here on stage | none (Send Slack Message shows as unavailable until Slack is allowed) |
| **Demo backup** | "Member signed up: Welcome, notify, reward" (Demo 1; unpublished before the talk so it doesn't fire alongside the live build, and published as the fallback) and a Run Script copy (draft) | Slack |
| **AI and Approvals** | "Enquiry reply: agent drafts, human decides" (Demo 2, published) and the media alt-text examples | Slack |
| **Marketing**, **Operations**, **Bookings** | The slide examples and the original trip-enquiry automation | as needed |

- **Demo 1** (Join the Travel Club form → Issue Gift Card £100 → Slack → welcome
  email). The Slack step posts to `#rick-test-channel`. Gift cards are issued in the
  "Wayfarer Travel Co." store (GBP).
- **Demo 2** (Plan My Trip form → AI agent draft → Request Approval → email or Slack).
  A run waiting for approval shows **Suspended**. Approve it from **Automation →
  Approvals → Review → Approve**; the run view itself has no approve button.

### Local email: Mailpit

Welcome and reply emails go to a local Mailpit inbox, not a real mail server. Install
it with winget and run it natively (SMTP on 1025 and the inbox on 8025 are its
defaults):

```
winget install axllent.mailpit
mailpit
```

Messages are lost when Mailpit stops; add `--database mailpit.db` to keep them between
runs. If you have Docker Desktop with WSL2, the container works too:
`docker run -d --name mailpit -p 8025:8025 -p 1025:1025 axllent/mailpit`. The
standalone Docker CLI can't run it, because it has no Linux engine.

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
