# Chat Service

This guide will help you set up and run the Chat Service, including configuring Firebase and running the necessary infrastructure using Docker Compose.

## Prerequisites

Before running the chat service, ensure you have the following installed:
- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)
- A Firebase account

## Setup

### 1. Register to Firebase
1. Go to [Firebase Console](https://console.firebase.google.com/).
2. Click **Add Project** and follow the setup instructions.
3. Once created, note your **Project ID** from the project settings.

### 2. Create a Firebase Service Account
1. Navigate to **Project Settings > Service Accounts**.
2. Click **Generate new private key**.
3. Download the generated JSON file.
4. Save the file as `firebase-service-account.json` in the root directory of the project.

### 3. Configure API Settings
1. Open the configuration file (`appsettings.json` or relevant config file).
2. Locate the following section:
   ```json
   "Firestore": {
     "ProjectId": "",
     "KeyFilePath": ""
   }
   ```
3. Fill in your Firebase **Project ID** and the path to the service account JSON file:
   ```json
   "Firestore": {
     "ProjectId": "your-project-id",
     "KeyFilePath": "firebase-service-account.json"
   }
   ```
4. Save the file.

### 4. Configure the Moderation Service
To use the moderation service, ensure that the MockModeration service is running, and configure the moderation settings as follows:

Run the MockModeration service (if not already running).
In your configuration file (appsettings.json), locate the Moderation section:
   ```json
	"Moderation": {
	  "EnableDispatcher": true,
	  "Pre": [
		""
	  ],
	  "Post": [
		""
	  ],
	  "Dispatcher": ""
	}
   ```
Fill in the appropriate API URLs for Pre, Post, and Dispatcher:
   ```json
	"Moderation": {
	  "EnableDispatcher": true,
	  "Pre": [
		"http://your-pre-api-url"
	  ],
	  "Post": [
		"http://your-post-api-url"
	  ],
	  "Dispatcher": "http://your-dispatcher-api-url"
	}
   ```
Save the configuration file.

### 5. Run the Infrastructure

1. Ensure your `firebase-service-account.json` file is correctly placed.
2. Open a terminal and navigate to the project directory.
3. Run the following command to start the infrastructure:

   ```sh
   docker compose up -d
   ```
   
4. To stop the services, run:

   ```sh
   docker compose down
   ```
  
5. To check logs, use:

   ```sh
   docker compose logs -f
   ```
  
This will spin up all required services using Docker Compose.

### 6. Docker Swarm Configuration
To run the Docker stack with Docker Swarm, follow these steps:

Ensure Docker Swarm Mode is Initialized
First, make sure Docker Swarm is initialized. If it's not, you can initialize it with:

```sh
docker swarm init
```

Deploy the Stack using Docker Compose
Deploy the stack with the following command:

```sh
docker stack deploy -c docker-compose.yml nats-stack
```

Remove the Stack
When you're done, you can remove the stack with:

```sh
docker stack rm nats-stack
```