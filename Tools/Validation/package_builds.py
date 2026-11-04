"""Package completed platform builds without debug/backup files. Run from any directory."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import subprocess
repo=Path(__file__).resolve().parents[2]
builds=repo/'TheLineHolds'/'Builds'
packages=builds/'Packages'
packages.mkdir(exist_ok=True)
windows=builds/'Windows'
if (windows/'The Line Holds.exe').exists():
    with ZipFile(packages/'The-Line-Holds-Windows.zip','w',ZIP_DEFLATED) as archive:
        for path in sorted(windows.rglob('*')):
            if path.is_file() and not any('BackUpThisFolder' in p for p in path.parts) and path.suffix not in ('.pdb', '.meta'):
                archive.write(path,path.relative_to(builds))
    print(packages/'The-Line-Holds-Windows.zip')
mac=builds/'macOS'/'The Line Holds.app'
if mac.exists():
    subprocess.run(['ditto','-c','-k','--sequesterRsrc','--keepParent',str(mac),str(packages/'The-Line-Holds-macOS.zip')],check=True)
    print(packages/'The-Line-Holds-macOS.zip')
