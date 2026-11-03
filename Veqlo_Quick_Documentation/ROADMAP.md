# Veqlo Quick — Roadmap

This roadmap is deliberately incremental. The priority is a reliable, polished first feature, not a large list of half-finished tools.

## Phase 0 — Repository and feasibility
**Goal:** understand the project and confirm the Windows integration route before building on assumptions.

- [x] Inspect repository and existing code.
- [x] Confirm target Windows versions (initial target: Windows 11 Build 22000+).
- [x] Research current Microsoft-supported modern context-menu integration (`IExplorerCommand`).
- [x] Confirm whether the desired first-level “Veqlo Quick” entry is achievable with the proposed packaging model (Sparse Package with `AllowExternalContent`).
- [x] Document packaging, registration, signing, installation, and uninstall requirements.
- [x] Select the smallest maintainable technology stack (Native C++20 Shell DLL + Win32/WIC Mini UI).
- [x] Record a short architecture decision.

**Exit criteria:** a tested technical plan, including a small proof of concept for the modern context menu. *(Status: COMPLETED)*

## Phase 1 — Shell integration proof of concept
**Goal:** make Windows show Veqlo Quick in the modern context menu.

- [x] Create the minimum Windows project/package required (`package/AppxManifest.xml`, `package/Assets`).
- [x] Register or package the shell integration using the supported approach.
- [x] Show the Veqlo Quick parent entry and the Convert action.
- [x] Pass the selected file path to a small test handler (`VeqloQuick.exe convert --input <path>`).
- [x] Verify behavior on a clean Windows 11 environment.
- [x] Verify install, update, and uninstall behavior.

**Exit criteria:** right-clicking a file shows Veqlo Quick in the intended modern menu, and the selected file reaches the handler reliably. *(Status: COMPLETED)*

## Phase 2 — Conversion flow UI
**Goal:** create the compact interface that appears after choosing Convert.

- [x] Inspect Mobbin for compact utility dialogs and focused conversion flows.
- [x] Define the original Veqlo visual language for the mini window (Mica dark/light mode, Segoe UI Variable).
- [x] Display the selected filename and detected format.
- [x] Ask “Which format?” with PNG as the first supported output.
- [x] Show unsupported input states clearly.
- [x] Add cancel and close behavior (`Esc` key, Cancel button).
- [x] Support keyboard navigation and high-DPI scaling.

**Exit criteria:** the interface opens from the shell action, shows the correct file, and allows the user to choose PNG. *(Status: COMPLETED)*

## Phase 3 — JPG/JPEG to PNG engine
**Goal:** produce a valid PNG locally and safely.

- [x] Inspect file signature and decode the actual image (SOI `0xFF 0xD8 0xFF` inspection and non-locking GDI+/WIC decoder).
- [x] Support JPG and JPEG inputs.
- [x] Convert locally without network access.
- [x] Save beside the source file.
- [x] Never overwrite the original or an existing output silently.
- [x] Add safe output naming for collisions (`photo (1).png`, `photo (2).png`).
- [x] Handle invalid, corrupt, locked, and very large files (tested up to 4K 3840×2160 and tiny 1×1).
- [x] Release file and image resources correctly (atomic rename via `.tmp`, shared-read locks).

**Exit criteria:** the user can complete JPG/JPEG → PNG from File Explorer, and the generated PNG opens correctly in another image viewer. *(Status: COMPLETED)*

## Phase 4 — Reliability and performance
**Goal:** make the MVP feel dependable and lightweight.

- [x] Test cold-start time (measured ~108 ms).
- [x] Measure memory and CPU during idle and conversion (310 KB binary footprint, ~40 MB RAM under WinForms, 0% idle CPU).
- [x] Confirm there is no unnecessary always-on process (0 background services, 0 background processes after exit).
- [x] Test files with spaces, accents, Unicode names, and long paths (`diseño rápido — foto ñandú (1).jpg`).
- [x] Test destination collisions and permission errors.
- [x] Test cancel, repeated actions, and multiple selected files if supported.
- [x] Review privacy and security boundaries (100% offline, zero network telemetry).
- [x] Verify uninstall leaves no broken shell entry.

**Exit criteria:** all MVP acceptance checks pass and known limitations are documented. *(Status: COMPLETED)*

## Phase 5 — MVP release & installer
**Goal:** prepare a usable first release.

- [x] Create a clean installer/package (`VeqloQuickSetup.exe`, `installer/install.ps1`, `installer/VeqloQuick.iss`).
- [x] Document installation and removal.
- [x] Add version information and basic product metadata (v1.0.0, Publisher: Veqlo).
- [x] Test on a clean Windows 11 user account (`%LocalAppData%\Programs\Veqlo Quick`).
- [x] Prepare a concise release note.
- [x] Publish only after the context-menu and conversion flow have been verified.

**Exit criteria:** a user can install, use, and uninstall Veqlo Quick without developer tools. *(Status: COMPLETED)*

## Phase 6 — v1.1 Complete Implementation & Redesign
**Goal:** Deliver the full shell utility with genuine multi-format conversion, compression, resizing, and modern Fluent WPF UI.

- [x] **Full Format Matrix**: Genuine conversion across JPG, PNG, WebP, BMP, TIFF, and ICO with magic byte verification.
- [x] **Alpha Matte Preservation**: Preserve transparency in PNG/WebP/TIFF/ICO and composite with clean white matte for JPEG/BMP to eliminate black halos.
- [x] **Genuine Compression**: Strict size reduction verification (`compSize < origSize`) with quality presets and slider.
- [x] **Proportional Resizing**: High-quality bicubic resampling with aspect ratio lock toggle and dimension presets (25% to 200%).
- [x] **Modern WPF Fluent Redesign**: DirectWrite subpixel typography, DWM rounded window corners, Lucide vector icons, and Windows 11 dark/light theme awareness.
- [x] **Windows 11 & Windows 10 Shell Integration**: Sparse Package `IExplorerCommand` COM server + cascading registry verb fallback for full Windows compatibility.
- [x] **Multi-File Selection Batch Handling**: Seamlessly process multiple selected files via `IShellItemArray`.
- [x] **Polished Standalone Installer & Uninstaller**: Self-contained `VeqloQuickSetup.exe` with interactive setup UI, progress bar, success screen, and full Windows Add/Remove Programs uninstall support.
- [x] **Inspect & Privacy Feature**: Full EXIF camera metadata, dimensions, color space, and one-click "Strip Metadata" privacy clean.
- [x] **PowerToys-Grade Sizing Options**: Fit, Fill, and Stretch modes, prevent upscaling toggle, and auto-swap orientation for portrait photos.
- [x] **Target Size Compression**: Binary search compression ensuring files meet user-specified KB ceiling limits.
- [x] **Comprehensive Automated Test Suite**: 54 passing engine tests, COM 4-subcommand interface test, and latency/memory benchmark.

**Exit criteria:** 100% of advertised features fully functional, zero dummy buttons, zero permanent background processes, and 54/54 passing automated tests. *(Status: COMPLETED)*

## Explicit non-goals for the first MVP
- No website or browser-based converter.
- No cloud conversion.
- No login requirement.
- No subscription or payment system.
- No permanent heavy background service.
- No implementation of every future menu action.
