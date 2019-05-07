#!/bin/sh
rm -rf ./SparkleShare/Mac/bin/ReleaseMac
/Applications/Visual\ Studio.app/Contents/MacOS/vstool build "--configuration:ReleaseMac" "SparkleShare.Mac.sln"
/Applications/Visual\ Studio.app/Contents/MacOS/vstool build "--configuration:ReleaseMac" "SparkleShare.Mac.sln"
rm -f SparkleShare.Mac.zip
cd ./SparkleShare/Mac/bin/ReleaseMac/
zip ../../../../SparkleShare.Mac.zip -r ./SparkleShare.Mac.app
