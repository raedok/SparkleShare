#!/bin/sh

# Expect path to app bundle argument
export bundle=$1
export projectFolder=$(dirname $0)

echo Postprocessing ${bundle}...

export PATH=/usr/local/bin:/opt/local/bin:/Library/Frameworks/Mono.framework/Versions/Current/bin:/usr/bin:/bin

cp /Library/Frameworks/Mono.framework/Versions/Current/lib/libmono-system-native.0.dylib ${bundle}/Contents/MonoBundle/libSystem.Native

if [ ! -d "${bundle}/Contents/Resources/git" ]; then
    ${projectFolder}/checkGit.sh
    tar -x -f ${projectFolder}/git.tar.gz --directory ${bundle}/Contents/Resources
fi
cp -R SparkleShareInviteOpener.app ${bundle}/Contents/Resources
cp config ${bundle}/Contents/MonoBundle
