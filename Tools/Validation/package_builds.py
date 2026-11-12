"""Package completed platform builds without debug/backup files. Run from any directory."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import argparse
import subprocess
repo=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--builds',type=Path,default=repo/'TheLineHolds'/'Builds',help='Folder containing macOS and Windows outputs')
parser.add_argument('--version',help='Include the release version and target architecture in the archive names')
args=parser.parse_args()
builds=args.builds.resolve()
packages=builds/'Packages'
if args.version:
    for binary in (builds/'Windows'/'The Line Holds.exe', builds/'macOS'/'The Line Holds.app'/'Contents'/'MacOS'/'The Line Holds'):
        if not binary.is_file():
            parser.error(f'Missing release player: {binary}')
    app=builds/'macOS'/'The Line Holds.app'
    developer_id='anchor apple generic and certificate leaf[field.1.2.840.113635.100.6.1.13] exists'
    subprocess.run(['codesign','--verify','--deep','--strict','-R='+developer_id,str(app)],check=True)
    subprocess.run(['xcrun','stapler','validate',str(app)],check=True)
    subprocess.run(['syspolicy_check','distribution',str(app)],check=True)
packages.mkdir(exist_ok=True)
prefix=f'The-Line-Holds-{args.version}' if args.version else 'The-Line-Holds'
win_zip=packages/(f'{prefix}-Windows-x64.zip' if args.version else f'{prefix}-Windows.zip')
mac_zip=packages/f'{prefix}-macOS.zip'
windows=builds/'Windows'
if (windows/'The Line Holds.exe').exists():
    with ZipFile(win_zip,'w',ZIP_DEFLATED) as archive:
        for path in sorted(windows.rglob('*')):
            if path.is_file() and not any('BackUpThisFolder' in p for p in path.parts) and path.suffix not in ('.pdb', '.meta'):
                archive.write(path,path.relative_to(builds))
    print(win_zip)
mac=builds/'macOS'/'The Line Holds.app'
if mac.exists():
    subprocess.run(['ditto','-c','-k','--sequesterRsrc','--keepParent',str(mac),str(mac_zip)],check=True)
    print(mac_zip)
