#!/bin/sh
set -e

KAFKA_CONNECT_URL="${KAFKA_CONNECT_URL:-http://kafka-connect:8083}"
MAX_RETRIES=30
RETRY_DELAY=2

echo "========================================="
echo "Connector Setup Script"
echo "========================================="
echo "Kafka Connect URL: $KAFKA_CONNECT_URL"
echo ""

# Wait for Kafka Connect to be ready
echo "Waiting for Kafka Connect to be ready..."
for i in $(seq 1 $MAX_RETRIES); do
  if curl -sf "$KAFKA_CONNECT_URL/connector-plugins" > /dev/null 2>&1; then
    echo "Kafka Connect is ready!"
    break
  fi
  if [ $i -eq $MAX_RETRIES ]; then
    echo "ERROR: Kafka Connect did not become ready after $MAX_RETRIES attempts"
    exit 1
  fi
  echo "  Attempt $i/$MAX_RETRIES - waiting ${RETRY_DELAY}s..."
  sleep $RETRY_DELAY
done

# Wait a bit more for Connect to fully initialize
echo "Waiting 5s for Kafka Connect to fully initialize..."
sleep 5

# Function to register/update a connector
register_connector() {
  local connector_name=$1
  local config_file=$2
  
  echo ""
  echo "Registering connector: $connector_name"
  echo "Config file: $config_file"
  
  # Extract config from JSON (handle both formats: just config, or wrapped in name/config)
  if [ ! -f "$config_file" ]; then
    echo "ERROR: Config file not found: $config_file"
    return 1
  fi
  
  # Read JSON file - current format is just the config object (correct for PUT endpoint)
  CONFIG_JSON=$(cat "$config_file")
  
  # Validate JSON is not empty
  if [ -z "$CONFIG_JSON" ]; then
    echo "ERROR: Config file is empty: $config_file"
    return 1
  fi
  
  # Register/update connector (PUT is idempotent)
  HTTP_CODE=$(curl -sS -w "%{http_code}" -o /tmp/response.json \
    -X PUT \
    -H "Content-Type: application/json" \
    --data "$CONFIG_JSON" \
    "$KAFKA_CONNECT_URL/connectors/$connector_name/config")
  
  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    echo "  Connector $connector_name registered/updated successfully (HTTP $HTTP_CODE)"
    return 0
  else
    echo "  [ERROR] Failed to register connector $connector_name (HTTP $HTTP_CODE)"
    cat /tmp/response.json 2>/dev/null || true
    return 1
  fi
}

# Function to wait for connector to be running
wait_for_connector() {
  local connector_name=$1
  local max_attempts=20
  local attempt=1
  
  echo "  Waiting for connector $connector_name to be RUNNING..."
  while [ $attempt -le $max_attempts ]; do
    STATUS=$(curl -sf "$KAFKA_CONNECT_URL/connectors/$connector_name/status" 2>/dev/null | grep -o '"state":"[^"]*"' | head -1 | cut -d'"' -f4 || echo "")
    
    if [ "$STATUS" = "RUNNING" ]; then
      echo "  Connector $connector_name is RUNNING"
      return 0
    elif [ "$STATUS" = "FAILED" ]; then
      echo "  Connector $connector_name is FAILED"
      curl -sf "$KAFKA_CONNECT_URL/connectors/$connector_name/status" | head -20
      return 1
    fi
    
    if [ $attempt -lt $max_attempts ]; then
      echo "    Attempt $attempt/$max_attempts - Status: ${STATUS:-UNKNOWN}, waiting 2s..."
      sleep 2
    fi
    attempt=$((attempt + 1))
  done
  
  echo "  Connector $connector_name did not reach RUNNING state after $max_attempts attempts"
  return 0  # Don't fail, connector might still be starting
}

# Register connectors
register_connector "sqlserver-source" "/connectors/sqlserver-source-connector.json"
SOURCE_SUCCESS=$?

register_connector "elasticsearch-sink" "/connectors/elasticsearch-sink-connector.json"
SINK_SUCCESS=$?

# Wait for connectors to be running
if [ $SOURCE_SUCCESS -eq 0 ]; then
  wait_for_connector "sqlserver-source"
fi

if [ $SINK_SUCCESS -eq 0 ]; then
  wait_for_connector "elasticsearch-sink"
fi

echo ""
echo "========================================="
if [ $SOURCE_SUCCESS -eq 0 ] && [ $SINK_SUCCESS -eq 0 ]; then
  echo "Connector setup completed successfully!"
  echo "  - sqlserver-source: Registered"
  echo "  - elasticsearch-sink: Registered"
  echo ""
  echo "Data will start flowing to Elasticsearch automatically."
else
  echo "Connector setup completed with errors"
  echo "  Check the logs above for details"
  exit 1
fi
echo "========================================="

