#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OVM_LIB_PATH="${HOME}/.local/share/ovm/current/lib"

if [[ -z "${OSCRIPT_CONFIG:-}" ]]; then
    export OSCRIPT_CONFIG="lib.system=${OVM_LIB_PATH}"
fi

OSCRIPT_BIN="${1:-${OSCRIPT_EXE:-}}"
if [[ -z "${OSCRIPT_BIN}" ]]; then
    OSCRIPT_BIN="$(command -v oscript 2>/dev/null || true)"
fi

if [[ -z "${OSCRIPT_BIN}" ]]; then
    echo "[ERROR] Failed to determine path to oscript." >&2
    echo "Pass it as the first parameter or set OSCRIPT_EXE variable." >&2
    exit 1
fi

if [[ ! -x "${OSCRIPT_BIN}" ]]; then
    echo "[ERROR] File '${OSCRIPT_BIN}' does not exist or is not executable." >&2
    exit 1
fi

cd "${SCRIPT_DIR}"
"${OSCRIPT_BIN}" testrunner.os -runAll .

