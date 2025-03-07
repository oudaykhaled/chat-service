#!/bin/bash

# Script to start the Mock Moderation service
# This script starts the Mock Moderation service on port 7110

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
log "Starting Mock Moderation service..." "INFO"
echo "Starting Mock Moderation service..."

# Check if the port is already in use
if lsof -i:7110 >/dev/null 2>&1; then
    log "Port 7110 is already in use. Stopping the process first..." "WARNING"
    echo "Port 7110 is already in use. Stopping the process first..."
    
    # Get the PID of the process using port 7110
    MODERATION_PID=$(lsof -t -i:7110)
    if [ -n "$MODERATION_PID" ]; then
        log "Stopping process with PID $MODERATION_PID" "INFO"
        kill -15 "$MODERATION_PID"
        sleep 2
        
        # Check if it's still running
        if lsof -i:7110 >/dev/null 2>&1; then
            log "Process did not stop gracefully. Forcing termination..." "WARNING"
            kill -9 "$MODERATION_PID"
            sleep 1
        fi
    fi
    
    # Verify port is now available
    if lsof -i:7110 >/dev/null 2>&1; then
        log "❌ Failed to stop process. Port 7110 is still in use." "ERROR"
        echo "❌ Failed to stop process. Port 7110 is still in use."
        exit 1
    fi
    
    log "✅ Previous process using port 7110 stopped" "SUCCESS"
fi

# Check if the binary exists
MOCK_MODERATION_BINARY="$ROOT_DIR/ChatService.MockModeration/bin/Debug/net8.0/ChatService.MockModeration.dll"
if [ ! -f "$MOCK_MODERATION_BINARY" ]; then
    log "❌ Mock Moderation binary not found at $MOCK_MODERATION_BINARY" "ERROR"
    echo "❌ Mock Moderation binary not found. Please build the solution first."
    exit 1
fi

# Start the Mock Moderation service
log "Starting Mock Moderation service..." "INFO"
cd "$ROOT_DIR/ChatService.MockModeration/bin/Debug/net8.0"
ASPNETCORE_URLS=https://localhost:7110 dotnet ChatService.MockModeration.dll > "$ROOT_DIR/scripts/mock-moderation.log" 2>&1 &
MODERATION_PID=$!

# Save the PID to a file for later use
echo "$MODERATION_PID" > "$ROOT_DIR/scripts/mock-moderation.pid"
log "Mock Moderation service started with PID $MODERATION_PID" "INFO"

# Wait for the service to start
log "Waiting for Mock Moderation service to start..." "INFO"
sleep 5

# Check if the process is still running
if ! ps -p "$MODERATION_PID" > /dev/null; then
    log "❌ Mock Moderation service failed to start" "ERROR"
    echo "❌ Mock Moderation service failed to start. Check logs at $ROOT_DIR/scripts/mock-moderation.log"
    exit 1
fi

# Verify the service is listening on port 7110
if ! lsof -i:7110 >/dev/null 2>&1; then
    log "❌ Mock Moderation service is not listening on port 7110" "ERROR"
    echo "❌ Mock Moderation service is not listening on port 7110"
    exit 1
fi

# Try to connect to the service to verify it's working
log "Verifying Mock Moderation service is responding..." "INFO"
sleep 2  # Give the service a moment to initialize

# Try up to 5 times to connect to the service
for i in {1..5}; do
    if curl -sk https://localhost:7110/api/moderations/health > /dev/null; then
        break
    fi
    log "Waiting for Mock Moderation service to become available (attempt $i)..." "INFO"
    sleep 2
    
    if [ $i -eq 5 ]; then
        log "⚠️ Could not verify Mock Moderation service is responding after 5 attempts" "WARNING"
        echo "⚠️ Could not verify Mock Moderation service is responding. Continuing anyway..."
        # We'll continue anyway since the process is running
    fi
done

log "✅ Mock Moderation service started successfully" "SUCCESS"
echo "✅ Mock Moderation service started successfully"
exit 0 