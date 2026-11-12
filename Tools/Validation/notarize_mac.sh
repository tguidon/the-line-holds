#!/bin/bash
# Sign the Unity standalone player inside out, submit it to Apple, then staple the ticket.
set -euo pipefail
if [[ $# -ne 3 ]]; then
    echo "Usage: $0 <release.app> <Developer ID Application identity> <notarytool profile>" >&2
    exit 2
fi
app=$1
identity=$2
profile=$3
entitlements="$(dirname "$0")/mac-player.entitlements"
requirement='anchor apple generic and certificate leaf[field.1.2.840.113635.100.6.1.13] exists'
[[ -d "$app/Contents" ]] || { echo "Missing app bundle: $app" >&2; exit 1; }
# Validate credentials before modifying the local build.
xcrun notarytool history --keychain-profile "$profile" >/dev/null
sign=(codesign --force --sign "$identity" --options runtime --timestamp)
for binary in "$app"/Contents/Frameworks/*.dylib "$app"/Contents/PlugIns/*.bundle; do
    [[ -e "$binary" ]] || continue
    "${sign[@]}" "$binary"
done
"${sign[@]}" --entitlements "$entitlements" "$app"
codesign --verify --strict --deep -R="$requirement" "$app"

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
ditto -c -k --keepParent "$app" "$work/submission.zip"
receipt="$(xcrun notarytool submit "$work/submission.zip" --keychain-profile "$profile" --output-format json)"
submission="$(plutil -extract id raw -o - - <<< "$receipt")"
echo "Apple notarization submission $submission"
xcrun notarytool wait "$submission" --keychain-profile "$profile"
result="$(xcrun notarytool info "$submission" --keychain-profile "$profile" --output-format json)"
if [[ "$(plutil -extract status raw -o - - <<< "$result")" != Accepted ]]; then
    xcrun notarytool log "$submission" --keychain-profile "$profile" >&2
    exit 1
fi
xcrun stapler staple "$app"
xcrun stapler validate "$app"
syspolicy_check distribution "$app"
