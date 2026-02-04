#!/usr/bin/env sh
set -e

host="$1"
port="$2"
shift 2

while ! nc -z "$host" "$port" >/dev/null 2>&1; do
  echo "Waiting for $host:$port..."
  sleep 1
 done

echo "$host:$port is available."
exec "$@"
