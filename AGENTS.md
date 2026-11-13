# The Line Holds

## Technology
- Unity 6.6 with Universal Render Pipeline (URP), using C#.
- Target macOS and Windows from one Unity project. Develop on Apple silicon macOS.
- Use Blender for 3D assets, with Python (`bpy`) scripts for repeatable generation and export.
- Use Git with Git LFS for large binary assets. Configure tracking before committing assets, and ignore Unity-generated folders such as `Library/`, `Temp/`, and build output.

## Development workflow
- Commit work at validated checkpoints as development progresses, using small, logically grouped commits with clear messages. Keep each Unity asset with its `.meta` file, review the staged diff, and exclude generated files, local settings, secrets, and build output before committing. Do not wait until the entire prototype or feature is finished to record its history.
- Use the installed CLI, skills, and MCPs for project work. If Computer Use is essential, pause, explain why those tools cannot complete the task, and wait for the user before using it.
- Install the Unity Pipeline package when creating the project and verify the live editor connection with `unity status`.
- Drive a connected editor through the CLI; do not hand-edit scene, prefab, or asset YAML while it is open.
- Prefer reusable C# editor tools and Blender scripts over manual setup. Keep automation in version control.
- Import exported models into Unity with consistent scale, materials, and prefab configuration. Keep editable source assets.
- Reuse suitable openly licensed assets. Record their sources, licenses, and required attribution.

## Validation and builds
- Start with a small playable prototype and iterate in working increments.
- Validate changes through compilation, focused tests where useful, and Play Mode checks. Inspect visual changes from the intended camera view.
- Keep build commands repeatable and maintain separate macOS and Windows outputs.
- Validate macOS on the Mac. Cross-build Windows x64 on the same Mac, verify the ZIP, and upload it to GitHub Releases. Do not use GitHub Actions Windows runners for any job or run the Windows player anywhere. Revisit Windows runtime testing only when the user requests it.
