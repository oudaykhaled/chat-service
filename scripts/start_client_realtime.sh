#!/bin/bash

# Script to start the Client Realtime service
# This script starts the Client Realtime service

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
log "Starting Client Realtime service..." "INFO"
echo "Starting Client Realtime service..."

# Check if the binary exists
CLIENT_REALTIME_BINARY="$ROOT_DIR/ChatService.Client.Realtime/bin/Debug/net8.0/ChatService.Client.Realtime.dll"
if [ ! -f "$CLIENT_REALTIME_BINARY" ]; then
    log "❌ Client Realtime binary not found at $CLIENT_REALTIME_BINARY" "ERROR"
    echo "❌ Client Realtime binary not found. Please build the solution first."
    exit 1
fi

# Check if API service is running
if ! lsof -i:5176 >/dev/null 2>&1; then
    log "❌ API service is not running on port 5176" "ERROR"
    echo "❌ API service is not running on port 5176. Please start it first."
    exit 1
fi

# Check if there's already a Client Realtime service running
CLIENT_REALTIME_PID_FILE="$ROOT_DIR/scripts/client-realtime.pid"
if [ -f "$CLIENT_REALTIME_PID_FILE" ]; then
    OLD_PID=$(cat "$CLIENT_REALTIME_PID_FILE")
    if ps -p "$OLD_PID" > /dev/null; then
        log "Client Realtime service is already running with PID $OLD_PID. Stopping it first..." "WARNING"
        echo "Client Realtime service is already running with PID $OLD_PID. Stopping it first..."
        
        kill -15 "$OLD_PID"
        sleep 2
        
        # Check if it's still running
        if ps -p "$OLD_PID" > /dev/null; then
            log "Process did not stop gracefully. Forcing termination..." "WARNING"
            kill -9 "$OLD_PID"
            sleep 1
        fi
        
        # Verify process is now stopped
        if ps -p "$OLD_PID" > /dev/null; then
            log "❌ Failed to stop Client Realtime service with PID $OLD_PID" "ERROR"
            echo "❌ Failed to stop Client Realtime service with PID $OLD_PID"
            exit 1
        fi
        
        log "✅ Previous Client Realtime service stopped" "SUCCESS"
    fi
fi

# Check for Firebase service account file
FIREBASE_SERVICE_ACCOUNT="$ROOT_DIR/ChatService.API/firebase-service-account.json"
if [ ! -f "$FIREBASE_SERVICE_ACCOUNT" ]; then
    log "❌ Firebase service account file not found at $FIREBASE_SERVICE_ACCOUNT" "ERROR"
    echo "❌ Firebase service account file not found. Please place it at ChatService.API/firebase-service-account.json"
    exit 1
fi

# Copy Firebase service account file to Client Realtime bin directory
CLIENT_REALTIME_BIN_DIR="$ROOT_DIR/ChatService.Client.Realtime/bin/Debug/net8.0"
log "Copying Firebase service account file to $CLIENT_REALTIME_BIN_DIR" "INFO"
cp "$FIREBASE_SERVICE_ACCOUNT" "$CLIENT_REALTIME_BIN_DIR/"
if [ $? -ne 0 ]; then
    log "❌ Failed to copy Firebase service account file" "ERROR"
    echo "❌ Failed to copy Firebase service account file"
    exit 1
fi
log "✅ Firebase service account file copied successfully" "SUCCESS"

# Clear the log file before starting
> "$ROOT_DIR/scripts/client-realtime.log"

# Start the Client Realtime service
log "Starting Client Realtime service..." "INFO"
cd "$ROOT_DIR/ChatService.Client.Realtime/bin/Debug/net8.0"
dotnet ChatService.Client.Realtime.dll > "$ROOT_DIR/scripts/client-realtime.log" 2>&1 &
CLIENT_REALTIME_PID=$!

# Save the PID to a file for later use
echo "$CLIENT_REALTIME_PID" > "$ROOT_DIR/scripts/client-realtime.pid"
log "Client Realtime service started with PID $CLIENT_REALTIME_PID" "INFO"

# Wait for the service to start
log "Waiting for Client Realtime service to start..." "INFO"
sleep 5

# Check if the process is still running
if ! ps -p "$CLIENT_REALTIME_PID" > /dev/null; then
    log "❌ Client Realtime service failed to start" "ERROR"
    echo "❌ Client Realtime service failed to start. Check logs at $ROOT_DIR/scripts/client-realtime.log"
    exit 1
fi

# Wait for the "Connected to Firestore" message to appear in the log
MAX_WAIT=30  # Maximum wait time in seconds
WAIT_INTERVAL=2  # Check interval in seconds
ELAPSED=0

while [ $ELAPSED -lt $MAX_WAIT ]; do
    if grep -q "Connected to Firestore" "$ROOT_DIR/scripts/client-realtime.log"; then
        log "✅ Client Realtime service connected to Firestore" "SUCCESS"
        echo "✅ Client Realtime service connected to Firestore"
        log "✅ Client Realtime service started successfully" "SUCCESS"
        echo "✅ Client Realtime service started successfully"
        exit 0
    fi
    
    sleep $WAIT_INTERVAL
    ELAPSED=$((ELAPSED + WAIT_INTERVAL))
    
    # Check if the process is still running
    if ! ps -p "$CLIENT_REALTIME_PID" > /dev/null; then
        log "❌ Client Realtime service process terminated unexpectedly" "ERROR"
        echo "❌ Client Realtime service process terminated unexpectedly. Check logs at $ROOT_DIR/scripts/client-realtime.log"
        exit 1
    fi
done

# If we get here, we timed out waiting for the "Connected to Firestore" message
if grep -q "Error" "$ROOT_DIR/scripts/client-realtime.log" || grep -q "Exception" "$ROOT_DIR/scripts/client-realtime.log"; then
    log "❌ Client Realtime service encountered an error" "ERROR"
    echo "❌ Client Realtime service encountered an error. Check logs at $ROOT_DIR/scripts/client-realtime.log"
    # Kill the process since it's not working correctly
    kill -15 "$CLIENT_REALTIME_PID"
    exit 1
else
    # The process is running but we didn't see the expected message
    # We'll assume it's working anyway
    log "⚠️ Could not verify Client Realtime service connected to Firestore within $MAX_WAIT seconds" "WARNING"
    echo "⚠️ Could not verify Client Realtime service connected to Firestore within $MAX_WAIT seconds"
    echo "⚠️ Check logs at $ROOT_DIR/scripts/client-realtime.log for details"
    log "✅ Client Realtime service started successfully (with warnings)" "SUCCESS"
    echo "✅ Client Realtime service started successfully (with warnings)"
    exit 0
fi 