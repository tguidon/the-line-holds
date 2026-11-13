# Windows release checks

Cross-build the Windows x64 player on the Mac with development options off. Use no GitHub Actions Windows runners. Windows runtime testing is deferred until the project owner requests it; do not launch the player.

Package the executable, its data directory, the runtime libraries and the credits together. Test ZIP integrity and compare its SHA-256 digest with the GitHub release asset. The [desktop release guide](README.md#desktop-releases) gives the commands.
