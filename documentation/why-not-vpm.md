# Why Not [VPM](https://vcc.docs.vrchat.com/)?

Short answer: **KISS** (Keep It Simple, Stupid). VPM exists to solve dependency management across interconnected packages, and VRCog doesn't have dependencies to manage. Adopting it now would mean building and maintaining infrastructure to solve a problem the project doesn't actually have.

## What VPM actually requires

VPM extends [Unity's Package Manager](https://docs.unity3d.com/2019.4/Documentation/Manual/Packages.html) format so the Creator Companion (VCC) can install, update, and resolve versions across packages. Shipping through it means creating and maintaining:

- A `package.json` manifest with the standard Unity manifest fields plus VPM-specific ones: `vpmDependencies`, a `url` pointing to a direct zip download, and optionally `legacyFolders`/`legacyFiles` for migrating users off an existing `.unitypackage`.
- Every release packaged as a zip and hosted at a stable URL that the manifest points to.
- A separate **[repository listing](https://vcc.docs.vrchat.com/vpm/repos)** (`index.json`) embedding the full manifest of every published version of every package, hosted somewhere VCC can fetch it from.
- Users manually adding that listing URL to VCC as a community repository *before* the package is even visible to install.

That's the right amount of machinery for a package with real dependencies: the VRChat Avatars SDK depending on the Base package, for example, or a tool that needs to compile against a specific SDK version. [VPM's version resolver](https://vcc.docs.vrchat.com/vpm/packages) is built around semver ranges and dependency-of-dependency resolution, which only pays for itself when there's an actual graph to resolve.

## What VRCog actually is

- A collection of (mostly) `EditorWindow` scripts living in a namespaced `Editor/` folder.
- No runtime component and no hard dependency on the VRChat SDK or any other package: every script compiles against nothing but `UnityEngine` and `UnityEditor`. Poiyomi Finder, for example, only searches for a shader *name* (a plain string filter); Poiyomi doesn't need to be installed for the tool to build or run.
    - Even if we did add a runtime component or hard dependency, the tools are designed to be additive: they don't replace or modify existing assets, so there's no risk of breaking a project by installing them. If we were to hook into an API like VRCFury's, we would still be able to ship a `.unitypackage` that works without it by falling back to a no-op implementation of the API.
- No dependency graph, so the part of VPM that justifies its overhead has nothing to do here.
- Small, infrequent, additive releases: a new tool window now and then, not coordinated version bumps across multiple packages.

## Side by side

| | `.unitypackage` via GitHub Releases | VPM |
|---|---|---|
| To publish | Export package, attach to a Release | Write & version a `package.json`, host zips, generate & host `index.json` |
| To install | Download, `Assets → Import Package → Custom Package` | Add a repo URL to VCC once, then add the package per project |
| Requires VCC installed | No | Yes |
| Needs a dependency graph to pay off | No | Yes |
| Moving parts that can break | One release asset | Listing host, zip host, VCC's cache, manifest/hash mismatches |

For a project this size, the VPM column is overhead without a corresponding benefit: infrastructure to maintain in exchange for nothing a static file download doesn't already do.

> **We may revisit this.** If VRCog grows real dependencies, starts shipping runtime components, or the VPM/VCC tooling matures enough that hosting a listing stops being manual upkeep, switching over would make sense. Until then, a `.unitypackage` on the Releases page is the simplest thing that works.
