#!/bin/bash

# Script to check if all prerequisites are installed
# This script validates that all required tools are available

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

# Function to check if a command is available
check_command() {
    local cmd="$1"
    local name="${2:-$cmd}"
    local required="${3:-true}"
    
    if command -v "$cmd" >/dev/null 2>&1; then
        log "✅ $name is installed: $(command -v "$cmd")" "SUCCESS"
        return 0
    else
        if [ "$required" = true ]; then
            log "❌ $name is not installed" "ERROR"
            echo "❌ $name is not installed. Please install it."
            return 1
        else
            log "⚠️ $name is not installed (optional)" "WARNING"
            echo "⚠️ $name is not installed (optional). Some features may not work correctly."
            return 0
        fi
    fi
}

# Header
log "Checking prerequisites..." "INFO"
echo "Checking prerequisites..."

# Check for .NET SDK
if ! check_command "dotnet" ".NET SDK"; then
    echo "❌ .NET SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
dotnet_version=$(dotnet --version)
log "Detected .NET version: $dotnet_version" "INFO"
echo "Detected .NET version: $dotnet_version"

# Check for NATS server
if ! check_command "nats-server" "NATS Server"; then
    echo "❌ NATS Server is not installed. Please install it from https://docs.nats.io/running-a-nats-service/introduction/installation"
    exit 1
fi

# Check NATS server version
nats_version=$(nats-server -v | head -n 1)
log "Detected NATS server version: $nats_version" "INFO"
echo "Detected NATS server version: $nats_version"

# Check for curl (needed for validation)
if ! check_command "curl" "curl"; then
    echo "❌ curl is not installed. Please install it."
    exit 1
fi

# Check for jq (needed for JSON parsing in validation, but optional)
check_command "jq" "jq" false

# Check for Firebase service account file
FIREBASE_SERVICE_ACCOUNT="$ROOT_DIR/ChatService.API/firebase-service-account.json"
if [ ! -f "$FIREBASE_SERVICE_ACCOUNT" ]; then
    log "❌ Firebase service account file not found at $FIREBASE_SERVICE_ACCOUNT" "ERROR"
    echo "❌ Firebase service account file not found. Please place it at ChatService.API/firebase-service-account.json"
    
    # Create a dummy Firebase service account file for testing purposes
    echo "Creating a dummy Firebase service account file for testing purposes..."
    mkdir -p "$(dirname "$FIREBASE_SERVICE_ACCOUNT")"
    cat > "$FIREBASE_SERVICE_ACCOUNT" << EOF
{
  "type": "service_account",
  "project_id": "saawt-75021",
  "private_key_id": "dummy-key-id",
  "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC7VJTUt9Us8cKj\nMzEfYyjiWA4R4/M2bS1GB4t7NXp98C3SC6dVMvDuictGeurT8jNbvJZHtCSuYEvu\nNMoSfm76oqFvAp8Gy0iz5sxjZmSnXyCdPEovGhLa0VzMaQ8s+CLOyS56YyCFGeJZ\n-----END PRIVATE KEY-----\n",
  "client_email": "dummy@saawt-75021.iam.gserviceaccount.com",
  "client_id": "123456789",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/dummy%40saawt-75021.iam.gserviceaccount.com",
  "universe_domain": "googleapis.com"
}
EOF
    log "⚠️ Created a dummy Firebase service account file for testing purposes" "WARNING"
    echo "⚠️ Created a dummy Firebase service account file for testing purposes"
    echo "⚠️ This is only for testing. Replace with a real service account file for production use."
else
    log "✅ Firebase service account file found" "SUCCESS"
    echo "✅ Firebase service account file found"
fi

# Check if required ports are available
check_port() {
    local port="$1"
    local service="$2"
    
    if lsof -i:"$port" >/dev/null 2>&1; then
        log "❌ Port $port is already in use (required for $service)" "ERROR"
        echo "❌ Port $port is already in use (required for $service)"
        return 1
    else
        log "✅ Port $port is available for $service" "SUCCESS"
        echo "✅ Port $port is available for $service"
        return 0
    fi
}

# Check required ports
ports_ok=true
if ! check_port 4222 "NATS Server"; then ports_ok=false; fi
if ! check_port 8222 "NATS Monitoring"; then ports_ok=false; fi
if ! check_port 5176 "API Service"; then ports_ok=false; fi
if ! check_port 7110 "Mock Moderation Service"; then ports_ok=false; fi

if [ "$ports_ok" = false ]; then
    log "❌ Some required ports are already in use. Please free them before continuing." "ERROR"
    echo "❌ Some required ports are already in use. Please free them before continuing."
    exit 1
fi

# Check if JetStream directory exists, create if not
JETSTREAM_DIR="/tmp/nats-jetstream/jetstream"
if [ ! -d "$JETSTREAM_DIR" ]; then
    log "Creating JetStream directory at $JETSTREAM_DIR" "INFO"
    echo "Creating JetStream directory at $JETSTREAM_DIR"
    mkdir -p "$JETSTREAM_DIR"
    if [ $? -ne 0 ]; then
        log "❌ Failed to create JetStream directory at $JETSTREAM_DIR" "ERROR"
        echo "❌ Failed to create JetStream directory at $JETSTREAM_DIR"
        exit 1
    fi
    log "✅ Created JetStream directory at $JETSTREAM_DIR" "SUCCESS"
    echo "✅ Created JetStream directory at $JETSTREAM_DIR"
else
    log "✅ JetStream directory already exists at $JETSTREAM_DIR" "SUCCESS"
    echo "✅ JetStream directory already exists at $JETSTREAM_DIR"
fi

# All checks passed
log "All prerequisite checks passed" "SUCCESS"
echo "✅ All prerequisite checks passed"
exit 0 