#!/bin/bash

THISDIR=$(dirname -- "$0")

LIB="$THISDIR/../lib"
OPM=$LIB/opm/src/cmd/opm.os

oscript $OPM "$@"
