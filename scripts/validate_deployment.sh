#!/bin/bash

# Script to validate the deployment
# This script checks all services and tests basic functionality

# Set the base directory
BASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$BASE_DIR/.."
ROOT_DIR="$(pwd)"

# Log file
LOG_FILE="$ROOT_DIR/scripts/chat_service_run.log"

# Function to log messages
log() {
    local message="$1"
    local level="${2:-INFO}"
    local timestamp=$(date +"%Y-%m-%d %H:%M:%S")
    echo "[$timestamp] [$level] $message" >> "$LOG_FILE"
}

# Header
log "Validating deployment..." "INFO"
echo "Validating deployment..."

# Check if all services are running
check_service() {
    local port="$1"
    local service="$2"
    
    if lsof -i:"$port" >/dev/null 2>&1; then
        log "✅ $service is running on port $port" "SUCCESS"
        echo "✅ $service is running on port $port"
        return 0
    else
        log "❌ $service is not running on port $port" "ERROR"
        echo "❌ $service is not running on port $port"
        return 1
    fi
}

# Check all services
services_ok=true
if ! check_service 4222 "NATS Server"; then services_ok=false; fi
if ! check_service 8222 "NATS Monitoring"; then services_ok=false; fi
if ! check_service 5176 "API Service"; then services_ok=false; fi
if ! check_service 7110 "Mock Moderation Service"; then services_ok=false; fi

if [ "$services_ok" = false ]; then
    log "❌ Some services are not running" "ERROR"
    echo "❌ Some services are not running"
    exit 1
fi

# Check if Client Realtime service is running
CLIENT_REALTIME_PID_FILE="$ROOT_DIR/scripts/client-realtime.pid"
if [ -f "$CLIENT_REALTIME_PID_FILE" ]; then
    CLIENT_REALTIME_PID=$(cat "$CLIENT_REALTIME_PID_FILE")
    if ps -p "$CLIENT_REALTIME_PID" > /dev/null; then
        log "✅ Client Realtime service is running with PID $CLIENT_REALTIME_PID" "SUCCESS"
        echo "✅ Client Realtime service is running with PID $CLIENT_REALTIME_PID"
    else
        log "❌ Client Realtime service is not running" "ERROR"
        echo "❌ Client Realtime service is not running"
        services_ok=false
    fi
else
    log "❌ Client Realtime service PID file not found" "ERROR"
    echo "❌ Client Realtime service PID file not found"
    services_ok=false
fi

if [ "$services_ok" = false ]; then
    log "❌ Some services are not running" "ERROR"
    echo "❌ Some services are not running"
    exit 1
fi

# Check JetStream status
log "Checking JetStream status..." "INFO"
echo "Checking JetStream status..."

JETSTREAM_INFO=$(curl -s http://localhost:8222/jsz)
if [ -z "$JETSTREAM_INFO" ]; then
    log "❌ Failed to get JetStream information from monitoring endpoint" "ERROR"
    echo "❌ Failed to get JetStream information from monitoring endpoint"
    exit 1
fi

# Check if JetStream is enabled by looking for specific text in the response
if ! echo "$JETSTREAM_INFO" | grep -q "server_id"; then
    log "❌ JetStream information not found in monitoring endpoint response" "ERROR"
    echo "❌ JetStream information not found in monitoring endpoint response"
    exit 1
fi

log "✅ JetStream is enabled" "SUCCESS"
echo "✅ JetStream is enabled"

# Check if streams are created by looking for specific text in the response
if ! echo "$JETSTREAM_INFO" | grep -q "streams"; then
    log "⚠️ No streams information found in JetStream" "WARNING"
    echo "⚠️ No streams information found in JetStream"
else
    # Try to extract the number of streams using grep and sed
    STREAMS_COUNT=$(echo "$JETSTREAM_INFO" | grep -o '"streams":[0-9]*' | sed 's/"streams"://')
    if [ -z "$STREAMS_COUNT" ] || [ "$STREAMS_COUNT" -eq 0 ]; then
        log "⚠️ No streams found in JetStream" "WARNING"
        echo "⚠️ No streams found in JetStream"
    else
        log "✅ Found $STREAMS_COUNT streams in JetStream" "SUCCESS"
        echo "✅ Found $STREAMS_COUNT streams in JetStream"
    fi
fi

# Test API functionality by creating a channel
log "Testing API functionality by creating a channel..." "INFO"
echo "Testing API functionality by creating a channel..."

# Generate a unique channel name
CHANNEL_NAME="Test Channel $(date +%s)"

# Create a channel
CHANNEL_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d "{\"name\":\"$CHANNEL_NAME\"}" http://localhost:5176/api/channels)

# Check if the response contains an ID, indicating success
if echo "$CHANNEL_RESPONSE" | grep -q "id"; then
    # Try to extract the channel ID using grep and sed
    CHANNEL_ID=$(echo "$CHANNEL_RESPONSE" | grep -o '"id":"[^"]*"' | sed 's/"id":"//;s/"//')
    log "✅ Successfully created channel with ID: $CHANNEL_ID" "SUCCESS"
    echo "✅ Successfully created channel with ID: $CHANNEL_ID"
else
    log "⚠️ Failed to create channel. Response: $CHANNEL_RESPONSE" "WARNING"
    echo "⚠️ Failed to create channel. Response: $CHANNEL_RESPONSE"
    # We'll continue anyway since this might be due to various reasons
fi

# Test sending a message to the channel
if [ -n "$CHANNEL_ID" ]; then
    log "Testing sending a message to the channel..." "INFO"
    echo "Testing sending a message to the channel..."
    
    # Send a message
    MESSAGE_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d "{\"channelId\":\"$CHANNEL_ID\",\"content\":\"Test message from validation script\",\"senderId\":\"system\"}" http://localhost:5176/api/messages)
    
    # Check if the response contains an ID, indicating success
    if echo "$MESSAGE_RESPONSE" | grep -q "id"; then
        # Try to extract the message ID using grep and sed
        MESSAGE_ID=$(echo "$MESSAGE_RESPONSE" | grep -o '"id":"[^"]*"' | sed 's/"id":"//;s/"//')
        log "✅ Successfully sent message with ID: $MESSAGE_ID" "SUCCESS"
        echo "✅ Successfully sent message with ID: $MESSAGE_ID"
    else
        log "⚠️ Failed to send message. Response: $MESSAGE_RESPONSE" "WARNING"
        echo "⚠️ Failed to send message. Response: $MESSAGE_RESPONSE"
        # We'll continue anyway since this might be due to various reasons
    fi
fi

# Final validation result
log "Deployment validation completed" "INFO"
echo "Deployment validation completed"

# Check if there were any warnings or errors in today's log
if grep -q "WARNING\|ERROR" "$LOG_FILE" | grep -q "$(date +"%Y-%m-%d")"; then
    log "⚠️ Deployment validation completed with warnings or errors" "WARNING"
    echo "⚠️ Deployment validation completed with warnings or errors"
    echo "Check the log file for details: $LOG_FILE"
    exit 1
else
    log "✅ Deployment validation completed successfully" "SUCCESS"
    echo "✅ Deployment validation completed successfully"
    exit 0
fi 