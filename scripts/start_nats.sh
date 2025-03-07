#!/bin/bash

# Script to start the NATS server with JetStream enabled
# This script configures and starts the NATS server

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
log "Starting NATS server..." "INFO"
echo "Starting NATS server..."

# Check if NATS is already running
if lsof -i:4222 >/dev/null 2>&1; then
    log "NATS server is already running on port 4222. Stopping it first..." "WARNING"
    echo "NATS server is already running on port 4222. Stopping it first..."
    
    # Get the PID of the NATS server
    NATS_PID=$(lsof -t -i:4222)
    if [ -n "$NATS_PID" ]; then
        log "Stopping NATS server with PID $NATS_PID" "INFO"
        kill -15 "$NATS_PID"
        sleep 2
        
        # Check if it's still running
        if lsof -i:4222 >/dev/null 2>&1; then
            log "NATS server did not stop gracefully. Forcing termination..." "WARNING"
            kill -9 "$NATS_PID"
            sleep 1
        fi
    fi
    
    # Verify port is now available
    if lsof -i:4222 >/dev/null 2>&1; then
        log "❌ Failed to stop NATS server. Port 4222 is still in use." "ERROR"
        echo "❌ Failed to stop NATS server. Port 4222 is still in use."
        exit 1
    fi
    
    log "✅ Previous NATS server instance stopped" "SUCCESS"
fi

# Create JetStream directory if it doesn't exist
JETSTREAM_DIR="/tmp/nats-jetstream/jetstream"
if [ ! -d "$JETSTREAM_DIR" ]; then
    log "Creating JetStream directory at $JETSTREAM_DIR" "INFO"
    mkdir -p "$JETSTREAM_DIR"
    if [ $? -ne 0 ]; then
        log "❌ Failed to create JetStream directory at $JETSTREAM_DIR" "ERROR"
        echo "❌ Failed to create JetStream directory at $JETSTREAM_DIR"
        exit 1
    fi
    log "✅ Created JetStream directory at $JETSTREAM_DIR" "SUCCESS"
fi

# Start NATS server with JetStream enabled
log "Starting NATS server with JetStream enabled..." "INFO"
nats-server -js -sd "$JETSTREAM_DIR" -m 8222 -user chatservice -pass p@ssw0rd1 > "$ROOT_DIR/scripts/nats-server.log" 2>&1 &
NATS_PID=$!

# Save the PID to a file for later use
echo "$NATS_PID" > "$ROOT_DIR/scripts/nats-server.pid"
log "NATS server started with PID $NATS_PID" "INFO"

# Wait for NATS server to start
log "Waiting for NATS server to start..." "INFO"
sleep 2

# Check if NATS server is running
if ! ps -p "$NATS_PID" > /dev/null; then
    log "❌ NATS server failed to start" "ERROR"
    echo "❌ NATS server failed to start. Check logs at $ROOT_DIR/scripts/nats-server.log"
    exit 1
fi

# Verify NATS server is listening on port 4222
if ! lsof -i:4222 >/dev/null 2>&1; then
    log "❌ NATS server is not listening on port 4222" "ERROR"
    echo "❌ NATS server is not listening on port 4222"
    exit 1
fi

# Verify JetStream is enabled by checking the monitoring endpoint
log "Verifying JetStream is enabled..." "INFO"
sleep 2  # Give the server a moment to initialize JetStream

# Try up to 5 times to connect to the monitoring endpoint
for i in {1..5}; do
    if curl -s http://localhost:8222/jsz > /dev/null; then
        break
    fi
    log "Waiting for NATS monitoring endpoint to become available (attempt $i)..." "INFO"
    sleep 2
    
    if [ $i -eq 5 ]; then
        log "❌ NATS monitoring endpoint is not available after 5 attempts" "ERROR"
        echo "❌ NATS monitoring endpoint is not available"
        exit 1
    fi
done

# Check if JetStream is enabled
JETSTREAM_INFO=$(curl -s http://localhost:8222/jsz)
if [ -z "$JETSTREAM_INFO" ]; then
    log "❌ Failed to get JetStream information from monitoring endpoint" "ERROR"
    echo "❌ Failed to get JetStream information from monitoring endpoint"
    exit 1
fi

# Check if the response contains JetStream information
if ! echo "$JETSTREAM_INFO" | grep -q "server_id"; then
    log "❌ JetStream information not found in monitoring endpoint response" "ERROR"
    echo "❌ JetStream information not found in monitoring endpoint response"
    exit 1
fi

log "✅ NATS server started successfully with JetStream enabled" "SUCCESS"
echo "✅ NATS server started successfully with JetStream enabled"
exit 0 