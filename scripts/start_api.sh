#!/bin/bash

# Script to start the API service
# This script starts the API service on port 5176

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
log "Starting API service..." "INFO"
echo "Starting API service..."

# Check if the port is already in use
if lsof -i:5176 >/dev/null 2>&1; then
    log "Port 5176 is already in use. Stopping the process first..." "WARNING"
    echo "Port 5176 is already in use. Stopping the process first..."
    
    # Get the PID of the process using port 5176
    API_PID=$(lsof -t -i:5176)
    if [ -n "$API_PID" ]; then
        log "Stopping process with PID $API_PID" "INFO"
        kill -15 "$API_PID"
        sleep 2
        
        # Check if it's still running
        if lsof -i:5176 >/dev/null 2>&1; then
            log "Process did not stop gracefully. Forcing termination..." "WARNING"
            kill -9 "$API_PID"
            sleep 1
        fi
    fi
    
    # Verify port is now available
    if lsof -i:5176 >/dev/null 2>&1; then
        log "❌ Failed to stop process. Port 5176 is still in use." "ERROR"
        echo "❌ Failed to stop process. Port 5176 is still in use."
        exit 1
    fi
    
    log "✅ Previous process using port 5176 stopped" "SUCCESS"
fi

# Check if the binary exists
API_BINARY="$ROOT_DIR/ChatService.API/bin/Debug/net8.0/ChatService.API.dll"
if [ ! -f "$API_BINARY" ]; then
    log "❌ API binary not found at $API_BINARY" "ERROR"
    echo "❌ API binary not found. Please build the solution first."
    exit 1
fi

# Check if NATS server is running
if ! lsof -i:4222 >/dev/null 2>&1; then
    log "❌ NATS server is not running on port 4222" "ERROR"
    echo "❌ NATS server is not running on port 4222. Please start it first."
    exit 1
fi

# Check if Mock Moderation service is running
if ! lsof -i:7110 >/dev/null 2>&1; then
    log "❌ Mock Moderation service is not running on port 7110" "ERROR"
    echo "❌ Mock Moderation service is not running on port 7110. Please start it first."
    exit 1
fi

# Start the API service
log "Starting API service..." "INFO"
cd "$ROOT_DIR/ChatService.API/bin/Debug/net8.0"
ASPNETCORE_URLS=http://localhost:5176 dotnet ChatService.API.dll > "$ROOT_DIR/scripts/api.log" 2>&1 &
API_PID=$!

# Save the PID to a file for later use
echo "$API_PID" > "$ROOT_DIR/scripts/api.pid"
log "API service started with PID $API_PID" "INFO"

# Wait for the service to start
log "Waiting for API service to start..." "INFO"
sleep 5

# Check if the process is still running
if ! ps -p "$API_PID" > /dev/null; then
    log "❌ API service failed to start" "ERROR"
    echo "❌ API service failed to start. Check logs at $ROOT_DIR/scripts/api.log"
    exit 1
fi

# Verify the service is listening on port 5176
if ! lsof -i:5176 >/dev/null 2>&1; then
    log "❌ API service is not listening on port 5176" "ERROR"
    echo "❌ API service is not listening on port 5176"
    exit 1
fi

# Try to connect to the service to verify it's working
log "Verifying API service is responding..." "INFO"
sleep 2  # Give the service a moment to initialize

# Try up to 5 times to connect to the service
for i in {1..5}; do
    if curl -s http://localhost:5176/api/health > /dev/null; then
        break
    fi
    log "Waiting for API service to become available (attempt $i)..." "INFO"
    sleep 2
    
    if [ $i -eq 5 ]; then
        log "⚠️ Could not verify API service is responding after 5 attempts" "WARNING"
        echo "⚠️ Could not verify API service is responding. Continuing anyway..."
        # We'll continue anyway since the process is running
    fi
done

log "✅ API service started successfully" "SUCCESS"
echo "✅ API service started successfully"
exit 0 