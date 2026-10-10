# CLAUDE.md

Umbraco 18 demo site for the Automate webinar and the "Automate in Minutes, Not Sprints"
talk. Site: `https://localhost:44390` (run from `src` with `dotnet run --launch-profile
Webinar`). `README.md` describes the site and its workspaces; `PROMPT.md` is the original
setup brief; `CAPTURE-*.md` are the capture briefs.

## Ground rules

- **Never type credentials.** Use the existing signed-in session in the Playwright MCP
  browser. If the backoffice shows `/umbraco/login` (it does after a few hours), stop and
  ask the user to sign in in that browser window.
- **Don't edit `src/umbraco/Deploy/Revision/*.uda` by hand.** Umbraco Deploy writes them.
- **Leave automations, workspaces and connections as they are** unless a brief names the
  exact change allowed. For set-up edits (for example clearing a field to film it being
  filled): never save, close the dialog with Close, reload, and read the value back.
- **Fictional data only:** Anna, anna@example.com, example.com addresses.
- **Count every form submission and approval** and report it with the run ID. Wait at
  least a minute between submissions (Automate rate limits: 60 runs per automation per
  minute, 10 concurrent).
- **Don't install software** (ffmpeg, Playwright browsers, pip packages) without asking.

## Things that trip people up on this site

- **Two automations share the Join the Travel Club form.** Demo 1 in Demo backup is
  published on it. Anything else published on the same form (for example a copy built
  in Live demo) fires as well, so one submission sends two gift cards, two emails and
  two Slack messages. Unpublish the one you don't want for the take (only with the
  user's OK), then republish it and confirm the version is unchanged.
- **Live demo must end empty**, with no Allowed Connections, because Demo 1 is built
  there on stage. Delete anything you create in it and remove Slack again afterwards.
- **Slack is the real Umbraco workspace**, channel `#rick-test-channel`. It isn't a
  clean demo space.
  The user has chosen to mask it in recordings (light theme, other names blurred, old
  messages hidden, channel relabelled "sales"); briefs often forbid that, so ask each
  time and note it in the action log.
- **Commerce store access is per user.** If Commerce shows no stores, the user needs
  access under Settings → Stores → Wayfarer Travel Co. → Permissions.
- **Email goes to Mailpit** (`http://localhost:8025`, SMTP `localhost:1025`). The SMTP
  change in `src/appsettings.Development.json` needs a site restart.

## Umbraco backoffice UI facts (Umbraco 18, Automate 18.5)

- First load in a fresh browser context can stick on a spinner; warm up in an
  unrecorded context (reload there if needed) and pass its `storageState()` on.
- Tree items are `uui-menu-item[label="…"]`; expand with `#caret-button`, open with
  `#label-button`. Their text isn't in the accessibility tree. Table cell text often sits
  in shadow slots (`innerText` is empty); read names from `ariaSnapshot()` instead.
- **Binding picker:** hover the property, click the "…" (property actions, invisible
  until hover), then **Insert binding**. The entry is inserted at the end of the field.
  Step outputs appear twice: under the step's name and under "Previous (…)"; pick the
  named one (`.first()`) to get `${ steps.<alias>.<prop> }`, not `${ previous.<prop> }`.
- **Step dialogs:** Save applies the settings; Close discards them. A new automation is
  unsaved until the footer Save, and navigating away loses it.
- **Step alias** is generated from the name and locked; click "Unlock input" to edit it.
- **The HTML Body code editor drops typed spaces in headless Chrome.** Type spaces with
  `keyboard.insertText(' ')`.
- **Add a step** with the "+" (Add action) handle under a node; it opens Select Action.
  Actions needing a connection the workspace doesn't allow are listed disabled
  ("Needs a Slack connection in this workspace's Allowed Connections").
- **Allowed Connections picker:** clicking a connection adds it at once (no confirm
  button), then Save the workspace.
- **Runs:** the run panel slides in (take screenshots once it settles). Read status from
  the Run Info Status tag (`umb-property-layout` labelled Status → `uui-tag`), not page
  text. Runs lists don't refresh themselves (click Runs, then Design, then Runs). For a
  proof shot use the run panel: it shows each step's status and duration, while the run
  canvas shows one overall status. There's no click path from a run to its canvas; it's
  at `/umbraco/section/automation/workspace/ua:run/edit/<runId>/view/canvas`.
- **Approvals:** Automation → Approvals → Review → Approve. Afterwards the list shows
  "No pending approvals." and the run completes 3–4 s later.
- **Front-end pages:** the Engage Cockpit "OPEN" badge only renders with backoffice
  cookies (`UMB_*`); record front-end pages in a context without them. The Join form
  redirects to a "Welcome to the Travel Club" thank-you page.
- **Commerce gift card list:** re-clicking Gift Cards doesn't reload it; click Discounts,
  then Gift Cards. Issued gift cards show as Inactive.

## Capturing screenshots and video

Use the `record-it` skill (`~/.claude/skills/record-it/`); its references hold the
details. What worked here:

- Headless installed Chrome launched from the Playwright MCP sandbox with
  `--force-device-scale-factor=2`, viewport 1920 × 1080, `deviceScaleFactor: 2`, so
  screencast frames are a real 3840 × 2160 (the visible MCP window is capped by the
  display). Keep a take in one tool call so the screencast never pauses; if it must
  span calls (launch with `--remote-debugging-port`, reconnect with `connectOverCDP`),
  log each pause, because it leaves a still frame to trim.
- The MCP sandbox has no `fs`/`require`: frames and events go to the skill's
  `frame_receiver.py` through a helper tab, then `build_actions.py` and `encode.py`
  (H.264 `-crf 19 -g 10 -bf 0 -movflags +faststart`, 30 fps, under 20 MB; split long
  files with `-f segment`). **Frequent keyframes are required:** Claude Motion's export
  rendered blank frames far from a keyframe in clips with x264's default spacing. Check
  with the skill's `check_keyframes.py` (a keyframe at least every 0.5 s).
- Verify every export before using it: the skill's `check_blank.py` samples at 10 fps and
  flags near-zero-variance stretches; look at the saved frame from each, because close-ups
  of light UI score as low as blank frames.
- Several pages on one clock: log each page's `startedAt` and the trigger click's
  `clickAt`, and give each result `secondsFromClick`. Time results from the first painted
  frame; detection alone is 0.1–0.15 s early.
- Before the take you'll use, deal with what earlier takes left (Mailpit emails, gift
  cards): clear them only with the user's OK, or plan to ring the newest row. Keep
  Mailpit's "New message" toast (bottom right) out of frame.
- The Playwright MCP only runs script files from the repo or `.playwright-mcp/`.
- Full ffmpeg is the winget Gyan.FFmpeg build under
  `%LOCALAPPDATA%\Microsoft\WinGet\Packages\` (not on PATH in older sessions). Use the
  Python under `%LOCALAPPDATA%\Python\bin\` (the `python` on PATH can be the Store
  stub).
- Human pace: 700 ms before each action, 80 ms per key, 1.5 s after each click's result,
  3 s hold on results; navigate by clicking after the first load.
- Save to `captures/round<N>/`; keep failed takes in `take1/`, `take2/` with a note.
