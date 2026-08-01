#!/bin/sh
set -eu

API_URL_VALUE="${VITE_API_URL:-}"
INDEX_HTML="/usr/share/nginx/html/index.html"

if [ -f "$INDEX_HTML" ]; then
  # Replace placeholder token inside the built index.html.
  # Token comes from index.html: __VITE_API_URL__
  sed -i "s|__VITE_API_URL__|$API_URL_VALUE|g" "$INDEX_HTML"
fi

exec "$@"
