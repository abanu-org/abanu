#!/bin/bash

cd $(dirname $0)

(cd src && msbuild lonos.build.sln /p:Configuration=Release)
