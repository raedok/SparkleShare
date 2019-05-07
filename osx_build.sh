#!/bin/sh
/Applications/Visual\ Studio.app/Contents/MacOS/vstool clean "--configuration:ReleaseMac" "SparkleShare.sln" >/dev/null 2>/dev/null
/Applications/Visual\ Studio.app/Contents/MacOS/vstool build "--configuration:ReleaseMac" "SparkleShare.sln" >/dev/null 2>/dev/null
/Applications/Visual\ Studio.app/Contents/MacOS/vstool build "--configuration:ReleaseMac" "SparkleShare.sln"
