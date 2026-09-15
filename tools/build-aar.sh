#!/bin/sh
# Builds MPChartLib from PhilJay/MPAndroidChart at the given git ref (branch, tag or commit; default master)
# and copies the release AAR plus the sources jar (used for the C# doc comments) into Jars/.
#
#   tools/build-aar.sh            # master
#   tools/build-aar.sh v3.1.0     # a tag
#   tools/build-aar.sh 9c7275a0   # a commit
#
# Upstream still builds with Android Gradle Plugin 7.0.4 / Gradle 7.2, which need JDK 11 and the android-28
# platform (build-tools 30.0.2 are downloaded on demand). Override JAVA_HOME / ANDROID_HOME if needed.
set -eu

REF="${1:-master}"
HERE="$(cd "$(dirname "$0")/.." && pwd)"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/mpandroidchart.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

: "${JAVA_HOME:=$(/usr/libexec/java_home -v 11)}"
: "${ANDROID_HOME:=$HOME/Library/Android/sdk}"
export JAVA_HOME ANDROID_HOME ANDROID_SDK_ROOT="$ANDROID_HOME"

git clone -q https://github.com/PhilJay/MPAndroidChart.git "$WORK"
git -C "$WORK" checkout -q "$REF"
echo "MPAndroidChart @ $(git -C "$WORK" rev-parse HEAD) ($(git -C "$WORK" log -1 --format=%cs))"

(cd "$WORK" && ./gradlew --no-daemon --console=plain -q :MPChartLib:assembleRelease :MPChartLib:sourcesJar)

cp "$WORK/MPChartLib/build/outputs/aar/MPChartLib-release.aar" "$HERE/Jars/MPChartLib-release.aar"
cp "$WORK/MPChartLib/build/libs/MPChartLib-sources.jar" "$HERE/Jars/MPChartLib-sources.jar"
ls -l "$HERE/Jars/"
echo "Now update <Version> and <PackageReleaseNotes> in MPAndroidChart.csproj and run: dotnet build -c Release"
