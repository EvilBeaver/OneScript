#!/bin/bash

if [ -z "$1" ]; then
    echo "No tagname specified"
    exit 1
fi

export LC_ALL=C.UTF-8
TAGNAME="$1"

git --no-pager log --pretty="* %s" --no-merges "${TAGNAME}..HEAD"