# Chat Service Deployment Scripts

This directory contains scripts to build, deploy, validate, and stop the Chat Service application.

## Overview

The scripts in this directory provide a comprehensive solution for deploying and managing the Chat Service application. They handle:

1. Checking prerequisites
2. Building the solution
3. Starting the NATS server with JetStream
4. Starting the Mock Moderation service
5. Starting the API service
6. Starting the Client Realtime service
7. Validating the deployment
8. Stopping all services

## Prerequisites

Before running these scripts, ensure you have:

- .NET SDK 8.0 or later
- NATS Server installed
- curl and jq installed
- Firebase service account file at `ChatService.API/firebase-service-account.json`

## Main Scripts

### `run_chat_service.sh`

The main orchestration script that runs all the other scripts in sequence. It:

1. Checks prerequisites
2. Builds the solution
3. Starts all services
4. Validates the deployment

Usage:
```bash
./scripts/run_chat_service.sh
```

### `stop_services.sh`

Stops all services started by the deployment script.

Usage:
```bash
./scripts/stop_services.sh
```

## Individual Scripts

These scripts are called by the main script, but can also be run individually:

- `check_prerequisites.sh`: Checks if all required tools and files are available
- `build_solution.sh`: Builds the .NET solution
- `start_nats.sh`: Starts the NATS server with JetStream enabled
- `start_mock_moderation.sh`: Starts the Mock Moderation service
- `start_api.sh`: Starts the API service
- `start_client_realtime.sh`: Starts the Client Realtime service
- `validate_deployment.sh`: Validates that all services are running correctly

## Logs

All scripts log their output to `scripts/chat_service_run.log`. Additionally, each service has its own log file:

- NATS server: `scripts/nats-server.log`
- Mock Moderation service: `scripts/mock-moderation.log`
- API service: `scripts/api.log`
- Client Realtime service: `scripts/client-realtime.log`

## Troubleshooting

If any script fails, check the log files for details. The main log file `scripts/chat_service_run.log` contains a summary of all operations.

Common issues:

1. **Port conflicts**: Ensure ports 4222, 8222, 5176, and 7110 are available
2. **Missing prerequisites**: Run `check_prerequisites.sh` to verify all requirements are met
3. **JetStream storage issues**: Check if the JetStream directory at `/tmp/nats-jetstream/jetstream` exists and is writable

## Configuration

The scripts use the following configuration:

- NATS server: Runs on port 4222 with monitoring on port 8222
- Mock Moderation service: Runs on port 7110
- API service: Runs on port 5176
- JetStream storage: Located at `/tmp/nats-jetstream/jetstream`

To modify these settings, edit the corresponding script files. 