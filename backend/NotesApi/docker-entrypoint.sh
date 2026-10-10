#!/bin/sh
set -eu

if [ -n "${PORT:-}" ]; then
  export ASPNETCORE_URLS="http://+:${PORT}"
fi

exec dotnet NotesApi.dll
