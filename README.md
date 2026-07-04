# VRCog ⚙️

> The missing gear in your VRChat workflow.

VRCog is a collection of Unity editor utilities built for VRChat creators. Whether you're rigging avatars, polishing worlds, or just tired of repetitive editor busywork, VRCog adds the tools Unity forgot - so you can spend less time fighting the editor and more time creating.

## Installation

**[VRCog doesn't use VPM](documentation/why-not-vpm.md).**

Instead, download the latest release from the [Releases](https://github.com/zuedev/VRCog/releases) page and unzip it into your Unity project's `Assets` folder. The editor scripts will automatically compile and the tools will be available in the `Tools → VRCog` menu.

To update, simply delete the old `VRCog` folder and replace it with the new one. No need to restart Unity as the editor scripts will recompile automatically.

## Tools

### File Stat Tree

`Tools → VRCog → File Stat Tree`

Audits all assets referenced by a selected hierarchy root and sorts them by on-disk file size, so the biggest contributors are easy to spot at a glance.

- Collects materials, textures, and meshes (including skinned meshes and particle system meshes) from all renderers under the target object
- Groups sub-assets by source file so each FBX or texture atlas is counted once, not once per sub-asset
- Shows asset type alongside each entry
- Results persist across domain reloads and play mode

> **Note:** Sizes reflect on-disk file size, not runtime memory (VRAM). For accurate memory figures, use Unity's [Memory Profiler](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@latest) package.

### Poiyomi Finder

`Tools → VRCog → Poiyomi Finder`

Scans a hierarchy for renderers using a shader whose name matches a filter string (default: `poiyomi`). Each matching object is listed once with the name of the matched material and shader.

- **Shader Filter** field — change the search term to find any shader by name
- **Select** — selects and pings the object in the Hierarchy so it's easy to find in collapsed trees
- **Select All** — selects all matching objects at once for batch operations

## Requirements

- Unity **2022.3** or later
- VRChat Creator Companion (for installation via VPM)

## License

VRCog is released into the public domain under the [Unlicense](LICENSE). Do whatever you want with it.
