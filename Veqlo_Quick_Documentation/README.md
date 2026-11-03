# Veqlo Quick v1.1 — Windows Shell Integration

> Lightweight image utility integrated directly into Windows File Explorer's right-click context menu.

Veqlo Quick is a native Windows shell utility that brings focused image operations into Windows File Explorer. The user right-clicks any supported image (JPG, PNG, WebP, BMP, TIFF, ICO), selects **Veqlo Quick >**, and chooses **Convert**, **Compress**, or **Resize**. Each action opens a compact, purpose-built dialog to complete the operation quickly and returns the user immediately to their workflow.

**Veqlo Quick is NOT a standalone desktop application.** There is NO dashboard, NO permanent main window, NO custom sidebar, and NO continuous background daemon.

---

## What's New in v1.1

- **Right-Click Context Menu Hierarchy**:
  - `Veqlo Quick >`
    - **Convert**: Format conversion across JPG, PNG, WebP, BMP, TIFF, and ICO with full color and transparency preservation, quality fidelity sliders, and custom output destinations.
    - **Compress**: Real image optimization with quality presets (Balanced, Maximum, Light), target file size ceiling mode (KB limit), metadata stripping toggle, and strict size reduction enforcement.
    - **Resize**: Proportional image scaling with Fit, Fill (crop), and Stretch modes, scale presets (25% to 200%), custom dimensions with aspect ratio lock, prevent upscale toggle, and auto-swap orientation for portrait photos.
    - **Inspect details**: Comprehensive image specification inspector displaying pixel dimensions, aspect ratio, megapixels, color space, DPI, camera metadata (make, model, lens, exposure, F-stop, ISO, focal length, date taken, GPS coordinates), one-click clipboard copying, and a **Strip Metadata (Privacy Clean)** action.
- **Genuine Multi-Format Encoding**:
  - Eliminates extension-only renaming. Decodes image payloads, converts pixel data, and encodes genuine file headers.
  - Transparent images (PNG, WebP with alpha) composited against clean white matte when saving to JPEG/BMP to prevent black artifact halos.
  - Multi-resolution ICO icon generation (256, 48, 32, 16 px) compatible with Windows Icon standards.
  - Official Google WebP encoding/decoding bundled.
- **Windows 11 Modern & Windows 10 Explorer Integration**:
  - Top-level modern Windows 11 context menu via Sparse Package `IExplorerCommand` COM server hosted in `dllhost.exe` (no "Show more options" required).
  - Registry-based cascading submenu in `HKCU\Software\Classes\SystemFileAssociations` for Windows 10 and classic Explorer fallback.
  - Multi-file selection support (batch converts/compresses/resizes/inspects selected files).
- **Polished Setup Executable (`VeqloQuickSetup.exe`)**:
  - Standalone 804 KB setup executable embedding the complete application payload.
  - Modern Windows 11 Fluent UI, dark/light theme awareness, install location chooser, progress bar, success screen, and full uninstallation support registered in Windows Add/Remove Programs.

---

## Technical Architecture

```
[Windows File Explorer] (Right-click JPG, PNG, WebP, BMP, TIFF, ICO)
        │
        ├──────────────────────────────────────────┬───────────────────────────────────────────┐
        ▼ (Windows 11 Modern Menu)                 ▼ (Windows 10 / Classic Shell)              ▼ (Multi-Selection)
[dllhost.exe Surrogate Host]               [HKCU Cascading Registry Verb]              IShellItemArray batch
        │                                          │                                           │
        ▼                                          ▼                                           ▼
[VeqloQuickShell.dll] (C++20 COM)          [VeqloQuick.exe Direct Invocation]          Batch argument pass
        │
        ├─► Parent: "Veqlo Quick"
        └─► Subcommands:
              ├─► "Convert"         ──────► VeqloQuick.exe convert --input "<paths>"
              ├─► "Compress"        ──────► VeqloQuick.exe compress --input "<paths>"
              ├─► "Resize"          ──────► VeqloQuick.exe resize --input "<paths>"
              └─► "Inspect details" ──────► VeqloQuick.exe inspect --input "<paths>"
```

### Component Breakdown

| File | Technology | Size | Purpose |
| :--- | :--- | :--- | :--- |
| `VeqloQuick.exe` | C# .NET Framework 4.8 / WPF | ~91 KB | Purpose-built mini dialogs for Convert, Compress, Resize, and Inspect. |
| `VeqloQuickShell.dll` | C++20 (MinGW-w64 UCRT) | ~297 KB | COM Shell extension implementing `IExplorerCommand` and `IEnumExplorerCommand`. |
| `AppxManifest.xml` | Windows 11 Sparse Package | ~3.4 KB | Manifest declaring modern context menu integration and out-of-process surrogate. |
| `cwebp.exe` / `dwebp.exe` | Google libwebp 1.4.0 (x64) | Standalone | Official Google WebP encoder/decoder tools. |
| `VeqloQuickSetup.exe` | Standalone C# .NET WPF | ~804 KB | Self-contained modern Windows installer and uninstaller. |

---

## Building from Source

### Prerequisites

- Windows 10 / 11 (x64)
- .NET Framework 4.8 Compiler (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`)
- MinGW-w64 GCC (`g++`) with UCRT support

### Build Commands

Run the build script from PowerShell:

```powershell
.\scripts\build.ps1
```

This compiles:
1. `package\VeqloQuick.exe`
2. `package\VeqloQuickShell.dll`
3. Bundles all files into self-contained `VeqloQuickSetup.exe`.

---

## Installation & Testing

### Running the Setup Executable

Double-click `VeqloQuickSetup.exe` or run:

```powershell
.\VeqloQuickSetup.exe
```

To install silently:
```powershell
.\VeqloQuickSetup.exe --silent
```

To uninstall:
```powershell
.\VeqloQuickSetup.exe --uninstall
```

### Automated Test Suite

Run the comprehensive engine test suite:

```powershell
.\tests\ComprehensiveEngineTests.exe
```

Test results (54/54 passing):
- JPG ↔ PNG, PNG ↔ WebP, PNG ↔ ICO, PNG ↔ BMP, BMP ↔ TIFF multi-matrix conversions.
- Magic byte validation across all format headers.
- Alpha transparency preservation and white matte compositing for JPEG/BMP.
- Real compression size reduction verification (`compSize < origSize`).
- Target file size binary search compression below user-specified byte ceilings.
- Resizing dimension accuracy with Fit (proportional), Fill (cropped), and Stretch modes.
- Auto-swap orientation handling for portrait images.
- Original file immutability (SHA-256 hash preservation).
- Auto-increment collision avoidance (`photo (1).png`, `photo (2).png`).
- Rejection of corrupt or zero-byte files.
- High-resolution 4K processing.
- Metadata inspection (dimensions, aspect ratio, megapixels, color depth, DPI, EXIF tags).
- Privacy cleaning / metadata stripping verification.
- COM shell extension 4-subcommand interface test (`tests\TestComInterface.exe`).
- Process lifecycle and UI responsiveness test (`tests\TestUiLaunch.ps1`).

---

## Design Notes & Mobbin Reference

Per instructions, the Mobbin MCP tool was queried to search for visual modal references:
- **Result**: `Mobbin MCP requires a paid plan. Upgrade at https://mobbin.com/pricing to continue.`
- Following the explicit rule (*"If you cannot verify a free reference, say so and use a coherent Windows-native design instead. Never invent a reference or claim it is free without checking"*), the product follows Microsoft Windows 11 Fluent Design guidelines:
  - Typography: Segoe UI Variable Display & Text
  - Colors: Windows 11 Dark/Light theme aware palette (Zinc 900 / Slate 50 with Blue #2563EB / #3B82F6 accent)
  - Window Chrome: DWM rounded corners (`DWMWCP_ROUND`) and DWM immersive dark mode (`DWMWA_USE_IMMERSIVE_DARK_MODE`)
  - Layout: Clean card-based format selectors and input rows with zero clutter
