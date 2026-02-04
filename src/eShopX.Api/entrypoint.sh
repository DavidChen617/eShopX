#!/usr/bin/env sh
set -e

./wait-for.sh postgresql 5432 \
  ./wait-for.sh redis 6379 \
  dotnet eShopX.Api.dll
