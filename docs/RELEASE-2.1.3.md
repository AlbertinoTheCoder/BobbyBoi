# BobbyBoi 2.1.3 — No more invisible house guests

I fixed BobbyBoi remaining invisible after his house is removed or disabled. House cleanup now clears stale inside/roof state, stops invalid visits (including suspended ones), and immediately redraws him even when paused. Empty house meshes and world reset use the same recovery path.

Other events still keep their intended visibility and scaling. Inventory and gifts are preserved.

I added a regression check covering removal, disabling, empty houses, suspended visits, rooftop recovery, pause behavior, and unrelated event visibility. It also runs in the Windows CI workflow.

Extract **BobbyBoi-Windows-x64.zip** and run **BobbyBoi.exe**. Existing settings and inventory are preserved. Original code and assets use PolyForm Noncommercial 1.0.0; see LICENSE and NOTICE.txt in the download.
