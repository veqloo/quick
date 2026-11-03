# AGENTS.md — Veqlo Quick

These instructions apply to every AI coding agent working in this repository. Read this file before inspecting, planning, editing, or running code.

## 1. Product truth

Veqlo Quick is a lightweight Windows file utility integrated into Windows File Explorer. It is **not** a website, browser tool, standalone image editor, or a large dashboard.

The intended flow is:

`File Explorer → right-click → Veqlo Quick → Convert → compact format picker → local conversion`

The first working feature is JPG/JPEG → PNG. Other actions such as Compress, Resize, and File information are future product direction, not permission to build them now.

## 2. Work process

1. Inspect the existing repository, current branch, project files, and available tooling before making changes.
2. Read all relevant project documentation before proposing architecture.
3. If the repository already contains a valid implementation, preserve and improve it rather than replacing it wholesale.
4. Write a short implementation plan before substantial changes.
5. Implement in small, testable steps.
6. Build and run relevant tests after each meaningful change.
7. Report what changed, commands run, test results, known limitations, and the next concrete step.
8. Do not claim a feature works unless it has been built and tested.

Do not ask the user to manually create files or paste code when you can perform the repository work yourself. Ask only when a decision genuinely blocks progress.

## 3. Scope control

For the first MVP, implement only:

- Windows 11 context-menu entry for Veqlo Quick.
- Convert action.
- Selected-file handoff.
- Local file inspection and validation.
- Compact conversion UI.
- JPG/JPEG → PNG conversion.
- Safe output creation and clear feedback.

Do not add accounts, subscriptions, analytics, telemetry, cloud processing, sync, an updater, a web dashboard, or unrelated utilities.

Do not implement Compress, Resize, or other conversions as fake buttons. If the shell menu architecture requires placeholders, make them visibly disabled or omit them until they are real.

## 4. Architecture and technical decisions

- Prefer a small, maintainable Windows-native architecture.
- Investigate the current Microsoft-supported Windows 11 context-menu extension mechanisms before choosing an implementation.
- Verify whether the selected approach supports a first-level modern Windows 11 context-menu entry. Do not quietly fall back to the legacy “Show more options” menu.
- Consider the packaging, registration, deployment, signing, and uninstall implications before committing to a shell-extension design.
- Do not assume a plain registry verb or legacy COM registration satisfies the modern-menu requirement.
- Keep shell integration separate from conversion logic and UI.
- Use a clear boundary for passing the selected file path and requested action.
- Prefer on-demand execution. Do not create a permanently running Windows service unless a concrete technical need is demonstrated.
- Avoid unnecessary dependencies and large runtimes.
- Do not use Python as the product implementation language.
- Do not introduce a database or backend for local conversion.

Before selecting frameworks or libraries, check current official documentation and the actual compatibility requirements. Explain trade-offs briefly in the implementation plan.

## 5. File safety and conversion correctness

- Treat file paths and file contents as untrusted input.
- Validate file existence, extension, file signature/decoding, permissions, and supported image content.
- Do not trust an extension alone to determine the actual file format.
- Do not overwrite the original.
- If the destination already exists, choose a safe collision strategy or ask the user; never silently replace it.
- Use a predictable output name, such as `photo.png`, with a safe alternative such as `photo (1).png` when needed.
- Preserve the source file unchanged.
- Write output safely so an interrupted conversion does not leave a corrupt final file.
- Handle large images and malformed files without crashing.
- Show understandable errors rather than raw stack traces.
- Keep conversion entirely local. No file contents or paths should be sent to external services.

## 6. Performance

- No continuous polling.
- No folder-wide scanning.
- No background indexing.
- No startup process unless explicitly justified.
- No network calls during conversion.
- Start only the required process/components for the requested action and release resources afterwards.
- Dispose image, stream, and native resources correctly.
- Measure startup time and memory use rather than making unsupported “zero resource” claims.

## 7. UI and design

- The Quick Mini UI is separate from the Veqlo marketing website.
- Design for a compact Windows utility window, not a web page.
- Use Mobbin to research patterns for compact utility flows, format selection, file details, progress, success, and error feedback.
- Mobbin references are inspiration. Do not copy a product's exact UI, branding, layout, or assets.
- Keep the interface focused on the selected file and the next action.
- Use clear labels, restrained typography, consistent spacing, and minimal decoration.
- Support keyboard navigation, visible focus, screen scaling, and sensible accessibility labels.
- Consider Windows light/dark mode and high-DPI displays.
- Do not add decorative animations that slow down the workflow.
- Keep the user informed during longer operations, but do not show fake progress.

## 8. Security and privacy

- Process files locally.
- Request only the permissions required.
- Avoid elevated privileges unless absolutely necessary; explain and justify any requirement.
- Do not execute content from selected files.
- Do not follow arbitrary paths or links embedded in file metadata.
- Do not log private file contents. Avoid logging full personal paths unless needed for local debugging, and never send logs remotely.
- Do not add telemetry without an explicit product decision and transparent consent.

## 9. Code quality

- Use clear names and small, cohesive components.
- Separate UI, shell integration, file inspection, and conversion logic.
- Handle exceptions at appropriate boundaries.
- Avoid duplicated conversion logic.
- Add tests for file validation, output naming, collision handling, and conversion correctness.
- Keep comments useful; do not narrate obvious code.
- Do not leave dead code, placeholder implementations, or unexplained TODOs in completed features.
- Keep dependencies minimal and document why each nontrivial dependency is needed.

## 10. Mobbin workflow

When Mobbin is available:

1. Search for real examples of compact desktop utilities and focused task dialogs.
2. Inspect how they handle file selection, choosing an output, progress, completion, and errors.
3. Record the useful patterns in a short design note.
4. Create an original Veqlo interface using those patterns.
5. Do not reproduce screenshots or copy proprietary assets.

If Mobbin does not contain suitable Windows desktop examples, use relevant desktop utility patterns and verify native Windows conventions against official Microsoft guidance.

## 11. Required validation

Before calling the MVP complete, verify:

- The modern Windows 11 context-menu entry appears in the intended location.
- The selected file reaches the correct action.
- JPG and JPEG inputs are handled.
- A valid PNG is produced and can be opened by another application.
- The source file remains unchanged.
- Existing destination files are not overwritten silently.
- Invalid, missing, locked, and unsupported files produce understandable feedback.
- Cancel works.
- The application closes cleanly and releases resources.
- The conversion works offline.
- No permanent background process remains after the action, unless documented and justified.

If the modern context-menu requirement cannot be met with the chosen architecture, stop and explain the limitation and alternatives. Do not silently deliver a legacy-only context-menu integration.

## 12. Completion report

At the end of each work session, report:

- Implemented.
- Not implemented.
- Build/test commands and their actual results.
- Windows versions or environments tested.
- Known limitations.
- Next recommended task.

Be honest. Never report successful builds, tests, deployments, or UI validation that were not actually performed.
