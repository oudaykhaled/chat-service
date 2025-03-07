#!/bin/bash

# Main script to run and control the chat service
# This script orchestrates the entire process of building and running the chat service

# Set the base directory
BASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$BASE_DIR/.."
ROOT_DIR="$(pwd)"

# Set colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Log file
LOG_FILE="$ROOT_DIR/scripts/chat_service_run.log"
> "$LOG_FILE" # Clear log file

# Function to log messages
log() {
    local message="$1"
    local level="${2:-INFO}"
    local timestamp=$(date +"%Y-%m-%d %H:%M:%S")
    echo -e "[$timestamp] [$level] $message"
    echo "[$timestamp] [$level] $message" >> "$LOG_FILE"
}

# Function to run a script and check its exit code
run_script() {
    local script="$1"
    local description="$2"
    local continue_on_error="${3:-false}"
    
    log "Running: $description" "INFO"
    
    if [ -x "$script" ]; then
        "$script" >> "$LOG_FILE" 2>&1
        exit_code=$?
        
        if [ $exit_code -eq 0 ]; then
            log "✅ $description completed successfully" "SUCCESS"
            return 0
        else
            log "❌ $description failed with exit code $exit_code" "ERROR"
            if [ "$continue_on_error" = "true" ]; then
                log "⚠️ Continuing despite error in $description" "WARNING"
                return 0
            else
                return 1
            fi
        fi
    else
        log "❌ Script $script is not executable" "ERROR"
        return 1
    fi
}

# Make all scripts executable
chmod +x "$BASE_DIR"/*.sh

# Print header
echo -e "${YELLOW}==================================================${NC}"
echo -e "${YELLOW}      Chat Service Deployment Script              ${NC}"
echo -e "${YELLOW}==================================================${NC}"
log "Starting Chat Service deployment process" "INFO"

# Step 1: Check prerequisites
if ! run_script "$BASE_DIR/check_prerequisites.sh" "Checking prerequisites"; then
    log "Prerequisites check failed. Exiting." "ERROR"
    exit 1
fi

# Step 2: Build the solution
if ! run_script "$BASE_DIR/build_solution.sh" "Building solution"; then
    log "Solution build failed. Exiting." "ERROR"
    exit 1
fi

# Step 3: Configure and start NATS server
if ! run_script "$BASE_DIR/start_nats.sh" "Starting NATS server"; then
    log "NATS server start failed. Exiting." "ERROR"
    exit 1
fi

# Step 4: Start the Mock Moderation service
if ! run_script "$BASE_DIR/start_mock_moderation.sh" "Starting Mock Moderation service"; then
    log "Mock Moderation service start failed." "ERROR"
    log "Stopping NATS server before exiting." "INFO"
    "$BASE_DIR/stop_services.sh" >> "$LOG_FILE" 2>&1
    exit 1
fi

# Step 5: Start the API service
if ! run_script "$BASE_DIR/start_api.sh" "Starting API service"; then
    log "API service start failed." "ERROR"
    log "Stopping all services before exiting." "INFO"
    "$BASE_DIR/stop_services.sh" >> "$LOG_FILE" 2>&1
    exit 1
fi

# Step 6: Start the Client Realtime service
# Continue even if this fails
run_script "$BASE_DIR/start_client_realtime.sh" "Starting Client Realtime service" "true"

# Step 7: Validate the deployment
if ! run_script "$BASE_DIR/validate_deployment.sh" "Validating deployment" "true"; then
    log "Deployment validation failed." "WARNING"
    echo -e "${YELLOW}Deployment validation failed but services are running.${NC}"
    echo -e "${YELLOW}Check the log file for details: $LOG_FILE${NC}"
    echo -e "${YELLOW}To stop all services, run: ./scripts/stop_services.sh${NC}"
    exit 1
fi

# Success message
echo -e "${GREEN}==================================================${NC}"
echo -e "${GREEN}      Chat Service Deployment Successful!         ${NC}"
echo -e "${GREEN}==================================================${NC}"
log "Chat Service deployment completed successfully" "SUCCESS"
echo -e "All services are running. To stop them, run: ${YELLOW}./scripts/stop_services.sh${NC}"
echo -e "For detailed logs, check: ${YELLOW}$LOG_FILE${NC}"

exit 0 