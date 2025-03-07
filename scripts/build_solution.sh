#!/bin/bash

# Script to build the .NET solution
# This script builds all projects in the solution

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
log "Building solution..." "INFO"
echo "Building solution..."

# Clean the solution first
log "Cleaning solution..." "INFO"
dotnet clean
if [ $? -ne 0 ]; then
    log "❌ Failed to clean solution" "ERROR"
    echo "❌ Failed to clean solution"
    exit 1
fi
log "✅ Solution cleaned successfully" "SUCCESS"

# Restore packages
log "Restoring packages..." "INFO"
dotnet restore
if [ $? -ne 0 ]; then
    log "❌ Failed to restore packages" "ERROR"
    echo "❌ Failed to restore packages"
    exit 1
fi
log "✅ Packages restored successfully" "SUCCESS"

# Build the solution
log "Building solution..." "INFO"
dotnet build --configuration Debug
if [ $? -ne 0 ]; then
    log "❌ Failed to build solution" "ERROR"
    echo "❌ Failed to build solution"
    exit 1
fi
log "✅ Solution built successfully" "SUCCESS"

# Verify that all required binaries exist
check_binary() {
    local binary="$1"
    local project="$2"
    
    if [ -f "$binary" ]; then
        log "✅ Binary exists: $binary" "SUCCESS"
        return 0
    else
        log "❌ Binary not found: $binary" "ERROR"
        echo "❌ Binary not found: $binary"
        return 1
    fi
}

# Check for API binary
API_BINARY="$ROOT_DIR/ChatService.API/bin/Debug/net8.0/ChatService.API.dll"
if ! check_binary "$API_BINARY" "ChatService.API"; then
    exit 1
fi

# Check for Mock Moderation binary
MOCK_MODERATION_BINARY="$ROOT_DIR/ChatService.MockModeration/bin/Debug/net8.0/ChatService.MockModeration.dll"
if ! check_binary "$MOCK_MODERATION_BINARY" "ChatService.MockModeration"; then
    exit 1
fi

# Check for Client Realtime binary
CLIENT_REALTIME_BINARY="$ROOT_DIR/ChatService.Client.Realtime/bin/Debug/net8.0/ChatService.Client.Realtime.dll"
if ! check_binary "$CLIENT_REALTIME_BINARY" "ChatService.Client.Realtime"; then
    exit 1
fi

# All checks passed
log "All binaries built successfully" "SUCCESS"
echo "✅ Solution built successfully"
exit 0 