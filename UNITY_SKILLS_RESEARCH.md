# Unity agent skills for The Line Holds

Research date: 2026-11-12. Scope: Unity 6.6, URP, C#, Apple silicon development, macOS/Windows releases, and repeatable Blender asset generation. Research only; nothing installed.

## Recommendation

Keep the official Unity skills already available in this session as the foundation. No additional community Unity collection emerged as essential for this project. Unity maintains [Unity-Technologies/skills](https://github.com/Unity-Technologies/skills); its README recommends its [official agent plugin](https://github.com/Unity-Technologies/unity-agent-plugin) for Codex so the same skills update together. Avoid installing a second copy of the collection merely to gain a different naming scheme.

These six capabilities are the practical core. Links point to the inspected upstream skill instructions, rather than descriptions from an aggregator.

| Capability | Skill(s) | Why it belongs here |
|---|---|---|
| Editor and tool control | [unity-cli](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-cli/SKILL.md) | Discovers live command schemas, connects to the intended project, manages Editors, and supports builds/tests. Its live-editor workflow matches this repository's prohibition on editing open scene/asset YAML. |
| Compile, test, inspect, iterate | [unity-pipeline](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-pipeline/SKILL.md) | Recompilation, focused tests, Play Mode, screenshots, and versioned C# builder scripts. Poll asynchronous work to completion and keep the unfocused Editor ticking. This extends the existing `AgentScripts` workflow. |
| Dependency management | [unity-package-management](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-package-management/SKILL.md) | Uses the Editor's package commands and dependency resolution, with a closed-project headless fallback. Check identifiers and compatible versions before adding packages. |
| Playable presentation | [craft-game-floors](https://github.com/Unity-Technologies/skills/blob/main/skills/craft-game-floors/SKILL.md) | Targeted guidance for camera, lighting, feedback, audio, menus and saves, with verification. Apply the requested floor against the existing art direction. |
| UI implementation | [ui](https://github.com/Unity-Technologies/skills/blob/main/skills/ui/SKILL.md), then `ui-ugui` or `ui-uitk` | Detects the current framework and routes appropriately. Follow this game's existing UI; use the relevant editor UI skill for new tools. |
| Performance triage | [project-auditor-fixes](https://github.com/Unity-Technologies/skills/blob/main/skills/project-auditor-fixes/SKILL.md) | Runs static audits, checks unavailable-rule failures, groups fixes and validates before/after behavior. Complement it with measured runtime profiling. |

## Conditional additions

Already available: use [urp-postprocessing](https://github.com/Unity-Technologies/skills/blob/main/skills/urp-postprocessing/SKILL.md) for Volume/camera configuration; `validate-urp-render-graph-renderer-feature` when adding custom renderer features; `optimize-audio` and `audio-setup-mixers` for audio work; `physics-3d-collision` for collision failures. Their upstream definitions are listed in the [official skill tree](https://github.com/Unity-Technologies/skills/tree/main/skills). They are task-specific tools, not prerequisites for every change.

One useful upstream skill missing from this session's catalog is [manage-cloud-build](https://github.com/Unity-Technologies/skills/blob/main/skills/manage-cloud-build/SKILL.md). Consider it only if adopting Unity Build Automation. It covers creating a target and diagnosing finished failures, explicitly excluding local builds and Pipeline Automation. It does not replace native Windows validation. Optional installation command, not executed:

```sh
npx skills add Unity-Technologies/skills --skill manage-cloud-build
```

## Gaps worth addressing

**Blender:** the repo already has `Tools/Blender/generate_assets.py` and editable source assets. A small project skill could document its deterministic generation/export commands, metres and forward-axis contract, material mapping, Unity reimport, and camera-based asset checks. This is more directly useful than another general modeling prompt collection. The community [libevm `blender` skill](https://github.com/libevm/agent-skills/blob/main/skills/blender/SKILL.md) is a reasonable reference candidate: it prefers direct datablock access and `bmesh`, and makes operator context explicit. It is not a must-have recommendation: its [repository](https://github.com/libevm/agent-skills) displayed only 2 stars, no verified install count was available, and it is not Blender Foundation maintained. Review against the installed Blender version before adoption.

**Runtime performance:** static audit results and short FPS samples are incomplete evidence. Extend the existing validation workflow with repeatable busy-scene CPU/GPU, allocations and memory captures on target players. Unity's [6.6 performance-data documentation](https://docs.unity.com/en-us/engine/6000.6/manual/analysis/profiler/profiling-applications/profiling-collect-data-introduction) says target-platform profiling gives more accurate timing than Play Mode profiling. This is a suggested project workflow, not a verified additional installable skill.

**Tests and builds:** `Tools/Validation/README.md` already documents substantial simulation, input, native-player and build checks. Keep these as the acceptance contract; wrap the existing commands in CI or a project skill when needed. A generic testing skill does not supply platform coverage or replace human pacing/listening review. Preserve separate macOS and Windows outputs and obtain native Windows evidence before release, as required by `AGENTS.md`.

## Discovery and confidence

Checked the [skills.sh leaderboard](https://skills.sh/) before targeted searches. The [official Unity directory](https://www.skills.sh/unity-technologies/skills) displayed approximately 9.9K installs for `unity-cli`, 8.5K for package management, 2.8K for Project Auditor, 2.3K for Pipeline, and 218 for cloud builds. The official repository displayed approximately 1.2K stars. These are dated adoption signals, not compatibility guarantees. Recommendations primarily follow inspected source instructions, Unity ownership, and this repository's existing workflow. The CLI discovery attempt did not return results in this environment; web search and direct upstream inspection supplied the evidence. Nothing was installed or executed in Unity.
