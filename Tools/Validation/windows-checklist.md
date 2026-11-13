# Windows release checks

Build the Windows x64 player with development options off. A successful build satisfies Windows release acceptance. Never execute the Windows player locally, in a VM, or on a hosted runner.

Package the executable, its data directory, the runtime libraries and the credits together. Test ZIP integrity and compare its SHA-256 digest with the GitHub release asset. The [desktop release guide](README.md#desktop-releases) gives the commands.
