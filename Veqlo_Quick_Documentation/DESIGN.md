# Veqlo Quick — Design Direction

## 1. Product experience

Veqlo Quick should feel like a small, native Windows utility that appears only when requested. It is not the Veqlo marketing website placed inside a window.

The product has two distinct surfaces:

1. **Windows File Explorer context menu:** the entry point to available actions.
2. **Quick Mini UI:** a compact focused window shown only after the user chooses an action such as Convert.

The separate Veqlo Control Center is for future account and global settings. It must not interrupt the quick file-action workflow.

## 2. Context-menu hierarchy

The user-facing structure is:

- Veqlo Quick
  - Convert
  - Compress
  - Resize
  - Inspect details

All four actions are fully implemented and functional. There are zero placeholder or nonfunctional actions.

The menu integrates into the modern Windows 11 context menu via a Sparse Package COM server (`IExplorerCommand`), without requiring the user to select "Show more options". A cascading submenu registry registration is also provided for Windows 10 and classic Explorer fallback.

## 3. Implemented Mini Action Dialogs (WPF Fluent UI)

All dialogs are built using hardware-accelerated Windows Presentation Foundation (WPF) with DirectWrite subpixel typography, DWM immersive dark mode, DWM rounded corners (`DWMWCP_ROUND`), and crisp Lucide vector icons.

### A. Convert Window
- Compact file summary strip showing filename, detected format badge, pixel dimensions, and size.
- Grid of 6 format cards: **PNG**, **JPG**, **WebP**, **BMP**, **TIFF**, **ICO** with clean hover states and selection rings.
- Quality fidelity slider for lossy formats (JPEG/WebP) with live percentage readout.
- Alpha transparency preservation checkbox.
- Destination directory selector ("Same directory as source" or custom browse).
- Convert button with smooth progress animation.
- Success state displaying resulting filename and actions: **Open File**, **Show in Folder**, **Done**.

### B. Compress Window
- Displays current file size and target format.
- Quality presets: **Balanced (75%)**, **Maximum (50%)**, **Light (90%)**.
- Fine-tuning quality slider (15% – 95%).
- Live estimated savings readout.
- Optional **Target Max File Size ceiling (KB)** with binary search optimization.
- **Strip Metadata toggle** for privacy and extra byte reduction.
- Destination directory selector.
- Strict size reduction guarantee: output file size is strictly verified to be smaller than the original.
- Result card displaying: `Saved X% • Original Size → Compressed Size`.

### C. Resize Window
- Displays original dimensions (`W × H px`) and aspect ratio.
- Sizing modes: **Fit** (proportional contain within bounds), **Fill** (crop to exact dimensions), **Stretch** (exact width and height).
- Quick scale presets: **25%**, **50%**, **75%**, **Original (100%)**, **150%**, **200%**.
- Custom Width & Height input fields with real-time numeric validation.
- Interactive Aspect Ratio Lock button (`🔒 Locked` / `🔓 Free`) to prevent unintended distortion.
- **Auto-swap width and height** toggle for portrait orientation photos.
- **Prevent upscaling** toggle ("Do not enlarge if smaller than target").
- Destination directory selector.
- High-quality bicubic resampling interpolation.

### D. Inspect & Privacy Window (`VeqloQuick.exe inspect`)
- Displays technical image specifications: dimensions, aspect ratio, megapixels, color space, DPI, transparency.
- Full EXIF camera metadata: camera make/model, lens, exposure time, F-stop, ISO speed, focal length, date taken, GPS coordinates.
- Multi-file pager (`◀ 1 of N ▶`) when multiple files are inspected.
- **Copy Specs** action (copies clean text summary to clipboard).
- **Strip Metadata (Privacy Clean)** action (creates a pristine, metadata-free copy with no camera tracking or GPS location data).

### E. Setup & Uninstaller Window (`VeqloQuickSetup.exe`)
- Polished Fluent UI installer embedding the full application payload.
- Veqlo monogram logo and version badge.
- Custom installation directory picker with FolderBrowserDialog.
- Feature selection checkboxes.
- Animated progress bar and status updates.
- Success completion screen with "Finish" action.
- Full uninstallation support via `--uninstall`, `--silent`, or Windows Add/Remove Programs.

## 3. Convert interaction

### Step A — Invocation

The user right-clicks a JPG/JPEG in File Explorer and chooses:

`Veqlo Quick → Convert`

The selected file is passed to the action handler. Do not open a general file picker if the user has already selected a file.

### Step B — Inspection

Inspect the selected file locally. Determine the actual image format from the file content/decoder, not just the extension.

The mini UI should show:

- File name.
- Detected current format.
- A clear question: **“Which format?”**
- Only output formats that are genuinely supported for that input.

For the initial MVP, JPG/JPEG → PNG is the required path.

### Step C — Conversion

After the user chooses PNG:

- Start conversion locally.
- Show progress only if the operation takes long enough to justify it.
- Keep the interface responsive.
- Allow cancellation where safe.
- Do not upload the image or its contents.

### Step D — Completion

Show a concise success state with:

- “Converted successfully” (or equivalent).
- Output filename.
- An action to open the containing folder, if practical.
- A close/done action.

If the destination exists, do not overwrite it silently. Use a safe alternative filename or ask the user.

### Error states

Design clear messages for:

- Unsupported file type.
- File cannot be read.
- Corrupt or invalid image.
- No write permission.
- Destination name already exists.
- Conversion failed.
- User cancelled.

Avoid exposing raw exception messages to normal users.

## 4. Visual direction

The visual identity should be recognizably Veqlo but adapted to a Windows utility.

Desired qualities:

- Minimal.
- Calm and precise.
- Modern, but not flashy.
- Clear hierarchy.
- Compact without feeling cramped.
- Fast to understand at a glance.
- Native-feeling on Windows 11.

Do not simply reuse the landing page layout, giant centered headline, or marketing-page spacing. The product UI has a different job.

### Color and theme

Start with a restrained neutral palette that works in both light and dark themes. Use semantic colors only when they communicate status (success, warning, error). Respect Windows theme preferences where feasible.

Do not hard-code a light-only interface. Verify contrast and focus visibility in both themes.

### Typography and layout

- Use a clear system-friendly typeface.
- Keep labels short and descriptive.
- Use consistent spacing and alignment.
- Make the primary action visually obvious.
- Avoid unnecessary cards inside a tiny dialog.
- Avoid oversized branding that pushes the task below the fold.

### Icons

Use simple, consistent icons. Icons should clarify actions, not decorate every line. Do not use emoji as product UI icons.

## 5. Mobbin research

Mobbin is available to the design workflow. Use it before finalizing the mini UI.

Search for references involving:

- Desktop utility dialogs.
- File conversion and export flows.
- Format selection.
- Compact settings panels.
- Progress and completion feedback.
- Empty, error, and unsupported states.

For each useful reference, note the pattern being borrowed (for example: clear file summary, segmented format choice, concise progress feedback). Do not copy the exact screen, brand identity, illustrations, wording, or proprietary assets.

Prefer desktop references where available. If Mobbin examples are primarily mobile or web, extract only general interaction principles and validate the final design against Windows desktop conventions.

## 6. Accessibility and usability

- All controls must be keyboard accessible.
- Focus state must be visible.
- Use accessible names for icon-only controls.
- Support Windows display scaling and high-DPI monitors.
- Keep text readable at normal Windows scaling.
- Do not rely on color alone to communicate status.
- Ensure the window can be closed and cancelled predictably.
- Avoid blocking the entire desktop during conversion.

## 7. Motion and feedback

Use restrained transitions only if they improve clarity. Do not add animations that delay the action or consume resources.

Never fake progress. If conversion is effectively immediate, show a brief processing state only when needed and then the real result.

## 8. Design deliverables

Before implementation is considered visually complete, produce:

- A short Mobbin research note with links or reference names and the patterns observed.
- A mini UI design for the conversion selection state.
- A processing state, if needed.
- A success state.
- Error/unsupported states.
- Light and dark theme checks.
- A screenshot of the implemented UI at normal Windows scaling.

The final UI should be original Veqlo product design, not a clone of the Veqlo website or a Mobbin reference.

---

## 9. Implemented Visual System (v1.0 Production)

The production v1.0 Mini UI implements all requirements from this specification:

- **Dimensions**: Fixed, non-intrusive 460 × 340 px window, centered on screen.
- **Backdrop & Styling**: Native Windows 11 DWM rounded corners (`DWMWA_WINDOW_CORNER_PREFERENCE = 33 / DWMWCP_ROUND`) and dynamic dark mode detection (`DWMWA_USE_IMMERSIVE_DARK_MODE = 20`).
- **Color Palettes**:
  - *Dark Theme*: Surface `#18181B` (Zinc 900), Cards `#27272A` (Zinc 800), Borders `#3F3F46` (Zinc 700), Accent `#3B82F6` (Electric Blue 500), Success `#10B981` (Emerald 500), Error `#EF4444` (Red 500).
  - *Light Theme*: Surface `#F8FAFC` (Slate 50), Cards `#FFFFFF` (White), Borders `#E2E8F0` (Slate 200), Accent `#2563EB` (Blue 600), Success `#059669` (Emerald 600), Error `#DC2626` (Red 600).
- **Typography**: Native `Segoe UI Variable Text` and `Segoe UI Variable Display` with seamless fallback to `Segoe UI`.
- **Keyboard Navigation**: `AcceptButton` bound to `Convertir` / `Listo`, `CancelButton` bound to `Cancelar` / `Cerrar`, with global `Esc` shortcut to dismiss.
- **States Implemented**:
  1. *Selection State*: File card with verified JPEG badge, pixel dimensions (`800 × 600 px`), file size, question "¿A qué formato?", active PNG card with accent highlight border, Cancel and Convert buttons.
  2. *Processing State*: Indeterminate marquee progress indicator, status label ("Convirtiendo imagen a PNG..."), file safety reassurance copy.
  3. *Completed State*: Emerald checkmark badge, "¡Conversión completada!", output filename, "Ver imagen" (opens converted file), "Abrir carpeta" (reveals file in File Explorer), "Listo" button.
  4. *Error State*: Red error badge, friendly error description (unsupported format, locked file, no read permissions), "Cerrar" button.
