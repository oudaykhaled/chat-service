#!/bin/bash

# Script to stop all services
# This script stops all services started by the deployment script

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
    echo "[$level] $message"
}

# Header
log "Stopping all services..." "INFO"

# Function to stop a service by PID file
stop_service_by_pid_file() {
    local pid_file="$1"
    local service_name="$2"
    
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        if ps -p "$pid" > /dev/null; then
            log "Stopping $service_name with PID $pid..." "INFO"
            kill -15 "$pid"
            sleep 2
            
            # Check if it's still running
            if ps -p "$pid" > /dev/null; then
                log "$service_name did not stop gracefully. Forcing termination..." "WARNING"
                kill -9 "$pid"
                sleep 1
            fi
            
            # Verify process is now stopped
            if ps -p "$pid" > /dev/null; then
                log "❌ Failed to stop $service_name with PID $pid" "ERROR"
                return 1
            else
                log "✅ $service_name stopped" "SUCCESS"
                rm "$pid_file"
                return 0
            fi
        else
            log "$service_name with PID $pid is not running" "INFO"
            rm "$pid_file"
            return 0
        fi
    else
        log "$service_name PID file not found" "INFO"
        return 0
    fi
}

# Function to stop a service by port
stop_service_by_port() {
    local port="$1"
    local service_name="$2"
    
    if lsof -i:"$port" >/dev/null 2>&1; then
        local pid=$(lsof -t -i:"$port")
        if [ -n "$pid" ]; then
            log "Stopping $service_name on port $port with PID $pid..." "INFO"
            kill -15 "$pid"
            sleep 2
            
            # Check if it's still running
            if lsof -i:"$port" >/dev/null 2>&1; then
                log "$service_name did not stop gracefully. Forcing termination..." "WARNING"
                kill -9 "$pid"
                sleep 1
            fi
            
            # Verify port is now available
            if lsof -i:"$port" >/dev/null 2>&1; then
                log "❌ Failed to stop $service_name on port $port" "ERROR"
                return 1
            else
                log "✅ $service_name stopped" "SUCCESS"
                return 0
            fi
        else
            log "No process found using port $port" "INFO"
            return 0
        fi
    else
        log "$service_name is not running on port $port" "INFO"
        return 0
    fi
}

# Function to stop a service by process name
stop_service_by_name() {
    local process_name="$1"
    local service_name="$2"
    
    local pids=$(ps aux | grep "$process_name" | grep -v grep | awk '{print $2}')
    if [ -n "$pids" ]; then
        for pid in $pids; do
            log "Stopping $service_name with PID $pid..." "INFO"
            kill -15 "$pid"
        done
        
        sleep 2
        
        # Check if any processes are still running
        pids=$(ps aux | grep "$process_name" | grep -v grep | awk '{print $2}')
        if [ -n "$pids" ]; then
            log "$service_name did not stop gracefully. Forcing termination..." "WARNING"
            for pid in $pids; do
                kill -9 "$pid"
            done
            sleep 1
        fi
        
        # Verify all processes are now stopped
        pids=$(ps aux | grep "$process_name" | grep -v grep | awk '{print $2}')
        if [ -n "$pids" ]; then
            log "❌ Failed to stop all $service_name processes" "ERROR"
            return 1
        else
            log "✅ $service_name stopped" "SUCCESS"
            return 0
        fi
    else
        log "$service_name is not running" "INFO"
        return 0
    fi
}

# Stop Client Realtime service
stop_service_by_pid_file "$ROOT_DIR/scripts/client-realtime.pid" "Client Realtime service"

# Also try to stop Client Realtime service by name
stop_service_by_name "ChatService.Client.Realtime" "Client Realtime service"

# Stop API service
stop_service_by_port 5176 "API service"
# Also try by PID file
stop_service_by_pid_file "$ROOT_DIR/scripts/api.pid" "API service"

# Stop Mock Moderation service
stop_service_by_port 7110 "Mock Moderation service"
# Also try by PID file
stop_service_by_pid_file "$ROOT_DIR/scripts/mock-moderation.pid" "Mock Moderation service"

# Stop NATS server
stop_service_by_port 4222 "NATS server"
# Also try by PID file
stop_service_by_pid_file "$ROOT_DIR/scripts/nats-server.pid" "NATS server"

# Final check to make sure all services are stopped
services_stopped=true

# Check if any service is still running on the required ports
check_port() {
    local port="$1"
    local service="$2"
    
    if lsof -i:"$port" >/dev/null 2>&1; then
        log "❌ $service is still running on port $port" "ERROR"
        services_stopped=false
    else
        log "✅ $service is not running on port $port" "SUCCESS"
    fi
}

# Check all ports
check_port 4222 "NATS Server"
check_port 8222 "NATS Monitoring"
check_port 5176 "API Service"
check_port 7110 "Mock Moderation Service"

# Check if Client Realtime service is still running
if ps aux | grep "ChatService.Client.Realtime" | grep -v grep > /dev/null; then
    log "❌ Client Realtime service is still running" "ERROR"
    services_stopped=false
else
    log "✅ Client Realtime service is not running" "SUCCESS"
fi

# Final status
if [ "$services_stopped" = true ]; then
    log "✅ All services stopped successfully" "SUCCESS"
    exit 0
else
    log "❌ Some services are still running" "ERROR"
    exit 1
fi 